using Overlook.Common.Data;

namespace Overlook.Server.Operations
{
    public class StoreSnapshotOperation : IOperation
    {
        public Snapshot SnapshotToStore { get; set; }

        public OperationSuccessDelegate SuccessCallback { get; set; }
        public OperationFailureDelegate FailureCallback { get; set; }
    }
}
