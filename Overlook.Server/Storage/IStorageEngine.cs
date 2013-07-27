using System;
using Overlook.Common.Data;

namespace Overlook.Server.Storage
{
    public interface IStorageEngine : IDisposable
    {
        void StoreSnapshot(Snapshot snapshot1);
        Snapshot[] GetSnapshots(DateTime startSearchDate, DateTime endSearchDate);
    }
}
