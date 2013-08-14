using System;
using System.Data.SQLite;
using NUnit.Framework;
using Overlook.Server.Storage.Sqlite;

namespace Overlook.Tests.Storage
{
    [TestFixture]
    public class SqliteStorageEngineTests : StorageEngineBaseTests
    {
        [SetUp]
        public void Setup()
        {
            _storageEngine = new SqliteStorageEngine(":memory:", true);
        }
    }
}
