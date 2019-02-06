using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Argon.Model
{
    public class MediaFile
    {
        private string title;
        private string name;
        private string path;
        private Image thumb;

        public string Title
        {
            get { return this.title; }
            set { this.title = value; }
        }
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string Path
        {
            get { return this.path; }
            set { this.path = value; }
        }

        public Image Thumb
        {
            get { return this.thumb; }
            set { this.thumb = value; }
        }
    }
}
