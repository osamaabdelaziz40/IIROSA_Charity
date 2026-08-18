using PagedList.Core;

namespace Framework.Core.Data.Paging.Request
{
    public class SearchDto<T>:PagingDto where T : new()
    {
        public T Filter { get; set; }
        public StaticPagedList<T> Items { get; set; }
        public SortingModel Sorting { get; set; }

    }
    public class SearchDto<T,S> :PagingDto where T : new()
    {
        public T Filter { get; set; }
        public StaticPagedList<S> Items { get; set; }
    }
    public class SimpleSearchDto<T> where T : new()
    {
        public T Filter { get; set; }
        public PagingModel Paging { get; set; }
        public SortingModel Sorting { get; set; }
        public ExportingModel Exporting { get; set; }

    }
}
