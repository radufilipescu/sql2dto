using sql2dto.Core.UnitTests.MappingTests.Data;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.MappingTests
{
    public class NoMapConfigTests
    {
        public class Employee_NoMap : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double? Ratio { get; set; }
        }

        [Fact]
        public void Test()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "RATIO");
            MappingTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Employee_NoMap>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            MappingTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
