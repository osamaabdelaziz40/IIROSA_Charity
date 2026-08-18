namespace Framework.Core.Data.Paging.Request
{
    public class PagingModel
    {
        private int _pageNumber { get; set; }
        public int PageNumber
        {
            get
            {
                return _pageNumber;
            }
            set
            {
                if (value <= 0)
                    value += 1;

                _pageNumber = value;
            }
        }
        private int _pageSize { get; set; }
        public int PageSize 
        {
            get
            {
                return _pageSize;
            }
            set
            {
                if (value <= 0)
                    value += 1;

                _pageSize = value;
            }
        }
    }
}
