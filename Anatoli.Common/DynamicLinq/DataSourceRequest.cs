using System.Collections.Generic;

namespace Anatoli.Common.DynamicLinq
{
    public class DataSourceRequest
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Take { get; set; }
        public int Skip { get; set; }

        public IEnumerable<Sort> Sort { get; set; }
        public Filter Filter { get; set; }
        public IEnumerable<Filter> Filters { get; set; }
       
    }
}
