using System;
using System.Data;
using System.Data.Common;

namespace Groton.Core.Data.Entities
{
    public class Employee
    {
        public int EmployeeID { get; set; }

        public string? Name { get; set; }

        public string? JobTitle { get; set; }
    }
}
