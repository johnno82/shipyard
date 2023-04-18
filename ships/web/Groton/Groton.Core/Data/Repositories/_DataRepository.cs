/*
    This is a basic implementation of a generic CRUD class using ADO.NET. You will need to replace the `ConnectionString` 
    with the actual connection string for your database. This class can be used as a base class for other data repositories, 
    which should provide the implementation for the abstract properties and methods:

        - `TableName`: The name of the database table for the entity.
        - `CreateDbParameters(TEntity entity)`: Create an array of `DbParameter` objects for the given entity.
        - `CreateEntity(IDataRecord record)`: Create a new entity object from the given `IDataRecord`.

    To ensure the code is safe from SQL injections, we use parameterized queries and do not concatenate user input directly into the SQL commands.
*/

using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Groton.Core.Data.Repositories
{
    public abstract class DataRepository<TEntity>
    {
        private readonly string _connectionString;

        protected abstract string TableName { get; }

        protected abstract string PrimaryKeyName { get; }

        public DataRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Add(TEntity entity)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO {TableName} VALUES({GetAddParametersPlaceholder()});";
                    command.Parameters.AddRange(CreateAddDbParameters(entity));

                    command.ExecuteNonQuery();
                }
            }
        }

        public TEntity? GetById(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM {TableName} WHERE {PrimaryKeyName} = @Id;";
                    var parameter = CreateParameter("@Id", id);
                    command.Parameters.Add(parameter);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return ReadEntity(reader);
                        }
                    }
                }
            }

            return default;
        }

        public TEntity[] GetAll()
        {
            var result = new List<TEntity>();
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM {TableName};";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(ReadEntity(reader));
                        }
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// You can use the `GetAll` method with optional paging parameters to retrieve a specific page of results:
        /// 
        ///   var filters = new Dictionary<string, object>
        ///   {
        ///     { "FirstName", "John" },
        ///     { "LastName", "Doe" }
        ///   };
        ///   
        ///   int pageNumber = 2;
        ///   int pageSize = 10;
        /// 
        ///   var results = myDataRepository.GetAll(filters, pageNumber, pageSize);
        /// 
        /// This would generate a query similar to:
        /// SELECT * FROM TableName WHERE FirstName = @FirstName AND LastName = @LastName;
        /// </summary>
        public TEntity[] GetAll(IDictionary<string, object> filters)
        {
            var result = new List<TEntity>();

            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM {TableName}";

                    if (filters != null && filters.Count > 0)
                    {
                        command.CommandText += " WHERE ";

                        var filterClauses = new List<string>();
                        foreach (var filter in filters)
                        {
                            filterClauses.Add($"{filter.Key} = @{filter.Key}");
                            command.Parameters.Add(CreateParameter($"@{filter.Key}", filter.Value));
                        }

                        command.CommandText += string.Join(" AND ", filterClauses);
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(ReadEntity(reader));
                        }
                    }
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// You can use the `GetAll` method with optional paging parameters to retrieve a specific page of results:
        /// 
        ///   var filters = new Dictionary<string, object>
        ///   {
        ///     { "FirstName", "John" },
        ///     { "LastName", "Doe" }
        ///   };
        ///   
        ///   int pageNumber = 2;
        ///   int pageSize = 10;
        /// 
        ///   var results = myDataRepository.GetAll(filters, pageNumber, pageSize);
        /// 
        /// This would generate a query similar to:
        /// SELECT * FROM TableName WHERE FirstName = @FirstName AND LastName = @LastName ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
        /// </summary>
        public TEntity[] GetAll(IDictionary<string, object> filters, int? pageIndex = null, int? pageSize = null)
        {
            var result = new List<TEntity>();

            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM {TableName}";

                    if (filters != null && filters.Count > 0)
                    {
                        command.CommandText += " WHERE ";

                        var filterClauses = new List<string>();
                        foreach (var filter in filters)
                        {
                            filterClauses.Add($"{filter.Key} = @{filter.Key}");
                            command.Parameters.Add(CreateParameter($"@{filter.Key}", filter.Value));
                        }

                        command.CommandText += string.Join(" AND ", filterClauses);
                    }

                    if (pageIndex.HasValue && pageSize.HasValue)
                    {
                        command.CommandText += $" ORDER BY {PrimaryKeyName} OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
                        command.Parameters.Add(CreateParameter("@Offset", (pageIndex.Value - 1) * pageSize.Value));
                        command.Parameters.Add(CreateParameter("@PageSize", pageSize.Value));
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(ReadEntity(reader));
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public void Update(int id, IDictionary<string, object> changes)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE {TableName} SET {GetUpdateParametersAssignment(changes)} WHERE {PrimaryKeyName} = @Id;";
                    command.Parameters.Add(CreateParameter("@Id", id));
                    command.Parameters.AddRange(CreateUpdateDbParameters(changes));

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Delete(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"DELETE FROM {TableName} WHERE {PrimaryKeyName} = @Id;";
                    command.Parameters.Add(CreateParameter("@Id", id));

                    command.ExecuteNonQuery();
                }
            }
        }

        protected abstract TEntity ReadEntity(IDataRecord record);

        protected DbParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }

        protected abstract DbParameter[] CreateAddDbParameters(TEntity entity);        

        private string GetAddParametersPlaceholder()
        {
            var parameters = CreateAddDbParameters(default);
            return string.Join(", ", Array.ConvertAll(parameters, p => p.ParameterName));
        }

        private DbParameter[] CreateUpdateDbParameters(IDictionary<string, object> changes)
        {
            return changes.Select(kvp => CreateParameter(kvp.Key, kvp.Value)).ToArray();
        }

        private string GetUpdateParametersAssignment(IDictionary<string, object> changes)
        {
            var parameters = CreateUpdateDbParameters(changes);
            return string.Join(", ", Array.ConvertAll(parameters, p => $"{p.SourceColumn} = {p.ParameterName}"));
        }

        private DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
