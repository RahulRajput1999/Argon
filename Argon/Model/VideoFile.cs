using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Argon.Model
{
    class VideoFile: MediaFile
    {
        private Image thumb;
        private string name;
        private string path;
        
        public Image Thumb
        {
            get { return thumb; }
            set { this.thumb = value; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }
        public string Path
        {
            get { return path; }
            set { this.path = value; }
        }

    }
}
