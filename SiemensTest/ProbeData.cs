using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiemensTest
{
    public class ProbeData : IProbeData
    {
        public int DbOffset { get; private set; }
        public double Measurement { get; set; }
        public double Microns { get; set; }
        public double Saturation { get; set; }
        public double IntegrationTime { get; set; }
        public double DistanceToTarget { get; set; }
        public double CrossWebPosition { get; set; }
        public short ReadStatus { get; set; }
        public double CycleAverage { get; set; }
        public double CycleAverageMicrons { get; set; }
        public double CycleMin { get; set; }
        public double CycleMax { get; set; }
        public double CycleStdDev { get; set; }
        public short CycleTotalCount { get; set; }
        public short CycleEvalCount { get; set; }
        public DateTime LastUpdated { get; set; }

        public ProbeData()
        {

        }

    }
}
