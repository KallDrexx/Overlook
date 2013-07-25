using Overlook.Common.Query;

namespace Overlook.Server.Operations
{
    public class DateRangeQueryOperation : IQueryOperation
    {
        public OperationSuccessDelegate SuccessCallback { get; set; }
        public OperationFailureDelegate FailureCallback { get; set; }
        public IQuery RanQuery { get; set; }
    }
}
