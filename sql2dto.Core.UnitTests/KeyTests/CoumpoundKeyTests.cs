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
    public class CoumpoundKeyTests
    {
        public class Empolyee : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Age { get; set; }
        }

        [Fact]
        public void Fetch_by_key_value()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee, int, string>(h);

            while (h.Read())
            {
                var e = empCol.FetchByKeyValue(
                    (h.GetInt32(nameof(Empolyee.Id)), h.GetString(nameof(Empolyee.Name)))
                );
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        [Fact]
        public void Fetch_by_key_props()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee, int, string>(h);

            while (h.Read())
            {
                var e = empCol.FetchByKeyProps(
                    nameof(Empolyee.Id),
                    nameof(Empolyee.Name)
                );
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        private DtoMapper<Empolyee> _empMapper = new DtoMapper<Empolyee>()
            .SetKeyPropNames(nameof(Empolyee.Id), nameof(Empolyee.Name));

        [Fact]
        public void Fetch_using_mapper_instance_config()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee, int, string>(h, _empMapper);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        [KeyProps(nameof(Id), nameof(Name))]
        public class Empolyee_Attr : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Age { get; set; }
        }

        [Fact]
        public void Fetch_using_attributes_map_config()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee_Attr, int, string>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        [KeyProps(nameof(Id1), nameof(Id2), nameof(Id3), nameof(Id4))]
        public class SomeClass
        {
            public int Id1 { get; set; }
            public string Id2 { get; set; }
            public decimal Id3 { get; set; }
            public string Id4 { get; set; }

            public string Data1 { get; set; }
        }

        [Fact]
        public void Create_4_keyes_fetch_and_collection()
        {
            var fakeReader = new FakeDataReader("Id1", "Id2", "Id3", "Id4", "Data");
            fakeReader.AddRow(1, "2", Convert.ToDecimal(3), "4", "DATA-1");
            var h = new ReadHelper(fakeReader);
            var fetch = h.Fetch<SomeClass>();
            fetch.All();
        }
    }
}
