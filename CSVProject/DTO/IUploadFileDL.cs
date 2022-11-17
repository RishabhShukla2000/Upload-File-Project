using CSVProject.Model;

namespace CSVProject.DTO
{
    public interface IUploadFileDL
    {
        public Task<UploadFileResponse> UploadFile(UploadFileRequest request, string Path);
    }
}
