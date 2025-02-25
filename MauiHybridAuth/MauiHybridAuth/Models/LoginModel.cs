﻿using System.ComponentModel.DataAnnotations;

namespace MauiHybridAuth.Models
{
    public class LoginModel
    {
        [Required]
        [Display(Name = "Email Address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string LoginFailureMessage { get; set; } = "Invalid Email or Password. Please try again.";
        
    }
}
