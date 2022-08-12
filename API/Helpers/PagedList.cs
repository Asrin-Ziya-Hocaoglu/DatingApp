using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers
{
    public class PagedList<T>: List<T>
    {
        

        public PagedList(IEnumerable<T> items ,int count , int pageNumber,  int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count/(double) pageSize); // 11/5 = 2,20 üste yuvarla => 3 = toplam olması gereken sayfa sayısı
            PageSize = pageSize;
            TotalCount = count;
            // AddRange(items);
            AddRange(items);
        }

        public int CurrentPage { get; set; }
        
        public int TotalPages  { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }


        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source , int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber-1) * pageSize).Take(pageSize).ToListAsync(); // 2-1 = 1 * 5 = take 5 obj from source 

            return new PagedList<T>(items,count,pageNumber,pageSize);
            

        }




        
    }
}