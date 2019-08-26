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
            Assert.Equal(1, alex.Id);
            Assert.Equal("Alex", alex.Name);
            Assert.Equal(0.1, alex.Age);

            var geroge = emps.ElementAt(1);
            Assert.Equal(2, geroge.Id);
            Assert.Equal("George", geroge.Name);
            Assert.Equal(0.2, geroge.Age);

            var lisa = emps.ElementAt(2);
            Assert.Equal(3, lisa.Id);
            Assert.Equal("Lisa", lisa.Name);
            Assert.Equal(0.3, lisa.Age);
        }
    }
}
