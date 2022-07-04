using System;
using System.ComponentModel.DataAnnotations;
namespace Users.Models{
    public class UserLoginDTO{

        [Required,EmailAddress]
        public string? Email{get;set;}
        [Required]
        public string? Password{get;set;}
        
    }
}