using System;

namespace Overlook.Server.Operations
{
    public delegate void OperationSuccessDelegate(IOperation operation);
    public delegate void OperationFailureDelegate(IOperation operation, Exception exception);

    public interface IOperation
    {
        OperationSuccessDelegate SuccessCallback { get; set; }
        OperationFailureDelegate FailureCallback { get; set; }
    }
}
