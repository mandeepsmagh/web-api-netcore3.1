using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using Entities;
using Entities.Dtos;
using Entities.Models;
using LoggerService;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CompanyEmployees.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mappper;

        public EmployeesController(IRepositoryManager repository, ILoggerManager logger,
            IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mappper = mapper;
        }

        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            var company = _repository.Company.GetCompanyAsync(companyId, trackChanges:false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            var employeesFromDb = _repository.Employee.GetEmployees(companyId, trackChanges:false);

            var employeesDto = _mappper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);

            return Ok(employeesDto);
        }

        [HttpGet("{id}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges:false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");

                return NotFound();
            }

            var employeeFromDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges:false);

            if (employeeFromDb == null)
            {
                _logger.LogInfo($"Employee with id : {id} doesn't exist in Db.");
            }

            var employeeDto = _mappper.Map<EmployeeDto>(employeeFromDb);

            return Ok(employeeDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody]EmployeeForCreationDto employee)
        {
            if (employee == null)
            {
                _logger.LogError("EmployeeForCreationDto object from client is null");

                return BadRequest("EmployeeForCreationDto object is null");
            }

            if(!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForCreationDto object.");
                return UnprocessableEntity(ModelState);
            }

            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges:false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the Db.");

                return NotFound();
            }

            var employeeEntitiy = _mappper.Map<Employee>(employee);

            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntitiy);
            await _repository.SaveAsync();

            var employeeToReturn = _mappper.Map<EmployeeDto>(employeeEntitiy);

            return CreatedAtRoute("GetEmployeeForCompany", 
                new {companyId, id = employeeToReturn.Id}, employeeToReturn);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges:false);

            if(company == null)
            {
                _logger.LogInfo($"company iwht id: {companyId} doesn't exist in the Db.");
                return NotFound();
            }

            var employeeForCompany = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges:false);

            if (employeeForCompany == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the Db.");

                return NotFound();
            }

            _repository.Employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id, [FromBody]
            EmployeeForUpdateDto employee)
        {
            if(employee == null)
            {
                _logger.LogError("EmployeeForUpdateDto object sent from client is null.");

                return BadRequest();
            }

            if(!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForCreationDto object.");
                return UnprocessableEntity(ModelState);
            }

            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges:false);
            
            if(company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the Db.");
                return NotFound();
            }

            var employeeEntitiy = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges:true);

            if(employeeEntitiy == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the database.");

                return NotFound();
            }

            _mappper.Map(employee, employeeEntitiy);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid id, 
            [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchdoc)
        {
            if(patchdoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null.");
                return BadRequest();
            }

            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges:false);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the Db.");
                return NotFound();
            }

            var employeeEntitiy = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges:true);

            if(employeeEntitiy == null)
            {
                _logger.LogInfo($"Employee with id: {id} doesn't exist in the Db.");
                return NotFound();
            }

            var employeeToPatch = _mappper.Map<EmployeeForUpdateDto>(employeeEntitiy);

            patchdoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if(!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the EmployeeForCreationDto object.");
                return UnprocessableEntity(ModelState);
            }

            _mappper.Map(employeeToPatch, employeeEntitiy);
            await _repository.SaveAsync();

            return NoContent();
        }

    }
}