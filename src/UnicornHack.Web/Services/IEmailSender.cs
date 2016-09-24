using System.Threading.Tasks;

namespace UnicornHack.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}