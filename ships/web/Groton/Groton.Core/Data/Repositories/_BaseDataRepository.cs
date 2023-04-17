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
    public abstract class BaseDataRepository<TEntity>
    {
        // You will need to replace this with your actual connection string
        private readonly string _connectionString = "your_connection_string_here";

        protected abstract string TableName { get; }

        public void Add(TEntity entity)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"INSERT INTO {TableName} VALUES({GetParametersPlaceholder()});";
                    command.Parameters.AddRange(CreateDbParameters(entity));

                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update(TEntity entity)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"UPDATE {TableName} SET {GetParametersAssignment()} WHERE Id = @Id;";
                    command.Parameters.AddRange(CreateDbParameters(entity));

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
                    command.CommandText = $"DELETE FROM {TableName} WHERE Id = @Id;";
                    var parameter = CreateParameter("@Id", id);
                    command.Parameters.Add(parameter);

                    command.ExecuteNonQuery();
                }
            }
        }

        public TEntity? Find(int id)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = $"SELECT * FROM {TableName} WHERE Id = @Id;";
                    var parameter = CreateParameter("@Id", id);
                    command.Parameters.Add(parameter);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return CreateEntity(reader);
                        }
                    }
                }
            }

            return default;
        }

        public List<TEntity> FindAll()
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
                            result.Add(CreateEntity(reader));
                        }
                    }
                }
            }

            return result;
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
        public List<TEntity> FindAll(Dictionary<string, object> filters)
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
                            result.Add(CreateEntity(reader));
                        }
                    }
                }
            }

            return result;
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
        public List<TEntity> FindAll(Dictionary<string, object> filters, int? pageNumber = null, int? pageSize = null)
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

                    if (pageNumber.HasValue && pageSize.HasValue)
                    {
                        command.CommandText += " ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
                        command.Parameters.Add(CreateParameter("@Offset", (pageNumber.Value - 1) * pageSize.Value));
                        command.Parameters.Add(CreateParameter("@PageSize", pageSize.Value));
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(CreateEntity(reader));
                        }
                    }
                }
            }

            return result;
        }

        protected abstract DbParameter[] CreateDbParameters(TEntity entity);

        protected abstract TEntity CreateEntity(IDataRecord record);

        protected DbParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }

        private string GetParametersPlaceholder()
        {
            var parameters = CreateDbParameters(default);
            return string.Join(", ", Array.ConvertAll(parameters, p => p.ParameterName));
        }

        private string GetParametersAssignment()
        {
            var parameters = CreateDbParameters(default);
            return string.Join(", ", Array.ConvertAll(parameters, p => $"{p.SourceColumn} = {p.ParameterName}"));
        }

        private DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
