namespace NomNomGo.IdentityService.Application.Models
{
    /// <summary>
    /// Модель для пагинированного списка
    /// </summary>
    /// <typeparam name="T">Тип элементов списка</typeparam>
    public class PaginatedList<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int PageNumber { get; }
        public int TotalPages { get; }
        public int TotalCount { get; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedList(IReadOnlyList<T> items, int count, int pageNumber, int pageSize)
        {
            Items = items;
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            TotalCount = count;
        }
    }
}