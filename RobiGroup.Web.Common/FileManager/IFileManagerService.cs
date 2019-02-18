using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RobiGroup.Web.Common.FileManager
{
    public interface IFileManagerService
    {
        IEnumerable<FileManagerItem> GetFileManagerItems(string currentDirectory);

        string GetBreadcrumbs(string currentDirectory);

        void CreateDirectory(string directoryPath);

        void DeleteDirectory(string directoryPath);

        void DeleteFile(string filePath);

        void BulkDelete(IEnumerable<string> deletePaths);

        string GetFileDownloadPath(string directoryPath);

        void UploadFile(string dir, IFormFile formFile);
    }
}
