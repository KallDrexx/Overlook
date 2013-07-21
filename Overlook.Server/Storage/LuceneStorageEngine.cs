using System;
using Overlook.Common.Data;
using Overlook.Common.Search;

namespace Overlook.Server.Storage
{
    public class LuceneStorageEngine : IStorageEngine
    {
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        public void StoreSnapshot(Snapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public Snapshot[] Search(DateRangeSearch search)
        {
            throw new NotImplementedException();
        }
    }
}
