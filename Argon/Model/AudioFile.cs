using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Argon.Model
{
    public class AudioFile: MediaFile
    {
        private string album;
        public string Album
        {
            get { return this.album; }
            set { this.album = value; }
        }
    }
}
