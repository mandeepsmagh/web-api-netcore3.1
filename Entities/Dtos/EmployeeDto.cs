using System;

namespace Entities.Dtos
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public String Name { get; set; }
        public int  Age { get; set; }
        public String Position { get; set; }

    }
}