using System.Threading.Tasks;

namespace RobiGroup.Web.Common.Services
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}
