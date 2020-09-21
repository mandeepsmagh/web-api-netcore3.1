
using AutoMapper;
using Entities.Dtos;
using Entities.Models;

namespace CompanyEmployees.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Company, CompanyDto>()
                .ForMember(c => c.FullAddress,
                    opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
            
            CreateMap<Employee, EmployeeDto>();

            CreateMap<CompanyForCreationDto, Company>();

            CreateMap<EmployeeForCreationDto, Employee>();

            CreateMap<EmployeeForUpdateDto, Employee>();

            CreateMap<CompanyForUpdateDto, Company>();

            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();
        }
    }
}