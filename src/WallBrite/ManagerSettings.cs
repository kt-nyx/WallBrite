using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallBrite
{
    public class ManagerSettings
    {
        public int UpdateIntervalHours { get; set; }
        public int UpdateIntervalMins { get; set; }
        public DateTime BrightestTime { get; set; }
        public DateTime DarkestTime { get; set; }
        public bool StartsOnStartup { get; set; }
        public string WallpaperStyle { get; set; }
    }
}
