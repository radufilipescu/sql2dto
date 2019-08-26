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
            public double Age { get; set; }
        }

        [Fact]
        public void Fetch_by_key_value()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
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
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee, int>(h);

            while (h.Read())
            {
                var e = empCol.FetchByKeyProps(nameof(Empolyee.Id));
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        private DtoMapper<Empolyee> _empMapper = DtoMapper<Empolyee>.Default.Clone()
            .SetKeyProps(_ => _.Id);

        [Fact]
        public void Fetch_using_mapper_instance_config()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee, int>(h, _empMapper);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        public class Empolyee_WithDefaultMapper : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Age { get; set; }
        }

        [Fact]
        public void Fetch_using_default_mapper_config()
        {
            DtoMapper<Empolyee_WithDefaultMapper>.Default
                .SetColumnsPrefix("EMP_")
                .SetKeyProps(_ => _.Id);

            var fakeReader = new FakeDataReader("EMP_Id", "EMP_Name", "EMP_Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee_WithDefaultMapper, int>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        [KeyProps(nameof(Id))]
        public class Empolyee_Attr : IEmployee
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Age { get; set; }
        }

        [Fact]
        public void Fetch_using_key_props_attribute()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee_Attr, int>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }

        public class Empolyee_Attr2 : IEmployee
        {
            [PropMap(IsKey = true)]
            public int Id { get; set; }
            public string Name { get; set; }
            public double Age { get; set; }
        }

        [Fact]
        public void Fetch_using_prop_map_attribute_with_is_key()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            KeyTestsData.SetupEmployeesData(fakeReader);

            var h = new ReadHelper(fakeReader);

            var empCol = new DtoCollection<Empolyee_Attr, int>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
