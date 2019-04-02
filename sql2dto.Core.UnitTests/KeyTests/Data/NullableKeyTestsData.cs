using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.KeyTests.Data
{
    public static class NullableKeyTestsData
    {
        public static void SetupHumansData(FakeDataReader fakeReader)
        {
            fakeReader.AddRow("Alex", 1);
            fakeReader.AddRow(null, 2);
            fakeReader.AddRow("Alex", 1);
            fakeReader.AddRow("Lisa", 3);
            fakeReader.AddRow(null, 2);
            fakeReader.AddRow("Alex", 1);
        }

        public static void AssertDataIntegrity(IEnumerable<IHuman> humans)
        {
            Assert.Equal(3, humans.Count());

            var alex = humans.First();
            Assert.Equal(alex.Name, "Alex");
            Assert.Equal(alex.Age, 1);

            var george = humans.ElementAt(1);
            Assert.Equal(george.Name, null);
            Assert.Equal(george.Age, 2);

            var lisa = humans.ElementAt(2);
            Assert.Equal(lisa.Name, "Lisa");
            Assert.Equal(lisa.Age, 3);
        }
    }
}
