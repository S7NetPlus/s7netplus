using System;

namespace SiemensTest
{
    public interface IProbeData
    {
        double Saturation { get; set; }
        double CrossWebPosition { get; set; }
        double CycleAverage { get; set; }
        double CycleAverageMicrons { get; set; }
        short CycleEvalCount { get; set; }
        double CycleMax { get; set; }
        double CycleMin { get; set; }
        double CycleStdDev { get; set; }
        short CycleTotalCount { get; set; }
        double DistanceToTarget { get; set; }
        double IntegrationTime { get; set; }
        DateTime LastUpdated { get; set; }
        double Measurement { get; set; }
        double Microns { get; set; }
        short ReadStatus { get; set; }
    }
}