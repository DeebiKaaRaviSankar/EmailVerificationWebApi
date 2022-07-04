using System;
namespace Users.Models{
    public class User{
        public int Id{get;set;}
        public string? Email{get;set;}
        public string? Role{get;set;}
        public byte[]? PasswordHash{get;set;}
        public byte[]? PasswordSalt{get;set;}

        public string? RefreshToken{get;set;}
        public DateTime tokenCreated{get;set;} = DateTime.Now;
        public DateTime tokenExpires{get;set;} = DateTime.Now;
        public string? VerificationToken{get;set;}
        public DateTime? VerifiedAt{get;set;}
        public string? ResetPasswordToken{get;set;}
        public DateTime? ResetTokenExpiresAt{get;set;}

    }
}