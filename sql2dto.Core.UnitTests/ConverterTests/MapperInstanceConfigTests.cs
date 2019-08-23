using sql2dto.Core.UnitTests.ConverterTests.Data;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.ConverterTests
{
    public class MapperInstanceConfigTests
    {
        public class Employee_WithMapper : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public bool IsActive { get; set; }
        }

        private DtoMapper<Employee_WithMapper> _employeeMapper = DtoMapper<Employee_WithMapper>.Default.Clone()
            .MapProp(_ => _.IsActive, (value) => Convert.ToBoolean(value ?? 0));

        [Fact]
        public void Test()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "IsActive");
            ConverterTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Employee_WithMapper>(h, _employeeMapper);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            ConverterTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
