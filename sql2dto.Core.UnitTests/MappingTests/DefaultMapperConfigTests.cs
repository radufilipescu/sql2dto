using sql2dto.Core.UnitTests.MappingTests.Data;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.MappingTests
{
    public class DefaultMapperConfigTests
    {
        public class Employee_WithDefaultMapper : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double? Age { get; set; }
        }

        static DefaultMapperConfigTests()
        {
            DtoMapper<Employee_WithDefaultMapper>.Default
                .MapProp(_ => _.Id, "EmpId")
                .MapProp(_ => _.Name, "emp_Name")
                .MapProp(_ => _.Age, "AGE");
        }

        [Fact]
        public void Test()
        {
            var fakeReader = new FakeDataReader("EmpId", "emp_Name", "AGE");
            MappingTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Employee_WithDefaultMapper>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            MappingTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
