﻿using System.ComponentModel.DataAnnotations;

namespace Authentication.Models.Dtos.Incoming
{
    public class UserRegistrationRequestDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
