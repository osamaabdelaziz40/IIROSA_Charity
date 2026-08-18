namespace Framework.Core.Data.Paging.Request
{
    public class SortingModel
    {
        public string? Column { get; set; }
        public bool IsDescending { get; set; } = true;
    }
}
