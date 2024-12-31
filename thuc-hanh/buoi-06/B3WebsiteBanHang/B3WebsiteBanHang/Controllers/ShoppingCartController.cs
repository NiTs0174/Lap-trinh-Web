using B3WebsiteBanHang.DataAccess;
using B3WebsiteBanHang.Models;
using B3WebsiteBanHang.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Newtonsoft.Json;
using NguyenMinhThang.Models;
using NguyenMinhThang.Repositories;
using System.Globalization;

namespace NguyenMinhThang.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProductRepository _productRepository;

        public ShoppingCartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IProductRepository productRepository)
        {
            _context = context;
            _userManager = userManager;
            _productRepository = productRepository;
        }
        //-------------------------------------------------------------------
        //public async Task<IActionResult> AddToCart(int productId, int quantity)
        //{
        //    // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
        //    Product product = await GetProductFromDatabase(productId);

        //    var cartItem = new CartItem
        //    {
        //        Id = productId,
        //        Name = product.Name,
        //        //----------------------
        //        Image = product.ImageUrl,
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
            return RedirectToAction("Index", "ShoppingCart");
        }
        //----------------------------------------------
        public IActionResult Index()
        {
            //var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            //return View(cart);
            //----------------
            var carts = GetCartItems();
            ViewBag.TongTien = carts.Sum(p => p.Price * p.Quantity);
            ViewBag.TongSoLuong = carts.Sum(p => p.Quantity);
            return View(carts);
        }
        //-----------------------------------------------------------
        public ActionResult UpdateCart(int id, int quantity)
        {
            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id == id);
            if (findCartItem != null)
            {
                findCartItem.Quantity = quantity;
                SaveCartSession(carts);
            }
            return RedirectToAction("Index");
        }
        //-----------------------------------------------------------
        public ActionResult DeleteCart(int id)
        {
            var carts = GetCartItems();
            var findCartItem = carts.FirstOrDefault(p => p.Id == id);
            if (findCartItem != null)
            {
                carts.Remove(findCartItem);
                SaveCartSession(carts);
            }
            return RedirectToAction("Index");
        }
        //-----------------------------------------------------------
        // Các actions khác...
        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            return View(new Order());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            //var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            var carts = GetCartItems();

            //xác thực giỏ hàng
            if (carts == null || !carts.Any())
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
            //decimal totalPrice = cart.Items.Sum(i => i.Price * i.Quantity);

            //Lưu thông tin đơn hàng vào cơ sở dữ liệu
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = carts.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = carts.Select(i => new OrderDetail
            {
                ProductId = i.Id,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            //Xóa Giỏ hàng sau khi thanh toán
            HttpContext.Session.Remove("Cart");

            // Trang xác nhận hoàn thành đơn hàng
            return View("OrderCompleted", order.Id);
        }
        //-------------------------------------
        //[HttpPost]
        //public async Task<IActionResult> Order()
        //{
        //    try
        //    {
        //        var carts = GetCartItems();

        //        var user = await _userManager.GetUserAsync(User);

        //        var order = new Order
        //        {
        //            UserId = user != null ? user.Id : null,
        //            OrderDate = DateTime.UtcNow,
        //            TotalPrice = carts.Sum(i => i.Price * i.Quantity),
        //            OrderDetails = carts.Select(item => new OrderDetail
        //            {
        //                ProductId = item.Id,
        //                Quantity = item.Quantity,
        //                Price = item.Price
        //            }).ToList()
        //        };

        //        _context.Orders.Add(order);
        //        await _context.SaveChangesAsync();

        //        //xóa session giỏ hàng
        //        HttpContext.Session.Remove("Cart"); //CARTKEY

        //        return View("OrderCompleted", order);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //----------------------------------------------------------------------------------------------
        private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            //return _context.Products.FirstOrDefault(p => p.Id == productId);
            return await _productRepository.GetByIdAsync(productId);
        }
        //-------------------------------------
        List<CartItem>? GetCartItems()
        {
            string jsoncart = HttpContext.Session.GetString("cart");
            if (jsoncart != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(jsoncart);
            }
            return new List<CartItem>();
        }
        //-------------------------------------
        void SaveCartSession(List<CartItem> ls)
        {
            string jsoncart = JsonConvert.SerializeObject(ls);
            HttpContext.Session.SetString("cart", jsoncart);
        }
        
    }
}
