using Microsoft.EntityFrameworkCore;
namespace RenewOne.Application.Common.Pagination
{
    public static class IQueryableExtensions
    {
        public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
        {
            var totalRecords = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResponse<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize)
            };
        }
    }
}
