using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.KeyTests.Data
{
    public static class KeyTestsData
    {
        public static void SetupEmployeesData(FakeDataReader fakeReader)
        {
            fakeReader.AddRow(1, "Alex", 0.1);
            fakeReader.AddRow(2, "George", 0.2);
            fakeReader.AddRow(1, "Alex", 0.1);
            fakeReader.AddRow(3, "Lisa", 0.3);
            fakeReader.AddRow(2, "George", 0.2);
            fakeReader.AddRow(1, "Alex", 0.1);
        }

        public static void AssertDataIntegrity(IEnumerable<IEmployee> emps)
        {
            Assert.Equal(3, emps.Count());

            var alex = emps.First();
            Assert.Equal(alex.Id, 1);
            Assert.Equal(alex.Name, "Alex");
            Assert.Equal(alex.Ratio, 0.1);

            var geroge = emps.ElementAt(1);
            Assert.Equal(geroge.Id, 2);
            Assert.Equal(geroge.Name, "George");
            Assert.Equal(geroge.Ratio, 0.2);

            var lisa = emps.ElementAt(2);
            Assert.Equal(lisa.Id, 3);
            Assert.Equal(lisa.Name, "Lisa");
            Assert.Equal(lisa.Ratio, 0.3);
        }
    }
}
