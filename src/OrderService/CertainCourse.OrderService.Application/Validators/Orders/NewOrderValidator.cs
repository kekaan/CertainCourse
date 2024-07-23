using CertainCourse.OrderService.Domain.ValueObjects.Customer;
using CertainCourse.OrderService.Domain.ValueObjects.Region;

namespace CertainCourse.OrderService.Application.Validators.Orders;

internal sealed class NewOrderValidator : INewOrderValidator
{
    private const double EARTH_RADIUS_KM = 6371.0;
    private const double MAX_DISTANCE_M = 5000;
    
    public bool IsValidByDistance(Address destinationAddress, Storage storage)
    {
        var distance = CalculateDistanceInMeters(
            destinationAddress.Latitude, destinationAddress.Longitude,
            storage.Latitude, storage.Longitude);

        return distance > MAX_DISTANCE_M;
    }
    
    private static double CalculateDistanceInMeters(double latitude1, double longitude1, double latitude2, double longitude2)
    {
        var latitude1Radians = DegreesToRadians(latitude1);
        var longitude1Radians = DegreesToRadians(longitude1);
        var latitude2Radians = DegreesToRadians(latitude2);
        var longitude2Radians = DegreesToRadians(longitude2);

        var dLatitude = latitude2Radians - latitude1Radians;
        var dLongitude = longitude2Radians - longitude1Radians;

        // Применяем формулу гаверсинусов
        var a = Math.Pow(Math.Sin(dLatitude / 2), 2) +
                Math.Cos(latitude1Radians) * Math.Cos(latitude2Radians) * Math.Pow(Math.Sin(dLongitude / 2), 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EARTH_RADIUS_KM * c * 1000;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}