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

            var result = new FileStreamResult(stream, "text/plain");
            result.FileDownloadName = filename;

            return result;
        }

    }
}
