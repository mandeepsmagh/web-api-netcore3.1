using System;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployee.ActionFilters
{
    public class ValidateCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        
        public ValidateCompanyExistsAttribute(IRepositoryManager repository, ILoggerManager logger)
        {
            _logger = logger;
            _repository = repository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var trackChanges = context.HttpContext.Request.Method.Equals("PUT");
            var id = (Guid)context.ActionArguments["id"];
            var company = await _repository.Company.GetCompanyAsync(id, trackChanges);

            if(company == null)
            {
                _logger.LogInfo($"Company with id: {id} doesn't exist in the Db.");
                context.Result = new NotFoundResult();
            }
            else
            {
                context.HttpContext.Items.Add("company", company);
                await next();
            }
        }
    }
}