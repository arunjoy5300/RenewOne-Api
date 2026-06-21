namespace RenewOne.Application.Common.Pagination
{
    public class PagedRequest
    {
        private const int MaxPageSize = 100;

        private int _pageNumber = 1;
        private int _pageSize = 4;

        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = Math.Max(1, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = Math.Clamp(value <= 0 ? 4 : value, 1, MaxPageSize);
        }
    }
}