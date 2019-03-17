using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Media.Playlists;

namespace Argon.Model
{
    class PlaylistFile
    {
        private string name;
        private string path;
        private Playlist myplaylist;

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

        public Playlist Myplaylist
        {
            get { return this.myplaylist; }
            set { this.myplaylist = value; }
        }
    }
}