using Talabat.APlS.DTOs;

namespace Talabat.APlS.Helpers
{
    public class Pagination<T>
    {

        public Pagination(int pageIndex, int pageSize, IReadOnlyList<T> data,int count)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            Data = data;
            Count = count;
        }

        public int PageIndex { get; set; } //= 1;
        public int PageSize { get; set; } //= 10;
        public int Count { get; set; }
        public IReadOnlyList<T> Data { get; set; }// = new List<T>();
    }
}
