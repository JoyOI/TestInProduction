using System.Collections.Generic;

namespace JoyOI.TestInProduction.Models
{
    public class ApiResult<T>
    {
        public int code { get; set; }

        public string msg { get; set; }

        public T data { get; set; }
    }

    public class PagedResult<T>
    {
        public int current { get; set; }

        public int total { get; set; }

        public int count { get; set; }

        public int size { get; set; }

        public IEnumerable<T> result { get; set; }
    }
}
