using CertainCourse.GatewayService.Infrastructure.ServiceLib.Converters;

namespace CertainCourse.GatewayService.Tests.InfrastructureTests;

public class UnixTimestampConverterTests
{
    [Fact]
    public void UnixTimeStampToDateTime_ShouldConvertCorrectly()
    {
        // Arrange
        long unixTimeStamp = 1625097600; // 1 July 2021 00:00:00 UTC

        // Act
        DateTime dateTime = UnixTimestampConverter.UnixTimeStampToDateTime(unixTimeStamp);

        // Assert
        Assert.Equal(new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Utc), dateTime);
    }

    [Fact]
    public void DateTimeToUnixTimeStamp_ShouldConvertCorrectly()
    {
        // Arrange
        DateTime dateTime = new DateTime(2021, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        long unixTimeStamp = UnixTimestampConverter.DateTimeToUnixTimeStamp(dateTime);

        // Assert
        Assert.Equal(1625097600, unixTimeStamp);
    }
}