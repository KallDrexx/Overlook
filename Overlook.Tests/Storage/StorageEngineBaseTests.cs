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
    }
}
