using System.Data;
using System.Text.Json;
using Dapper;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.TypeHandlers;

public class JsonTypeHandler<T>: SqlMapper.TypeHandler<T>
{
    public override T? Parse(object value)
    {
        var json = (string)value;
        return JsonSerializer.Deserialize<T>(json);
    }

    public override void SetValue(IDbDataParameter parameter, T? value)
    {
        parameter.Value = JsonSerializer.Serialize(value);
    }
}