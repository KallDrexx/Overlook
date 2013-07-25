using Overlook.Common.Query;

namespace Overlook.Server.Operations
{
    public interface IQueryOperation : IOperation
    {
        IQuery RanQuery { get; set; } 
    }
}
