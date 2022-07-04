using System;
using System.Collections.Generic;

namespace Users.Models
{
    public partial class Book
    {
        public int Id { get; set; }
        public string Title{get;set;}=null!;
        public string Description{get;set;}=null!;
        public string Author{get;set;} = null!;
        public string Ratings{get;set;}=null!;
        public string Genre { get; set; } = null!;
        

    }
}
