using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESRI.ArcGIS.Geometry;

namespace Phonatech3
{
    public class Tower
    {
        /// <summary>
        /// 
        /// 
        /// </summary>
        public string ID { get; set; }
        public string towerType { get; set; }
        public string networkBand { get; set; }
        public double towerCost { get; set; }
        public double towerCoverage { get; set; }
        public double towerHeight { get; set; }
        public double towerBaseArea { get; set; }
        public IPoint towerLocation { get; set; }
    }
}
