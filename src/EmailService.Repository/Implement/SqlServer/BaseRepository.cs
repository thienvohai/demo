using EmailService.Domain;
using Dapper;
using System.Data;
using System.Reflection;

namespace EmailService.Repository;

public abstract class BaseRepository
{
    public int CommandTimeout { get; set; } = 30;
    public IDbTransaction DbTransaction { get; private set; }
    public IDbConnectionProvider DbConnectionProvider { get; private set; }
    public IDbConnection DbConnection => DbConnectionProvider.DbConnection;

    public BaseRepository(IDbConnectionProvider dbConnectionProvider, IDbTransaction transaction)
    {
        DbConnectionProvider = dbConnectionProvider;
        DbTransaction = transaction;
        Schema = dbConnectionProvider.Schema;
    }

    public string Schema { get; private set; }

    public async Task<T?> GetByIdAsync<T>(Guid id) where T : BaseEntity
    {
        var type = typeof(T);
        var tableNameAttribute = type.GetCustomAttribute<TableAttribute>(false);
        var tableName = tableNameAttribute!.Name;
        var parameter = new DynamicParameters();
        parameter.Add("@id", id);
        var query = $@"Select * From [{Schema}].[{tableName}]
                       WHERE [{BaseColumns.Id}] = @id";

        var result = await DbConnection.QueryAsync<T>(query, parameter, DbTransaction, CommandTimeout);
        if (result != null && result.Any())
            return result.First();
        
        return null;
    }

    public async Task<IEnumerable<T>> GetRangeAsync<T>(EmailPageQuery pageQuery) where T : BaseEntity
    {
        var type = typeof(T);
        var tableNameAttribute = type.GetCustomAttribute<TableAttribute>(false);
        var tableName = tableNameAttribute!.Name;
        var parameter = new DynamicParameters();
        parameter.Add("@pageSize", pageQuery.PageSize);
        parameter.Add("@pageIndex", (pageQuery.PageIndex - 1) * pageQuery.PageSize);
        parameter.Add("@false", false);
        var query = $@"Select * From [{Schema}].[{tableName}]
               WHERE [{BaseColumns.IsDeleted}] = @false
               ORDER BY
               {BaseColumns.Modified} DESC
               OFFSET @pageIndex ROWS FETCH NEXT @pageSize ROWS ONLY;";

        var result = await DbConnection.QueryAsync<T>(query, parameter, DbTransaction, CommandTimeout);
        return result;
    }

    public async Task<long> CountAsync<T>() where T : BaseEntity
    {
        var type = typeof(T);
        var tableNameAttribute = type.GetCustomAttribute<TableAttribute>(false);
        var tableName = tableNameAttribute!.Name;
        var parameter = new DynamicParameters();
        parameter.Add("@false", false);
        var query = $@"Select COUNT(*) From [{Schema}].[{tableName}]
                       WHERE [{BaseColumns.IsDeleted}] = @false";

        var result = await DbConnection.ExecuteScalarAsync<long>(query, parameter, DbTransaction, CommandTimeout);
        return result;
    }

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

        var columnNames = string.Join(", ", columnMappings.Select(c => $"[{c.ColumnName}]"));
        var valueParameters = string.Join(", ", columnMappings.Select(c => c.ParameterName));
        var insertQuery = $"INSERT INTO [{Schema}].[{tableName}] ({columnNames}) VALUES ({valueParameters})";
        var dynamicParams = new DynamicParameters();
        foreach (var mapping in columnMappings)
        {
            if (mapping.Property.PropertyType == typeof(DateTime))
                dynamicParams.Add(mapping.ParameterName, mapping.Property.GetValue(entity), DbType.DateTime2);
            else
                dynamicParams.Add(mapping.ParameterName, mapping.Property.GetValue(entity));
        }
        await DbConnection.ExecuteAsync(insertQuery, dynamicParams, DbTransaction, CommandTimeout);
    }

    public async Task AddRangeAsync<T>(IEnumerable<T> entities) where T : BaseEntity
    {
        if (entities == null || !entities.Any())
            throw new ArgumentNullException(nameof(entities));

        var type = typeof(T);
        var firstEntity = entities.First();
        var properties = type.GetProperties().Where(p => IsSimpleType(p.PropertyType));

        var tableNameAttribute = type.GetCustomAttribute<TableAttribute>(false);
        var tableName = tableNameAttribute!.Name;

        var columnMappings = properties.Select(p =>
        {
            var columnAttr = p.GetCustomAttributes<ColumnAttribute>(false).FirstOrDefault();
            return new
            {
                ColumnName = columnAttr?.Name ?? p.Name,
                Property = p
            };
        }).ToList();

        var columnNames = string.Join(", ", columnMappings.Select(c => $"[{c.ColumnName}]"));

        var dynamicParameters = new DynamicParameters();
        var valueParameterList = new List<string>();
        int count = 0;

        foreach (var entity in entities)
        {
            var valueParameters = columnMappings.Select(c => $"@{c.Property.Name}{count}").ToArray();
            for (int i = 0; i < valueParameters.Length; i++)
            {
                if (columnMappings[i].Property.PropertyType == typeof(DateTime))
                    dynamicParameters.Add(valueParameters[i], columnMappings[i].Property.GetValue(entity), DbType.DateTime2);
                else
                    dynamicParameters.Add(valueParameters[i], columnMappings[i].Property.GetValue(entity));
            }
            valueParameterList.Add($"({string.Join(", ", valueParameters)})");
            count++;
        }

        var insertQuery = $"INSERT INTO [{Schema}].[{tableName}] ({columnNames}) VALUES {string.Join(", ", valueParameterList)}";

        await DbConnection.ExecuteAsync(insertQuery, dynamicParameters, DbTransaction, CommandTimeout);
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
