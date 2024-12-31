using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using B3WebsiteBanHang.Models;
using B3WebsiteBanHang.Repository;
using SQLitePCL;
using B3WebsiteBanHang.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using NguyenMinhThang.Models;
using Newtonsoft.Json;

namespace B3WebsiteBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _context = context;
        }
// GET: Product------------------------------------------------------------------------------------
        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            //var products = await _productRepository.GetAllAsync();
            //return View(products);
            int pageSize = 5;
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);
            var paginatedProducts = await PaginatedList<Product>.CreateAsync(productsQuery,
            pageNumber, pageSize);
            return View(paginatedProducts);
        }
// GET: Product/Details/5------------------------------------------------------------------------------------
        [Authorize(Roles = "admin, user")]        
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
// GET: Product/Create------------------------------------------------------------------------------------
        [Authorize (Roles = "admin")]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }
        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageUrl)
        {
            if (ModelState.IsValid)
            {
                if (imageUrl != null)
                {
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            // Nếu ModelState không hợp lệ, hiển thị form với dữ liệu đã nhập
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }
// GET: Product/Edit/5------------------------------------------------------------------------------------
        [Authorize(Roles = "admin")]
        
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }
        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product,IFormFile imageUrl)
        {
            ModelState.Remove("ImageUrl"); 
            // Loại bỏ xác thực ModelState cho ImageUrl
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id); 
                // Giả định có phương thức GetByIdAsync                                                     
                // Giữ nguyên thông tin hình ảnh nếu không có hình mới được tải lên            
                if (imageUrl == null)

                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }
                else
                {
                    // Lưu hình ảnh mới
                    product.ImageUrl = await SaveImage(imageUrl);
                }
                // Cập nhật các thông tin khác của sản phẩm
                existingProduct.Name = product.Name;
                existingProduct.Price = product.Price;
                existingProduct.Description = product.Description;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.ImageUrl = product.ImageUrl;

                await _productRepository.UpdateAsync(existingProduct);

                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View(product);
        }
// GET: Product/Delete/5------------------------------------------------------------------------------------
        [Authorize(Roles = "admin")]       
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        // POST: Product/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
//------------------------------------SEARCH-----------------------------------------------
        [HttpGet]
        public async Task<IActionResult> SearchProducts(string query)
        {
            try
            {
                if(string.IsNullOrEmpty(query))
                    return BadRequest("Search query is required.");
                var result = await _context.Products.Where(p => p.Name.Contains(query) || 
                                                    (p.Description != null && 
                                                    p.Description.Contains(query))).ToListAsync();
                return View("Index", result);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
//---------------------------------PHÂN TRANG--------------------------------------------------
        public async Task<IActionResult> PagingNoLibrary(int pageNumber)
        {
            int pageSize = 10;
            IQueryable<Product> productsQuery = _context.Products.Include(p => p.Category);
            var pagedProducts = await productsQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(pagedProducts);
        }
        //------------------------------------------------------------------------------------
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName); 
            // Thay đổi đường dẫn theo cấu hình của bạn
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName; // Trả về đường dẫn tương đối
        }
        //---------------------------------Giỏ hàng--------------------------------------------------
        
    }
}
