using System.Threading.Tasks;

namespace UnicornHack.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}