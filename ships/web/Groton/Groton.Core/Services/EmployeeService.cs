using Groton.Core.Data.Entities;
using Groton.Core.Data.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groton.Core.Services
{
    public class EmployeeService
    {
        private readonly ILogger<EmployeeService> _logger;

        private readonly EmployeeRepository _repository;

        public EmployeeService(
            ILogger<EmployeeService> logger,
            EmployeeRepository repository)
        {
            _logger = logger;
            _repository = repository;
        }

        public void Add(Employee employee)
        {
            if(String.IsNullOrEmpty(employee.Name))
                throw new ArgumentNullException(nameof(employee.Name));

            if (String.IsNullOrEmpty(employee.JobTitle))
                throw new ArgumentNullException(nameof(employee.JobTitle));

            _repository.Add(employee);
        }

        public Employee? GetById(int id) 
        {
            return _repository.GetById(id);
        }

        public Employee[] GetAll(string? name = null, string? jobTitle = null, int? pageIndex = null, int? pageSize = null)
        {
            var filters = new Dictionary<string, object>(0);

            if(!String.IsNullOrWhiteSpace(name))
                filters.Add("Name", name);
            if(!String.IsNullOrWhiteSpace(jobTitle))
                filters.Add("JobTitle", jobTitle);

            return _repository.GetAll(filters, pageIndex, pageSize);
        }

        public void Update(int id, string? name = null, string? jobTitle = null)
        {
            var changes = new Dictionary<string, object>(0);

            if (!String.IsNullOrWhiteSpace(name))
                changes.Add("Name", name);
            if (!String.IsNullOrWhiteSpace(jobTitle))
                changes.Add("JobTitle", jobTitle);

            _repository.Update(id, changes);
        }
    }
}
