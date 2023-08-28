using System.Collections.Generic;

namespace TomPIT.Data.Storage;
public class QueryArgs
{
    public List<OrderByDescriptor> OrderBy { get; set; }

    public PagingArgs Paging { get; set; }

    public static QueryArgs Default => new()
    {
        Paging = new PagingArgs
        {
            Index = 0,
            Size = 10
        }
    };

    public static QueryArgs NoPaging => new()
    {
        Paging = new PagingArgs
        {
            Index = 0,
            Size = int.MaxValue
        }
    };

    public QueryArgs()
    {
        OrderBy = new List<OrderByDescriptor>();
        Paging = new PagingArgs();
    }
}
