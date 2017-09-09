using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.ConverterTests.Data
{
    public static class ConverterTestsData
    {
        public static void SetupEmployeesData(FakeDataReader fakeReader)
        {
            fakeReader.AddRow(1, "Alex", 1);
            fakeReader.AddRow(2, "George", 0);
            fakeReader.AddRow(3, "Lisa", 1);
            fakeReader.AddRow(4, "Jerome", null);
        }

        public static void AssertDataIntegrity(IEnumerable<IEmployee> emps)
        {
            Assert.Equal(4, emps.Count());

            var alex = emps.First();
            Assert.Equal(alex.Id, 1);
            Assert.Equal(alex.Name, "Alex");
            Assert.Equal(alex.IsActive, true);

            var geroge = emps.ElementAt(1);
            Assert.Equal(geroge.Id, 2);
            Assert.Equal(geroge.Name, "George");
            Assert.Equal(geroge.IsActive, false);

            var lisa = emps.ElementAt(2);
            Assert.Equal(lisa.Id, 3);
            Assert.Equal(lisa.Name, "Lisa");
            Assert.Equal(lisa.IsActive, true);

            var jerome = emps.ElementAt(3);
            Assert.Equal(jerome.Id, 4);
            Assert.Equal(jerome.Name, "Jerome");
            Assert.Equal(jerome.IsActive, false);
        }
    }
}
