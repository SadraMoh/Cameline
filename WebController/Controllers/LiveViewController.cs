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
        public async Task<FileStreamResult> Live()
        {
            if (camSv.LiveImageStream == null) throw new FileNotFoundException();

            var stream = camSv.LiveImageStream;

            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                bytes = memoryStream.ToArray();
            }

            //string base64 = Convert.ToBase64String(bytes);
            //new MemoryStream(System.Text.Encoding.UTF8.GetBytes(base64));

            var result = new FileStreamResult(new MemoryStream(bytes), "image/jpg");
            result.FileDownloadName = CameraService.LIVE_KEY;

            return result;
        }

    }

}

