using sql2dto.Attributes;
using sql2dto.Core.UnitTests.ConverterTests.Data;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.ConverterTests
{
    public class AttributesMapConfigTests
    {
        public class Employee_AttrMap : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            
            [NullableIntToBooleanConverterAttribute]
            public bool IsActive { get; set; }
        }

        public class NullableIntToBooleanConverterAttribute : ConverterAttribute
        {
            public override Func<object, object> Converter => (v) => Convert.ToBoolean(v ?? 0);
        }

        [Fact]
        public void Test()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "IsActive");
            ConverterTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Employee_AttrMap>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            ConverterTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
