using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;

namespace NguyenMinhThang.Models
{
    public class CartItem
    {
        //Product Ìnfỏrmation-----
        public int Id { get; set; }
        public string Name { get; set; }
        //------------------
        public string? Image { get; set; }
        public decimal Price { get; set; }
        //Quatity Info---------------------
        public int Quantity { get; set; }
    }
}
