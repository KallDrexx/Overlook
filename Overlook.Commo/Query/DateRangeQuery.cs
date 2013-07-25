using System;

namespace Overlook.Common.Query
{
    public class DateRangeQuery : IQuery
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public QueryResult[] Results { get; set; }
    }
}
