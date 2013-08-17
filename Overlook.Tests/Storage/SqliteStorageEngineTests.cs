using Microsoft.VisualStudio.TestTools.UnitTesting;
using Overlook.Server.Storage.Sqlite;

namespace Overlook.Tests.Storage
{
    [TestClass]
    public class SqliteStorageEngineTests : StorageEngineBaseTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            _storageEngine = new SqliteStorageEngine(":memory:", true);
        }
    }
}
