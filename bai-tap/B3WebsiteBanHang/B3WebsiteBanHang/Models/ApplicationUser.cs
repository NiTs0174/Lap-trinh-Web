using Microsoft.AspNetCore.Identity;

namespace B3WebsiteBanHang.Models
{
    public class ApplicationUser : IdentityUser
    {
        public  string FullName { get; set; }
        public  string Address { get; set; }
    }
}
