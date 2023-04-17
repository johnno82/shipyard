using Groton.Core.Data.Entities;
using System;
using System.Data;
using System.Data.Common;

namespace Groton.Core.Data.Repositories
{
    public class EmployeeDataRepository : BaseDataRepository<Employee>
    {
        protected override string TableName => "Employees";

        protected override DbParameter[] CreateDbParameters(Employee entity)
        {
            return new DbParameter[]
            {
            CreateParameter("@EmployeeID", entity.EmployeeID),
            CreateParameter("@Name", entity.Name),
            CreateParameter("@JobTitle", entity.JobTitle)
            };
        }

        protected override Employee CreateEntity(IDataRecord record)
        {
            return new Employee
            {
                EmployeeID = Convert.ToInt32(record["EmployeeID"]),
                Name = record["Name"].ToString(),
                JobTitle = record["JobTitle"].ToString()
            };
        }
    }
}
