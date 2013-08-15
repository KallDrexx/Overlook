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
                    var device = hardware.Name;
                    var category = sensor.SensorType.ToString();
                    var name = sensor.Name;
                    var value = Convert.ToDecimal(sensor.Value);

                    // TODO: Set suffix based on sensor type
                    var metric = new Metric(device, category, name, "");
                    yield return new KeyValuePair<Metric, decimal>(metric, value);
                }
            }
        }
    }
}
