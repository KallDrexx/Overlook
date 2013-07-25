using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Overlook.Common.Data;
using Overlook.Common.Query;
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

        /// <summary>
        /// Queues a snapshot to be stored
        /// </summary>
        /// <param name="snapshot"></param>
        /// <param name="onSuccess">Delegate run when the snapshot is successfully saved</param>
        /// <param name="onFailure">Delegate run when the snapshot fails to save</param>
        public void StoreSnapshot(Snapshot snapshot, OperationSuccessDelegate onSuccess = null, OperationFailureDelegate onFailure = null)
        {
            if (snapshot == null)
                return; // Nothing to save

            var operation = new StoreSnapshotOperation {SnapshotToStore = snapshot};
            operation.SuccessCallback += onSuccess;
            operation.FailureCallback += onFailure;

            _operations.Enqueue(operation);
        }

        /// <summary>
        /// Queues a query for metrics
        /// </summary>
        /// <param name="query"></param>
        /// <param name="onSuccess">Delegate to run when the query has successfully completed</param>
        /// <param name="onFailure">Delegate to run when the query fails</param>
        /// <exception cref="ArgumentNullException">Thrown if a null query is passed in</exception>
        /// <exception cref="ArgumentException">Thrown if the query type is not known</exception>
        public void RunQuery(IQuery query, OperationSuccessDelegate onSuccess, OperationFailureDelegate onFailure)
        {
            if (query == null)
                throw new ArgumentNullException("query");

            IQueryOperation operation;
            if (query.GetType() == typeof (DateRangeQuery))
                operation = new DateRangeQueryOperation {RanQuery = query};
            else
                throw new ArgumentException("Query has an unknown type");

            operation.FailureCallback += onFailure;
            operation.SuccessCallback += onSuccess;
            _operations.Enqueue(operation);
        }

        private void HandleOperations()
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
