using Windows.Storage;
using Windows.Storage.Streams;

namespace SubLib
{
    public class MovieInfo
    {
        public string sublanguageid;
        public string moviehash;
        public double moviebytesize;

        public MovieInfo(string hex, double length, string languages)
        {
            this.moviehash = hex;
            this.sublanguageid = languages;
            this.moviebytesize = length;
        }


    }
}