using System;
using Overlook.Common.Data;

namespace Overlook.Server.Storage
{
    public interface IStorageEngine : IDisposable
    {
        /// <summary>
        /// Stores a snapshot in the storage engine
        /// </summary>
        /// <param name="snapshot"></param>
        /// <exception cref="ArgumentNullException">Thrown when a null snapshot is provided</exception>
        void StoreSnapshot(Snapshot snapshot);

        /// <summary>
        /// Retrieves all snapshots between the specified time period
        /// </summary>
        /// <param name="startSearchDate"></param>
        /// <param name="endSearchDate"></param>
        /// <returns></returns>
        Snapshot[] GetSnapshots(DateTime startSearchDate, DateTime endSearchDate);
    }
}
