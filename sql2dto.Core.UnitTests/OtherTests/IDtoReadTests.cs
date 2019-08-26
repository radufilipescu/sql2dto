using sql2dto.Attributes;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.OtherTests
{
    public class IDtoReadTests
    {
        public static int DtoReadCallsCount { get; set; }

        [KeyProps(nameof(Id))]
        [ColumnsPrefix("A_")]
        public class A : IOnDtoRead
        {
            public A()
            {
                BItems = new List<B>();
            }

            public int Id { get; set; }
            public string Name { get; set; }

            public List<B> BItems { get; set; }

            public void OnDtoRead(Dictionary<string, object> rowValues)
            {
                DtoReadCallsCount++;
                Assert.Empty(BItems);
            }
        }

        [KeyProps(nameof(Id))]
        [ColumnsPrefix("B_")]
        public class B
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void Test_1()
        {
            IDtoReadTests.DtoReadCallsCount = 0;

            var fakeDataReader = new FakeDataReader("A_ID", "A_NAME", "B_ID", "B_NAME");

            fakeDataReader.AddRow(1, "A1", 1, "B1");
            fakeDataReader.AddRow(1, "A1", 2, "B2");
            fakeDataReader.AddRow(1, "A1", 3, "B3");

            fakeDataReader.AddRow(2, "A2", 1, "B1");
            fakeDataReader.AddRow(2, "A2", 2, "B2");
            fakeDataReader.AddRow(2, "A2", 3, "B3");

            fakeDataReader.AddRow(3, "A2", 1, "B1");
            fakeDataReader.AddRow(3, "A2", 2, "B2");
            fakeDataReader.AddRow(3, "A2", 3, "B3");

            using (var h = new ReadHelper(fakeDataReader))
            {
                var fetch = h.Fetch<A>().Include<B>((a, b) => a.BItems.Add(b));
                var result = fetch.All(); 
            }

            Assert.Equal(3, IDtoReadTests.DtoReadCallsCount);
        }
    }
}
