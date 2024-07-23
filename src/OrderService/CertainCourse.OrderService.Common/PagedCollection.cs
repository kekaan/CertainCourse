namespace CertainCourse.OrderService.Common;

public sealed record PagedCollection<T>
{
    public string PageToken { get; }
    public int PageSize { get; }
    public string NextPageToken { get; }
    public IReadOnlyCollection<T> Items { get; }

    public PagedCollection(IReadOnlyCollection<T> source, int pageSize, string pageToken, string nextPageToken)
    {
        PageToken = pageToken;
        PageSize = pageSize;
        NextPageToken = nextPageToken;
        Items = source;
    }
}