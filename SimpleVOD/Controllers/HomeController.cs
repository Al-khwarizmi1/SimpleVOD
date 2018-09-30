using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
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

        public FileStreamResult GetVideoMp4(string id)
        {
            var path = GetFilePath(id);

            if (path.EndsWith(".mp4"))
            {
                return File(System.IO.File.OpenRead(path), "video/mp4");
            }
            else
            {
                var args = string.Format(options.ffmpeg.toMp4, path);

                return File(GetFFmpegStream(args), "video/mp4");
            }
        }

        public FileStreamResult GetVideoOgg(string id)
        {
            var path = GetFilePath(id);

            var args = string.Format(options.ffmpeg.toOgg, path);

            return File(GetFFmpegStream(args), "video/ogg");
        }

        public FileStreamResult GetVideoWebM(string id)
        {
            var path = GetFilePath(id);

            var args = string.Format(options.ffmpeg.toWebM, path);

            return File(GetFFmpegStream(args), "video/webm");
        }

        private string GetFilePath(string id)
        {
            var path = Path.Combine(options.RootDirectory, videoIndex.FindNode(id).GetFullPath());
            if (path == null)
            {
                throw new FileNotFoundException("Requested file not found");
            }
            return path;
        }

        private Stream GetFFmpegStream(string arguments)
        {
            try
            {
                var p = new Process();
                var sti = p.StartInfo;
                sti.CreateNoWindow = true;
                sti.UseShellExecute = false;
                sti.FileName = options.ffmpeg.Path;
                sti.Arguments = arguments;
                sti.LoadUserProfile = false;
                sti.RedirectStandardInput = true;
                sti.RedirectStandardOutput = true;
                p.Start();

                if (WaitProcessToStart(p, TimeSpan.FromMinutes(1)) == false)
                {
                    throw new TimeoutException("Unable to start ffmepg");
                }

                return p.StandardOutput.BaseStream;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private bool WaitProcessToStart(Process p, TimeSpan timeout)
        {
            var waitStart = DateTime.Now;

            while (true)
            {
                if (DateTime.Now - waitStart > timeout)
                {
                    return false;
                }

                try
                {
                    var time = p.StartTime;
                    return true;
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                }
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
