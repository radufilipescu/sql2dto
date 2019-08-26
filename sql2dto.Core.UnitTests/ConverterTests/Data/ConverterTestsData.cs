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
            Assert.Equal(1, alex.Id);
            Assert.Equal("Alex", alex.Name);
            Assert.True(alex.IsActive);

            var geroge = emps.ElementAt(1);
            Assert.Equal(2, geroge.Id);
            Assert.Equal("George", geroge.Name);
            Assert.False(geroge.IsActive);

            var lisa = emps.ElementAt(2);
            Assert.Equal(3, lisa.Id);
            Assert.Equal("Lisa", lisa.Name);
            Assert.True(lisa.IsActive);

            var jerome = emps.ElementAt(3);
            Assert.Equal(4, jerome.Id);
            Assert.Equal("Jerome", jerome.Name);
            Assert.False(jerome.IsActive);
        }
    }
}
