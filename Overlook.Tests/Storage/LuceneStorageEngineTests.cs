using NUnit.Framework;
using Overlook.Server.Storage;

namespace Overlook.Tests.Storage
{
    [TestFixture]       
    public class LuceneStorageEngineTests : StorageEngineBaseTests
    {
        [SetUp]
        public void Setup()
        {
            _storageEngine = new LuceneStorageEngine();
        }
    }
}
