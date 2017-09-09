using sql2dto.Attributes;
using sql2dto.Core.UnitTests.MappingTests.Data;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.MappingTests
{
    public class AttributesMapConfigTests
    {
        public class Employee_AttrMap : IEmployee
        {
            [PropMap(ColumnName = "EmpId")]
            public int Id { get; set; }

            [PropMap(ColumnName = "Emp_NAME")]
            public string Name { get; set; }

            [PropMap(ColumnName = "ratio")]
            public double? Ratio { get; set; }
        }

        [Fact]
        public void Test()
        {
            var fakeReader = new FakeDataReader("EmpId", "emp_Name", "RATIO");
            MappingTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Employee_AttrMap>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            MappingTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
