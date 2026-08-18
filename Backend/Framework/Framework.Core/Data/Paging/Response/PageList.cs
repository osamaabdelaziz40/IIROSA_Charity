using System.Collections.Generic;

namespace Framework.Core.Data.Paging.Response
{
    public class PageList<T>
    {
        public PageList()
        {
            DataList = new List<T>();
        }
        public PageList(List<T> dataList, int totalCount)
        {
            DataList = dataList;
            TotalCount = totalCount;
        }
        public List<T> DataList { get; private set; }
        public int TotalCount { get; private set; }
    }
}
