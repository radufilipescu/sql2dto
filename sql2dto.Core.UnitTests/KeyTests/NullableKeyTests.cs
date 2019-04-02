using sql2dto.Core.UnitTests.KeyTests.Data;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.KeyTests
{
    public class NullableKeyTests
    {
        public class Human : IHuman
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public void Fetch_by_nullable_key_value()
        {
            var fakeReader = new FakeDataReader("Name", "Age");
            NullableKeyTestsData.SetupHumansData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var humCol = new DtoCollection<Human, string>(h);

            while (h.Read())
            {
                if (h.IsDBNull(nameof(Human.Name)))
                {
                    // KeyesToIndexes to store this key as ~~~
                    // However, the name will be stored as null in the dto
                    var e = humCol.FetchByKeyValue("~~~"); 
                }
                else
                {
                    var e = humCol.FetchByKeyValue(h.GetString(nameof(Human.Name)));
                }
            }

            NullableKeyTestsData.AssertDataIntegrity(humCol.InnerList.Cast<IHuman>());
        }

        public class Human_2Keyes : IHuman
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public void Fetch_by_compound_nullable_key_value()
        {
            var fakeReader = new FakeDataReader("Name", "Age");
            NullableKeyTestsData.SetupHumansData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var humCol = new DtoCollection<Human, string, int>(h);

            while (h.Read())
            {
                if (h.IsDBNull(nameof(Human.Name)))
                {
                    // KeyesToIndexes to store this key as (~~~, id)
                    // However, the name will be stored as null in the dto
                    var e = humCol.FetchByKeyValue(("~~~", h.GetInt32(nameof(Human.Age))));
                }
                else
                {
                    var e = humCol.FetchByKeyValue(
                        (h.GetString(nameof(Human.Name)), h.GetInt32(nameof(Human.Age)))
                    );
                }
            }

            NullableKeyTestsData.AssertDataIntegrity(humCol.InnerList.Cast<IHuman>());
        }
    }
}
