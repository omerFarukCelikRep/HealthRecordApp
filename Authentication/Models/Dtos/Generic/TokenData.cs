using System.ComponentModel.DataAnnotations;

namespace Authentication.Models.Dtos.Generic
{
    public class TokenData
    {
        [Required]
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
