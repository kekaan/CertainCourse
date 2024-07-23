namespace CertainCourse.OrderService;

internal static class UnixTimestampConverter
{
    public static DateTime UnixTimeStampToDateTime(long unixTimeStamp)
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp);
        
        return dateTimeOffset.UtcDateTime;
    }

    public static long DateTimeToUnixTimeStamp(DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
    }
}