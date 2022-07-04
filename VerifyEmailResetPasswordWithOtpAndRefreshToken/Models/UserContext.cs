using Users.Models;
using Microsoft.EntityFrameworkCore;
namespace Users.Models{
    public class UserContext:DbContext{
        public UserContext(DbContextOptions<UserContext> options): base(options){

        }

        public DbSet<User> users {get;set;}
        public virtual DbSet<Book> books{get;set;}
    }
}