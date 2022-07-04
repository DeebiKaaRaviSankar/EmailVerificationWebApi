using EmailService.Models;
using Users.Models;
namespace EmailService.Models{
public interface IMailService
{
    public Task sendEmailAsync(User user,string subject,string body);
} 
}