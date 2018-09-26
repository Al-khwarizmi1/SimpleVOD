using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SimpleVOD.Models;

namespace SimpleVOD.Controllers
{
    public class HomeController : Controller
    {
        private readonly VideoIndex videoIndex;

        private VideoConfiguration options;

        public HomeController(VideoIndex videoIndex, IOptions<VideoConfiguration> options)
        {
            this.videoIndex = videoIndex;
            this.options = options.Value;
        }

        public IActionResult Index(string id = null)
        {
            var nodes = new List<VideoNode>();
            VideoNode parent = null;

            if (id != null)
            {
                parent = videoIndex.FindNode(id);
            }

            if (parent == null)
            {
                nodes = videoIndex.RootNodes;
            }
            else
            {
                if (parent.Children.Any())
                {
                    nodes = parent.Children;
                }
                else
                {
                    var path = Path.Combine(options.RootDirectory, videoIndex.FindNode(id).GetFullPath());
                    if (System.IO.File.Exists(path))
                    {
                        return View("Play", parent);
                    }
                }
            }

            return View(nodes);
        }

        public IActionResult Play(string id)
        {
            return View(videoIndex.FindNode(id));
        }

        public async Task<FileStreamResult> GetVideo(string id)
        {
            var path = Path.Combine(options.RootDirectory, videoIndex.FindNode(id).GetFullPath());
            if (path != null)
            {
                return File(System.IO.File.OpenRead(path), "video/mp4");
            }
            else
            {
                return null;
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
