using System;
using System.ComponentModel.DataAnnotations;

namespace Users.Models{
    public class UserRegisterDTO{
        [Required,EmailAddress]
        public string? Email{get;set;}
        [Required,MinLength(6,ErrorMessage ="Minimum Length must be atleast 6 characters!"),MaxLength(14)]
        public string? Password{get;set;}
        [Required]
        public string? Role{get;set;}
        [Required,Compare("Password")]
        public string? ConfirmPassword{get;set;}


    }
}