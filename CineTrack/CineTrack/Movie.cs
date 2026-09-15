using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CineTrack
{
    public class Movie
    {
        public string Title { get; set; }
        public string Genre { get; set; }
        public int Length { get; set; }       // Movie time in minutes
        public double Rating { get; set; }     // Star rating (1-5)
        public bool IsWatched { get; set; }    // True if checked off, False if not
    }
}
