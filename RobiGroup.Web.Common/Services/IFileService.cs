using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RobiGroup.Web.Common.Services
{
    public interface IFileService
    {
        string Thumbnail(string path, int size);

        string FilePathToURL(string path);

        Task<string> Save(IFormFile file, string path);
    }
}