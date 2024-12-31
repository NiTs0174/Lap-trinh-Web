using B3WebsiteBanHang.DataAccess;
using B3WebsiteBanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Newtonsoft.Json;
using NguyenMinhThang.Models;
using NguyenMinhThang.Repositories;
using NguyenMinhThang.Session;

namespace NguyenMinhThang.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ShoppingCartController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //    public IActionResult AddToCart(int productId, int quantity)
        //{
        //    // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
        //    var product = GetProductFromDatabase(productId);
        //    var cartItem = new CartItem
        //    {
        //        Id = productId,
        //        Name = product.Name,
        //        Price = product.Price,
        //        Quantity = quantity
        //    };
        //    var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
        //    cart.AddItem(cartItem);
        //    HttpContext.Session.SetObjectAsJson("Cart", cart);
        //    return RedirectToAction("Index");
        //}
        public async Task<ActionResult> AddToCart(int id)
        {
            Product? itemProduct = _context.Products.FirstOrDefault(p => p.Id == id);
            if (itemProduct == null)
                return BadRequest("Sản phẩm không tồn tại");
            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id.Equals(id));
            if (findCartItem == null)
            {
                //Th thêm mới vào giỏ hàng
                findCartItem = new CartItem()
                {
                    Id = itemProduct.Id,
                    Name = itemProduct.Name,
                    Image = itemProduct.ImageUrl,
                    Price = itemProduct.Price,
                    Quantity = 1
                };
                carts.Add(findCartItem);
            }
            else
                findCartItem.Quantity++;
            SaveCartSession(carts);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Index()
        {
            //var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            //return View();
            var carts = GetCartItems();
            ViewBag.TongTien = carts.Sum(p => p.Price * p.Quantity);
            ViewBag.TongSoLuong = carts.Sum(p => p.Quantity);
            return View(carts);
        }

        // Các actions khác...
        private Product GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            return _context.Products.FirstOrDefault(p => p.Id == productId);
        }

        public IActionResult Checkout()
        {
            return View(new Order());
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            //xác thực giỏ hàng
            if (cart == null || !cart.Items.Any())
            {
                // Xử lý giỏ hàng trống...
                return RedirectToAction("Index");
            }
            // Xác thực người dùng
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account"); // Hoặc xử lý theo cách khác
            }
            // Tính toán tổng giá trị đơn hàng
            decimal totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            //Lưu thông tin đơn hàng vào cơ sở dữ liệu
            //var user = await _userManager.GetUserAsync(User);
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.Id,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
            //_context.Orders.Add(order);
            await _context.SaveChangesAsync();
            //Xóa Giỏ hàng sau khi thanh toán
            HttpContext.Session.Remove("Cart");
            // Trang xác nhận hoàn thành đơn hàng
            return View("OrderCompleted", order.Id);
        }
        //------------------------------
        List<CartItem>? GetCartItems()
        {
            string jsoncart = HttpContext.Session.GetString("cart");
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
            }
            return new List<CartItem>();
        }
        void SaveCartSession(List<CartItem> ls)
        {
            string jsoncart = JsonConvert.SerializeObject(ls);
            HttpContext.Session.SetString("cart"
            , jsoncart);
        }
        //----------------------------
        
    }
}
