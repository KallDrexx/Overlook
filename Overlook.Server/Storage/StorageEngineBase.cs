using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Overlook.Common.Data;
using Overlook.Common.Search;
using Overlook.Server.Operations;

namespace Overlook.Server.Storage
{
    public abstract class StorageEngineBase
    {
        private readonly ConcurrentQueue<IOperation> _operations; 
        private readonly Task _operationHandlerTask;
        private readonly CancellationTokenSource _cancellationTokenSource;

        protected StorageEngineBase()
        {
            _operations = new ConcurrentQueue<IOperation>();
            _cancellationTokenSource = new CancellationTokenSource();

            _operationHandlerTask = new Task(HandleOperations, _cancellationTokenSource.Token);
            _operationHandlerTask.Start();
        }

        public void StoreSnapshot(Snapshot snapshot, OperationSuccessDelegate onSuccess = null, OperationFailureDelegate onFailure = null)
        {
            if (snapshot == null)
                return; // Nothing to save

            var operation = new StoreSnapshotOperation {SnapshotToStore = snapshot};
            operation.SuccessCallback += onSuccess;
            operation.FailureCallback += onFailure;

            _operations.Enqueue(operation);
        }

        public void HandleOperations()
        {
            const int noOperationRecheckMillisecondDelay = 500;

            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                IOperation operation;
                if (!_operations.TryDequeue(out operation))
                {
                    Thread.Sleep(noOperationRecheckMillisecondDelay);
                }
                else
                {
                    var exception = ProcessOperation(operation);
                    if (exception == null)
                    {
                        if (operation.SuccessCallback != null)
                            operation.SuccessCallback(operation);
                    }
                    else
                    {
                        if (operation.FailureCallback != null)
                            operation.FailureCallback(operation, exception);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the passed in operation
        /// </summary>
        /// <param name="operation"></param>
        /// <returns>
        /// Returns null if no exceptions occurred (success), otherwise returns the exception
        /// that caused the failure
        /// </returns>
        protected abstract Exception ProcessOperation(IOperation operation);
    }
}
