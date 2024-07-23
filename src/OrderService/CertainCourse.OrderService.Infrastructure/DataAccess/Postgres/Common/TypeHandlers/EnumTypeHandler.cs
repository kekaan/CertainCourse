using System.Data;
using Dapper;
using NpgsqlTypes;

namespace CertainCourse.OrderService.Infrastructure.DataAccess.Postgres.Common.TypeHandlers;

public class EnumTypeHandler<T> : SqlMapper.TypeHandler<T> where T : struct, IConvertible
{
    public override T Parse(object value)
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        return value switch
        {
            int i => (T)(object)i,
            string s => (T)Enum.Parse(typeof(T), s),
            _ => throw new NotSupportedException($"{value} not a valid MyPostgresEnum value")
        };
    }

    public override void SetValue(IDbDataParameter parameter, T value)
    {
        if (!typeof(T).IsEnum)
        {
            throw new ArgumentException("T must be an enumerated type");
        }

        parameter.DbType = (DbType)NpgsqlDbType.Unknown;
        parameter.Value = Enum.GetName(typeof(T), (int)(object)value)?.ToLowerInvariant(); 
    }
}