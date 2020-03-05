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
            public double? Age { get; set; }
        }

        private DtoMapper<Employee_WithMapper> _propsEmployeeMapper = DtoMapper<Employee_WithMapper>.Default.Clone()
            .MapProp(_ => _.Id, "EmpId")
            .MapProp(_ => _.Name, "emp_Name")
            .MapProp(_ => _.Age, "AGE");

        [Fact]
        public void Test()
        {
            var fakeReader = new FakeDataReader("EmpId", "emp_Name", "AGE");
            MappingTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Employee_WithMapper>(h, _propsEmployeeMapper);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            MappingTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        private DtoMapper<Employee_WithMapper> _prefixedEmployeeMapper = DtoMapper<Employee_WithMapper>.Default.Clone()
            .SetColumnsPrefix("EMP_");

        [Fact]
        public void Test2()
        {
            var fakeReader = new FakeDataReader("EMP_Id", "EMP_Name", "EMP_Age");
            MappingTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Employee_WithMapper>(h, _prefixedEmployeeMapper);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            MappingTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        private DtoMapper<Employee_WithMapper> _employeeMapper = DtoMapper<Employee_WithMapper>.Default.Clone()
            .SetColumnsPrefix("EMP_")
            .MapProp(_ => _.Id, "VALUE")
            .MapProp(_ => _.Name, "LABEL");

        [Fact]
        public void Test3()
        {
            var fakeReader = new FakeDataReader("EMP_VALUE", "EMP_LABEL", "EMP_Age");
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
