using NUnit.Framework;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    [TestFixture]
    public class SqliteStorageEngineTests : StorageEngineBaseTests
    {
        [SetUp]
        public void Setup()
        {
            _storageEngine = new SqliteStorageEngine("test", true);
        }
    }
}
