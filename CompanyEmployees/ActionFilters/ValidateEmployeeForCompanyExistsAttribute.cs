using System;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployee.ActionFilters
{
    public class ValidateEmployeeForCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;

        public ValidateEmployeeForCompanyExistsAttribute(IRepositoryManager repository, ILoggerManager logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATch")) ? true : false;

            var companyId = (Guid)context.ActionArguments["companyId"];
            var company = await _repository.Company.GetCompanyAsync(companyId, false);

            if(company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the Db.");
                context.Result = new NotFoundResult();
                return;
            }

            var id = (Guid)context.ActionArguments["id"];
            var employee = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);

            if(employee == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the Db.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }
        }
    }
}