using Microsoft.Extensions.Primitives;

namespace CSVProject.Model
{
    public class UploadFileRequest
    {
        public IFormFile File { get; set; }
    }

    public class UploadFileResponse
    { 
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
    public class ExcelParameter
    {
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public int Age { get; set; }
    }
}
