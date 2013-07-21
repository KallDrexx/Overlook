using Overlook.Common.Data;
using Overlook.Common.Search;

namespace Overlook.Server.Storage
{
    public interface IStorageEngine
    {
        void Initialize();
        void StoreSnapshot(Snapshot snapshot);
        Snapshot[] Search(DateRangeSearch search);
    }
}
