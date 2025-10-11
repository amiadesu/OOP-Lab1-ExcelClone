using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExcelClone.Services.GoogleDrive;

public interface IGoogleDriveService
{
    public bool IsSignedIn { get; }
    public string? Email { get; }

    public Task Init();
    public Task SignIn();
    public Task<List<Google.Apis.Drive.v3.Data.File>> GetFiles(string? fileExtension = null);
    public Task<string> DownloadFileAsync(string fileId);
    public Task<Google.Apis.Drive.v3.Data.File> UploadFileAsync(string fileName, Stream content, string mimeType);
    public Task<Google.Apis.Drive.v3.Data.File> UploadOrReplaceFileAsync(string fileName, Stream content, string mimeType);
    public Task SignOut();
}