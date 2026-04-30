using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSA.Utils
{
    public static class MapperService
    {
        /// <summary>
        /// Map từ 1 Object sang 1 Object (Copy instance và thay đổi hình thái)
        /// </summary>
        public static TDestination MapTo<TSource, TDestination>(this TSource source)
        {
            if (source == null) return default;
            return source.Adapt<TDestination>();
        }

        /// <summary>
        /// Map từ 1 Object đã có sẵn sang 1 Object khác (Update instance)
        /// </summary>
        public static TDestination MapTo<TSource, TDestination>(this TSource source, TDestination destination)
        {
            if (source == null) return destination;
            return source.Adapt(destination);
        }

        /// <summary>
        /// Map danh sách (Collection)
        /// </summary>
        public static IEnumerable<TDestination> MapList<TSource, TDestination>(this IEnumerable<TSource> source)
        {
            if (source == null) return Enumerable.Empty<TDestination>();
            return source.Adapt<IEnumerable<TDestination>>();
        }
    }
}
