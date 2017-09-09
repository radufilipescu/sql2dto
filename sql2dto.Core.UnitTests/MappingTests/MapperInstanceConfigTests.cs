using sql2dto.Core.UnitTests.MappingTests.Data;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.MappingTests
{
    public class MapperInstanceConfigTests
    {
        public class Employee_WithMapper : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double? Ratio { get; set; }
        }

        private DtoMapper<Employee_WithMapper> _employeeMapper = new DtoMapper<Employee_WithMapper>()
            .MapProp(nameof(Employee_WithMapper.Id), "EmpId")
            .MapProp(nameof(Employee_WithMapper.Name), "emp_Name")
            .MapProp(nameof(Employee_WithMapper.Ratio), "RATIO");

        [Fact]
        public void Test()
        {
            var fakeReader = new FakeDataReader("EmpId", "emp_Name", "RATIO");
            MappingTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Employee_WithMapper>(h, _employeeMapper);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            MappingTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
