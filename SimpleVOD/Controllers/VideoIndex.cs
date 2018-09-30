using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleVOD.Controllers
{
    public class VideoIndex
    {
        private VideoConfiguration options;

        public List<VideoNode> RootNodes { get; set; }

        private List<VideoNode> flatList;

        public VideoIndex(IOptions<VideoConfiguration> options)
        {
            this.options = options.Value;

            RootNodes = new List<VideoNode>();
            flatList = new List<VideoNode>();

            PopulateVideoNodes();
        }

        public VideoNode FindNode(string id)
        {
            return flatList.FirstOrDefault(x => string.Equals(x.Id, id, StringComparison.InvariantCulture));
        }

        private void PopulateVideoNodes()
        {
            var root = options.RootDirectory;
            var rootDirs = Directory.GetDirectories(root);

            foreach (var dir in rootDirs)
            {
                var node = new VideoNode() { Title = dir.Split('\\').Last() };
                TraverseChildren(node);
                RootNodes.Add(node);
                flatList.Add(node);
            }
        }

        private bool ValidFileExtension(string fileName)
        {
            return options.SupportedFormats.Any(x => fileName.ToLowerInvariant().EndsWith(x.ToLowerInvariant()));
        }

        private void TraverseChildren(VideoNode parent)
        {
            var dirs = Directory.GetDirectories(Path.Combine(options.RootDirectory, parent.GetFullPath()));

            AddFiles(parent);

            foreach (var dir in dirs)
            {
                var child = new VideoNode() { Title = dir.Split('\\').Last() };

                parent.Children.Add(child);
                child.Parent = parent;
                flatList.Add(child);

                TraverseChildren(child);
            }
        }

        private void AddFiles(VideoNode node)
        {
            var dir = Path.Combine(options.RootDirectory, node.GetFullPath());

            var files = Directory.GetFiles(dir);
            foreach (var file in files.Where(f => ValidFileExtension(f)))
            {
                var video = new VideoNode()
                {
                    Title = file.Split('\\').Last(),
                    Parent = node,
                };

                node.Children.Add(video);
                flatList.Add(video);
            }
        }
    }

    public class VideoNode
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<VideoNode> Children { get; set; }
        public VideoNode Parent { get; set; }

        public VideoNode()
        {
            Id = Guid.NewGuid().ToString();
            Children = new List<VideoNode>();
        }

        public string GetFullPath()
        {
            return Path.Combine(GetBreadCrumb().Select(x => x.Title).ToArray());
        }

        public List<VideoNode> GetBreadCrumb()
        {
            if (Parent == null)
            {
                return new List<VideoNode> { this };
            }
            else
            {
                var parents = Parent.GetBreadCrumb();
                parents.Add(this);
                return parents;
            }
        }
    }
}
