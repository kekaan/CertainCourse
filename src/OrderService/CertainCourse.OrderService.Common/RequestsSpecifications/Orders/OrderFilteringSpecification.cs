namespace CertainCourse.OrderService.Common.RequestsSpecifications.Orders;

/// <summary>
/// Модель для применения фильтрации. Каждая коллекция модели содержит в себе допустимые значения
/// для каждого поля, то есть означает логическое "или' при применения фильтра.
/// Чтобы применить фильтры в значении "и" необходимо добавить еще один такой фильтр
/// в коллекцию, передаваемую в метод репозитория
/// </summary>
public sealed class OrderFilteringSpecification
{
    public DateTime? MinCreateDateTime { get; init; }
    public IEnumerable<int>? RegionIdPossibleValues { get; init; }
    public IEnumerable<int>? CustomerIdPossibleValues { get; init; }
}