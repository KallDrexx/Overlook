using System;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;
using Overlook.Common.Data;

namespace Overlook.Server.MetricRetriever
{
    public class OpenHardwareMonitorMetricRetriever : IMetricRetriever
    {
        private readonly Computer _computer;

        public OpenHardwareMonitorMetricRetriever()
        {
            _computer = new Computer();
            _computer.Open();

            // Enable sensors
            _computer.HDDEnabled = true;
            _computer.RAMEnabled = true;
            _computer.GPUEnabled = true;
            _computer.MainboardEnabled = true;
            _computer.CPUEnabled = true;
        }

        public IEnumerable<KeyValuePair<Metric, decimal>> GetCurrentMetricValues()
        {
            // Loop through all the sensors
            foreach (var hardware in _computer.Hardware)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    
                }
            }

            return null;
        }
    }
}
