using CertainCourse.OrderService.Common;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Common.Extensions;

public static class ReadOnlyCollectionExtension
{
    public static PagedCollection<T> ToPagedList<T>(this IReadOnlyCollection<T> source, int pageSize, string pageToken,
        string nextPageToken)
    {
        return new PagedCollection<T>(source, pageSize, pageToken, nextPageToken);
    }
}