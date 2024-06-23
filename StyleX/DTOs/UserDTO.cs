using System.ComponentModel.DataAnnotations;

namespace StyleX.DTOs
{
    public class LoginModel
    {
        public string email { get; set; } = null!;
        public string password { get; set; } = null!;
    }
}
