using System;
namespace RefreshTokens{
    public class RefreshToken{
        public string refreshtoken{get;set;}
        public DateTime Created{get;set;} = DateTime.Now;
        public DateTime Expired{get;set;}

        
    }
}