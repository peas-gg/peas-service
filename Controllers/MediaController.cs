using Microsoft.AspNetCore.Mvc;
using PEAS.Services;

namespace PEAS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpPost("image")]
        public ActionResult<Uri> Image([FromBody] byte[] imageData)
        {
            var response = _mediaService.UploadImage(imageData);
            return Ok(response);
        }
    }
}