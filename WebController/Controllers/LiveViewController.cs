using Microsoft.AspNetCore.Mvc;
using WebController.Services;

namespace WebController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LiveViewController : Controller
    {
        private readonly IWebHostEnvironment environment;
        private readonly CameraService camSv;

        public LiveViewController(IWebHostEnvironment environment, CameraService camSv)
        {
            this.environment = environment;
            this.camSv = camSv;
        }

        [HttpGet]
        public JsonResult Index()
        {
            return Json("Welcome to the API");
        }

        [HttpGet("[action]")]
        public FileStreamResult GetImage(string filename)
        {
            if (string.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            string path = Path.Combine(camSv.ImageSaveDirectory, filename);

            var stream = new FileStream(path, FileMode.Open);

            var result = new FileStreamResult(stream, "image/jpg");
            result.FileDownloadName = filename;

            return result;
        }

        [HttpGet("[action]")]
        public async Task<FileStreamResult> Live(long cameraId)
        {
            if (!camSv.StreamingCameras.TryGetValue(cameraId, out Stream? stream))
                throw new FileNotFoundException("Camera not found");

            if (stream == null) throw new FileNotFoundException("Stream is null"); 

            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                bytes = memoryStream.ToArray();
            }

            //string base64 = Convert.ToBase64String(bytes);
            //new MemoryStream(System.Text.Encoding.UTF8.GetBytes(base64));

            //var memoryStream = new MemoryStream(bytes);
            //stream.CopyTo(memoryStream);

            var result = new FileStreamResult(new MemoryStream(bytes), "image/jpg");
            result.FileDownloadName = $"{CameraService.LIVE_KEY}_{cameraId}_{DateTime.Now.Ticks}";

            return result;
        }

    }

}

