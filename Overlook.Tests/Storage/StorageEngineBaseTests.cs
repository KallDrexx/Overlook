﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Overlook.Common.Data;
using Overlook.Common.Queries;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    public class StorageEngineBaseTests
    {
        protected IStorageEngine _storageEngine;

        [Test]
        public void Can_Save_And_Retrieve_Snapshot_Metrics()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string suffix = "suffix";
            const decimal metric1Value = 123.4m;

            var metric1 = new Metric(device, category, name1, suffix);

            var snapshot1 = new Snapshot
            {
                Date = DateTime.Now,
                MetricValues = new KeyValuePair<Metric, decimal>[]
                {
                    new KeyValuePair<Metric, decimal>(metric1, metric1Value), 
                }
            };

            _storageEngine.StoreSnapshot(snapshot1);

            var query = new Query
            {
                StartDate = snapshot1.Date.AddMinutes(-1),
                EndDate = snapshot1.Date.AddMinutes(1),
                Metrics = new[] {metric1}
            };

            var results = _storageEngine.ExecuteQuery(query);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(1, resultsArray.Length, "Incorrect number of results were returned");
            Assert.AreEqual(metric1, resultsArray[0].Metric, "Returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "Returned metric values array was null");
            Assert.AreEqual(1, resultsArray[0].Values.Length, "Incorrect number of metric values returned");
            Assert.AreEqual(snapshot1.Date, resultsArray[0].Values[0].Key, "Incorrect metric date");
            Assert.AreEqual(metric1Value, resultsArray[0].Values[0].Value, "Incorrect metric value");
        }

        [Test]
        public void Can_Return_Multiple_Metric_Type_Values()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string name2 = "name2";
            const string suffix = "suffix";
            const decimal metric1Value = 123.4m;
            const decimal metric2Value = 50m;

            var metric1 = new Metric(device, category, name1, suffix);
            var metric2 = new Metric(device, category, name2, suffix);

            var snapshot1 = new Snapshot
            {
                Date = DateTime.Now,
                MetricValues = new KeyValuePair<Metric, decimal>[]
                {
                    new KeyValuePair<Metric, decimal>(metric1, metric1Value), 
                    new KeyValuePair<Metric, decimal>(metric2, metric2Value), 
                }
            };

            _storageEngine.StoreSnapshot(snapshot1);

            var query = new Query
            {
                StartDate = snapshot1.Date.AddMinutes(-1),
                EndDate = snapshot1.Date.AddMinutes(1),
                Metrics = new[] { metric1, metric2 }
            };

            var results = _storageEngine.ExecuteQuery(query);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(2, resultsArray.Length, "Incorrect number of results were returned");

            Assert.AreEqual(metric1, resultsArray[0].Metric, "First returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "First returned metric values array was null");
            Assert.AreEqual(1, resultsArray[0].Values.Length, "Incorrect number of metric values returned for first metric");
            Assert.AreEqual(snapshot1.Date, resultsArray[0].Values[0].Key, "Incorrect metric date for first metric");
            Assert.AreEqual(metric1Value, resultsArray[0].Values[0].Value, "Incorrect metric value for first metric");

            Assert.AreEqual(metric2, resultsArray[1].Metric, "Second returned metric was not correct");
            Assert.IsNotNull(resultsArray[1].Values, "Second returned metric values array was null");
            Assert.AreEqual(1, resultsArray[1].Values.Length, "Incorrect number of metric values returned for second metric");
            Assert.AreEqual(snapshot1.Date, resultsArray[1].Values[0].Key, "Incorrect metric date for second metric");
            Assert.AreEqual(metric2Value, resultsArray[1].Values[0].Value, "Incorrect metric value for second metric");
        }

        [Test]
        public void Multiple_Values_Returned_In_Order_Of_Snapshot_Date()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string suffix = "suffix";
            const decimal metricValue1 = 123.4m;
            const decimal metricValue2 = 50m;

            var metric1 = new Metric(device, category, name1, suffix);

            var snapshot1 = new Snapshot
            {
                Date = DateTime.Now,
                MetricValues = new KeyValuePair<Metric, decimal>[]
                {
                    new KeyValuePair<Metric, decimal>(metric1, metricValue1), 
                }
            };

            var snapshot2 = new Snapshot
            {
                Date = DateTime.Now.AddMinutes(-5),
                MetricValues = new KeyValuePair<Metric, decimal>[]
                {
                    new KeyValuePair<Metric, decimal>(metric1, metricValue2), 
                }
            };

            _storageEngine.StoreSnapshot(snapshot1);
            _storageEngine.StoreSnapshot(snapshot2);

            var query = new Query
            {
                StartDate = snapshot2.Date.AddMinutes(-1),
                EndDate = snapshot1.Date.AddMinutes(1),
                Metrics = new[] { metric1 }
            };

            var results = _storageEngine.ExecuteQuery(query);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(1, resultsArray.Length, "Incorrect number of results were returned");

            Assert.AreEqual(metric1, resultsArray[0].Metric, "Returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "Returned metric values array was null");
            Assert.AreEqual(2, resultsArray[0].Values.Length, "Incorrect number of metric values returned");

            Assert.AreEqual(snapshot2.Date, resultsArray[0].Values[0].Key, "Incorrect metric date for first result");
            Assert.AreEqual(metricValue2, resultsArray[0].Values[0].Value, "Incorrect metric value for first result");

            Assert.AreEqual(snapshot1.Date, resultsArray[0].Values[1].Key, "Incorrect metric date for second result");
            Assert.AreEqual(metricValue1, resultsArray[0].Values[1].Value, "Incorrect metric value for second result");
        }

        [Test]
        public void Query_Results_Returned_As_Average_Per_Minute()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string suffix = "suffix";
            const decimal metricValue1 = 0m;
            const decimal metricValue2 = 50m;
            const decimal metricValue3 = 75m;
            const decimal expectedAverage = (metricValue1 + metricValue2)/2;

            var metric1 = new Metric(device, category, name1, suffix);

            var snapshot1 = new Snapshot
            {
                Date = new DateTime(2013, 1, 1, 1, 1, 15),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue1) }
            };

            var snapshot2 = new Snapshot
            {
                Date = snapshot1.Date.AddSeconds(30),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue2) }
            };

            var snapshot3 = new Snapshot
            {
                Date = snapshot1.Date.AddMinutes(1),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue3) }
            };

            _storageEngine.StoreSnapshot(snapshot1);
            _storageEngine.StoreSnapshot(snapshot2);
            _storageEngine.StoreSnapshot(snapshot3);

            var query = new Query
            {
                StartDate = snapshot1.Date.AddMinutes(-5),
                EndDate = snapshot3.Date.AddMinutes(5),
                Metrics = new[] { metric1 },
                Resolution = QueryResolution.Minute
            };

            var results = _storageEngine.ExecuteQuery(query);
            var expectedDate1 = snapshot1.Date.AddSeconds(-snapshot1.Date.Second);
            var expectedDate2 = snapshot3.Date.AddSeconds(-snapshot3.Date.Second);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(1, resultsArray.Length, "Incorrect number of results were returned");

            Assert.AreEqual(metric1, resultsArray[0].Metric, "Returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "Returned metric values array was null");
            Assert.AreEqual(2, resultsArray[0].Values.Length, "Incorrect number of metric values returned");

            Assert.AreEqual(expectedDate1, resultsArray[0].Values[0].Key, "Incorrect metric date for first result");
            Assert.AreEqual(expectedAverage, resultsArray[0].Values[0].Value, "Incorrect metric value for first result");

            Assert.AreEqual(expectedDate2, resultsArray[0].Values[1].Key, "Incorrect metric date for second result");
            Assert.AreEqual(metricValue3, resultsArray[0].Values[1].Value, "Incorrect metric value for second result");
        }

        [Test]
        public void Query_Results_Returned_As_Average_Per_Hour()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string suffix = "suffix";
            const decimal metricValue1 = 0m;
            const decimal metricValue2 = 50m;
            const decimal metricValue3 = 75m;
            const decimal expectedAverage = (metricValue1 + metricValue2) / 2;

            var metric1 = new Metric(device, category, name1, suffix);

            var snapshot1 = new Snapshot
            {
                Date = new DateTime(2013, 1, 1, 1, 15, 0),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue1) }
            };

            var snapshot2 = new Snapshot
            {
                Date = snapshot1.Date.AddMinutes(30),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue2) }
            };

            var snapshot3 = new Snapshot
            {
                Date = snapshot1.Date.AddHours(1),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue3) }
            };

            _storageEngine.StoreSnapshot(snapshot1);
            _storageEngine.StoreSnapshot(snapshot2);
            _storageEngine.StoreSnapshot(snapshot3);

            var query = new Query
            {
                StartDate = snapshot1.Date.AddMinutes(-5),
                EndDate = snapshot3.Date.AddMinutes(5),
                Metrics = new[] { metric1 },
                Resolution = QueryResolution.Hour
            };

            var results = _storageEngine.ExecuteQuery(query);
            var expectedDate1 = snapshot1.Date.AddMinutes(-snapshot1.Date.Minute);
            var expectedDate2 = snapshot3.Date.AddMinutes(-snapshot3.Date.Minute);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(1, resultsArray.Length, "Incorrect number of results were returned");

            Assert.AreEqual(metric1, resultsArray[0].Metric, "Returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "Returned metric values array was null");
            Assert.AreEqual(2, resultsArray[0].Values.Length, "Incorrect number of metric values returned");

            Assert.AreEqual(expectedDate1, resultsArray[0].Values[0].Key, "Incorrect metric date for first result");
            Assert.AreEqual(expectedAverage, resultsArray[0].Values[0].Value, "Incorrect metric value for first result");

            Assert.AreEqual(expectedDate2, resultsArray[0].Values[1].Key, "Incorrect metric date for second result");
            Assert.AreEqual(metricValue3, resultsArray[0].Values[1].Value, "Incorrect metric value for second result");
        }

        [Test]
        public void Query_Results_Returned_As_Average_Per_Day()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string suffix = "suffix";
            const decimal metricValue1 = 0m;
            const decimal metricValue2 = 50m;
            const decimal metricValue3 = 75m;
            const decimal expectedAverage = (metricValue1 + metricValue2) / 2;

            var metric1 = new Metric(device, category, name1, suffix);

            var snapshot1 = new Snapshot
            {
                Date = new DateTime(2013, 1, 1, 10, 0, 0),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue1) }
            };

            var snapshot2 = new Snapshot
            {
                Date = snapshot1.Date.AddHours(12),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue2) }
            };

            var snapshot3 = new Snapshot
            {
                Date = snapshot1.Date.AddHours(24),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue3) }
            };

            _storageEngine.StoreSnapshot(snapshot1);
            _storageEngine.StoreSnapshot(snapshot2);
            _storageEngine.StoreSnapshot(snapshot3);

            var query = new Query
            {
                StartDate = snapshot1.Date.AddDays(-1),
                EndDate = snapshot3.Date.AddDays(1),
                Metrics = new[] { metric1 },
                Resolution = QueryResolution.Day
            };

            var results = _storageEngine.ExecuteQuery(query);
            var expectedDate1 = snapshot1.Date.AddHours(-snapshot1.Date.Hour);
            var expectedDate2 = snapshot3.Date.AddHours(-snapshot3.Date.Hour);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(1, resultsArray.Length, "Incorrect number of results were returned");

            Assert.AreEqual(metric1, resultsArray[0].Metric, "Returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "Returned metric values array was null");
            Assert.AreEqual(2, resultsArray[0].Values.Length, "Incorrect number of metric values returned");

            Assert.AreEqual(expectedDate1, resultsArray[0].Values[0].Key, "Incorrect metric date for first result");
            Assert.AreEqual(expectedAverage, resultsArray[0].Values[0].Value, "Incorrect metric value for first result");

            Assert.AreEqual(expectedDate2, resultsArray[0].Values[1].Key, "Incorrect metric date for second result");
            Assert.AreEqual(metricValue3, resultsArray[0].Values[1].Value, "Incorrect metric value for second result");
        }

        [Test]
        public void Query_Results_Returned_As_Average_Per_Month()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string suffix = "suffix";
            const decimal metricValue1 = 0m;
            const decimal metricValue2 = 50m;
            const decimal metricValue3 = 75m;
            const decimal expectedAverage = (metricValue1 + metricValue2) / 2;

            var metric1 = new Metric(device, category, name1, suffix);

            var snapshot1 = new Snapshot
            {
                Date = new DateTime(2013, 1, 10, 0, 0, 0),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue1) }
            };

            var snapshot2 = new Snapshot
            {
                Date = snapshot1.Date.AddDays(15),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue2) }
            };

            var snapshot3 = new Snapshot
            {
                Date = snapshot1.Date.AddDays(25),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue3) }
            };

            _storageEngine.StoreSnapshot(snapshot1);
            _storageEngine.StoreSnapshot(snapshot2);
            _storageEngine.StoreSnapshot(snapshot3);

            var query = new Query
            {
                StartDate = snapshot1.Date.AddDays(-1),
                EndDate = snapshot3.Date.AddDays(1),
                Metrics = new[] { metric1 },
                Resolution = QueryResolution.Month
            };

            var results = _storageEngine.ExecuteQuery(query);
            var expectedDate1 = snapshot1.Date.AddDays(-snapshot1.Date.Day + 1);
            var expectedDate2 = snapshot3.Date.AddDays(-snapshot3.Date.Day + 1);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(1, resultsArray.Length, "Incorrect number of results were returned");

            Assert.AreEqual(metric1, resultsArray[0].Metric, "Returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "Returned metric values array was null");
            Assert.AreEqual(2, resultsArray[0].Values.Length, "Incorrect number of metric values returned");

            Assert.AreEqual(expectedDate1, resultsArray[0].Values[0].Key, "Incorrect metric date for first result");
            Assert.AreEqual(expectedAverage, resultsArray[0].Values[0].Value, "Incorrect metric value for first result");

            Assert.AreEqual(expectedDate2, resultsArray[0].Values[1].Key, "Incorrect metric date for second result");
            Assert.AreEqual(metricValue3, resultsArray[0].Values[1].Value, "Incorrect metric value for second result");
        }

        [Test]
        public void Query_Results_Returned_As_Average_Per_Year()
        {
            const string device = "device";
            const string category = "category";
            const string name1 = "name1";
            const string suffix = "suffix";
            const decimal metricValue1 = 0m;
            const decimal metricValue2 = 50m;
            const decimal metricValue3 = 75m;
            const decimal expectedAverage = (metricValue1 + metricValue2) / 2;

            var metric1 = new Metric(device, category, name1, suffix);

            var snapshot1 = new Snapshot
            {
                Date = new DateTime(2013, 5, 1, 0, 0, 0),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue1) }
            };

            var snapshot2 = new Snapshot
            {
                Date = snapshot1.Date.AddMonths(6),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue2) }
            };

            var snapshot3 = new Snapshot
            {
                Date = snapshot1.Date.AddMonths(12),
                MetricValues = new[] { new KeyValuePair<Metric, decimal>(metric1, metricValue3) }
            };

            _storageEngine.StoreSnapshot(snapshot1);
            _storageEngine.StoreSnapshot(snapshot2);
            _storageEngine.StoreSnapshot(snapshot3);

            var query = new Query
            {
                StartDate = snapshot1.Date.AddDays(-1),
                EndDate = snapshot3.Date.AddDays(1),
                Metrics = new[] { metric1 },
                Resolution = QueryResolution.Year
            };

            var results = _storageEngine.ExecuteQuery(query);
            var expectedDate1 = snapshot1.Date.AddMonths(-snapshot1.Date.Month + 1);
            var expectedDate2 = snapshot3.Date.AddMonths(-snapshot3.Date.Month + 1);

            Assert.IsNotNull(results, "Null results returned");

            var resultsArray = results.ToArray();
            Assert.AreEqual(1, resultsArray.Length, "Incorrect number of results were returned");

            Assert.AreEqual(metric1, resultsArray[0].Metric, "Returned metric was not correct");
            Assert.IsNotNull(resultsArray[0].Values, "Returned metric values array was null");
            Assert.AreEqual(2, resultsArray[0].Values.Length, "Incorrect number of metric values returned");

            Assert.AreEqual(expectedDate1, resultsArray[0].Values[0].Key, "Incorrect metric date for first result");
            Assert.AreEqual(expectedAverage, resultsArray[0].Values[0].Value, "Incorrect metric value for first result");

            Assert.AreEqual(expectedDate2, resultsArray[0].Values[1].Key, "Incorrect metric date for second result");
            Assert.AreEqual(metricValue3, resultsArray[0].Values[1].Value, "Incorrect metric value for second result");
        }
    }
}
