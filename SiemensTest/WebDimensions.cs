using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTest
{
    public class WebDimensions
    {
        public float WebWidth { get; set; }

        public float EdgeTrim { get; set; }

        public short LaneCount { get; set; }

        public bool IndependentEdgerimLanes { get; set; }

        public WebDimensions()
        {

        }
    }
}
