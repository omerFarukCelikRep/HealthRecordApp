using System.ComponentModel.DataAnnotations;

namespace Authentication.Models.Dtos.Incoming
{
    public class TokenRequestDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
