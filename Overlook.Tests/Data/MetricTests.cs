using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Overlook.Common.Data;

namespace Overlook.Tests.Data
{
    [TestClass]
    public class MetricTests
    {
        [TestMethod]
        public void Can_Create_Parsable_String_From_Metric_Details()
        {
            var metric = new Metric("device", "category", "name", "suffix");

            const string expectedResult = "(device|category|name|suffix)";
            var actualResult = metric.ToParsableString();

            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        public void Can_Create_Metric_From_Parsable_Metric_String()
        {
            const string parsableString = "(device|category|name|suffix)";
            var expectedMetric = new Metric("device", "category", "name", "suffix");
            var actualMetric = Metric.Create(parsableString);
            Assert.AreEqual(expectedMetric, actualMetric);
        }

        [TestMethod]
        public void Null_Metric_Returned_When_Created_With_Null_String()
        {
            var metric = Metric.Create(null);
            Assert.IsNull(metric, "Metric was not null");
        }

        [TestMethod]
        public void Null_Metric_Returned_When_Less_Than_Four_Parts_Given()
        {
            var metric = Metric.Create("(one|two|three)");
            Assert.IsNull(metric, "Metric was not null");
        }

        [TestMethod]
        public void Null_Metric_Returned_When_More_Than_Four_Parts_Given()
        {
            var metric = Metric.Create("(one|two|three|four|five)");
            Assert.IsNull(metric, "Metric was not null");
        }
    }
}
