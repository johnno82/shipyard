﻿using Groton.Core.Data.Entities;
using System;
using System.Data;
using System.Data.Common;

namespace Groton.Core.Data.Repositories
{
    public class EmployeeRepository : DataRepository<Employee>
    {
        protected override string TableName => "Employees";

        protected override string PrimaryKeyName => "EmployeeID";

        public EmployeeRepository(string connectionString) 
            : base(connectionString)
        { 
        }

        protected override Employee ReadEntity(IDataRecord record)
        {
            return new Employee
            {
                EmployeeID = Convert.ToInt32(record["EmployeeID"]),
                Name = record["Name"].ToString(),
                JobTitle = record["JobTitle"].ToString()
            };
        }

        protected override DbParameter[] CreateAddDbParameters(Employee entity)
        {
            return new DbParameter[]
            {
                CreateParameter("@Name", entity.Name),
                CreateParameter("@JobTitle", entity.JobTitle)
            };
        }
    }
}
