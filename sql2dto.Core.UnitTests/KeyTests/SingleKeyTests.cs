using sql2dto.Attributes;
using sql2dto.Core.UnitTests.KeyTests.Data;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.KeyTests
{
    public class SingleKeyTests
    {
        public class Empolyee : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Ratio { get; set; }
        }

        [Fact]
        public void Fetch_by_key_value()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Ratio");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee, int>(h);

            while (h.Read())
            {
                var e = empCol.FetchByKeyValue(h.GetInt32(nameof(Empolyee.Id)));
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        [Fact]
        public void Fetch_by_key_props()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Ratio");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee, int>(h);

            while (h.Read())
            {
                var e = empCol.FetchByKeyProps(nameof(Empolyee.Id));
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
