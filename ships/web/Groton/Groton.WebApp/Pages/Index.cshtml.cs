using Groton.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Groton.WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        private readonly EmployeeService _employeeService;

        public IndexModel(
            ILogger<IndexModel> logger,
            EmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        public void OnGet()
        {

        }
    }
}