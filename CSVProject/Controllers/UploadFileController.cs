using CSVProject.DTO;
using CSVProject.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace CSVProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadFileController : ControllerBase
    {
        public readonly IUploadFileDL _uploadFileDL;
        public UploadFileController(IUploadFileDL uploadFileDL) 
        {
            _uploadFileDL = uploadFileDL;
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request) 
        {
            UploadFileResponse response = new UploadFileResponse();
            string Path = "UploadFileFolder/" + request.File.FileName;
            try 
            {
                using (FileStream stream = new FileStream(Path, FileMode.CreateNew))
                { 
                    await request.File.CopyToAsync(stream);
                }
                response =await _uploadFileDL.UploadFile(request,Path);
            }

            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return Ok(response);
        }

    }
}
