using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorPro
{
    //地图配置属性
    public class MapConfigAttribute
    {
        public int Zoom { get; set; }
        public int MinZoom { get; set; }
        public int MaxZoom { get; set; }
        public string MapProvider { get; set; }
        public string MapName { get; set; }

        public string Center { get; set; }
        public string MapBound { get; set; }

        public MapConfigAttribute Clone()
        {
            return this.MemberwiseClone() as MapConfigAttribute;
        }
    }
}
