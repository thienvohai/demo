using Dapper;
using System.Data;
using System.Reflection;

namespace EmailService;

public abstract class BasePgRepository
{
    public int CommandTimeout { get; set; } = 30;
    public IDbTransaction DbTransaction { get; private set; }
    public IDbConnectionProvider DbConnectionProvider { get; private set; }
    public IDbConnection DbConnection => DbConnectionProvider.DbConnection;

    public BasePgRepository(IDbConnectionProvider dbConnectionProvider, IDbTransaction transaction)
    {
        DbConnectionProvider = dbConnectionProvider;
        DbTransaction = transaction;
        Schema = dbConnectionProvider.Schema;
    }

    public string Schema { get; private set; }

    public async Task AddAsync<T>(T entity) where T : BaseEntity
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var type = typeof(T);
        var tableNameAttribute = type.GetCustomAttribute<TableAttribute>(false);
        var tableName = tableNameAttribute!.Name;
        var properties = type.GetProperties().Where(p => IsSimpleType(p.PropertyType)).ToList();
        var columnMappings = properties.Select(p =>
        {
            var columnAttr = p.GetCustomAttributes<ColumnAttribute>(false).FirstOrDefault();
            return new
            {
                ColumnName = columnAttr?.Name ?? p.Name,
                ParameterName = $"@{p.Name}",
                Property = p
            };
        }).ToList();

        var columnNames = string.Join(", ", columnMappings.Select(c => $"\"{c.ColumnName}\""));
        var valueParameters = string.Join(", ", columnMappings.Select(c => c.ParameterName));
        var insertQuery = $"INSERT INTO \"{Schema}\".\"{tableName}\" ({columnNames}) VALUES ({valueParameters})";
        var dynamicParams = new DynamicParameters();
        foreach (var mapping in columnMappings)
        {
            if (mapping.Property.PropertyType == typeof(DateTime))
                dynamicParams.Add(mapping.ParameterName, mapping.Property.GetValue(entity), DbType.DateTime);
            else
                dynamicParams.Add(mapping.ParameterName, mapping.Property.GetValue(entity));
        }
        await DbConnection.ExecuteAsync(insertQuery, dynamicParams, DbTransaction, CommandTimeout);
    }

    private bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);
        // If it's nullable, we only want to check the underlying type
        type = underlyingType ?? type;

        return type.IsPrimitive ||
               type.IsEnum ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTime) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }
}
