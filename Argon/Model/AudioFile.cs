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
        private string artist;
        private List<string> genre;
        public string Album
        {
            get { return this.album; }
            set { this.album = value; }
        }
        public string Artist
        {
            get { return this.artist; }
            set { this.artist = value; }
        }
        public List<string> Genre
        {
            get { return genre; }
            set { this.genre = value; }
        }
    }
}
