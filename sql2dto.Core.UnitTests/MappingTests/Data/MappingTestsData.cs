using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using sql2dto.Core.UnitTests.ConverterTests.Data;

namespace sql2dto.Core.UnitTests.MappingTests.Data
{
    public static class MappingTestsData
    {
        public static void SetupEmployeesData(FakeDataReader fakeReader)
        {
            fakeReader.AddRow(1, "Alex", 0.1);
            fakeReader.AddRow(2, "George", null);
            fakeReader.AddRow(3, "Lisa", 0.3);
        }

        public static void AssertDataIntegrity(IEnumerable<IEmployee> emps)
        {
            var alex = emps.First();
            Assert.Equal(alex.Id, 1);
            Assert.Equal(alex.Name, "Alex");
            Assert.Equal(alex.Ratio, 0.1);

            var george = emps.ElementAt(1);
            Assert.Equal(george.Id, 2);
            Assert.Equal(george.Name, "George");
            Assert.Equal(george.Ratio, null);

            var lisa = emps.ElementAt(2);
            Assert.Equal(lisa.Id, 3);
            Assert.Equal(lisa.Name, "Lisa");
            Assert.Equal(lisa.Ratio, 0.3);
        }

        internal static void AssertDataIntegrity(IEnumerable<ConverterTests.Data.IEmployee> enumerable)
        {
            throw new NotImplementedException();
        }
    }
}
