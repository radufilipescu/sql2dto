﻿using sql2dto.Attributes;
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

        private DtoMapper<Empolyee> _empMapper = new DtoMapper<Empolyee>()
            .SetKeyPropNames(nameof(Empolyee.Id));

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

        [KeyProps(nameof(Id))]
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

            var empCol = new DtoCollection<Empolyee_Attr, int>(h);

            while (h.Read())
            {
                var e = empCol.Fetch();
            }

            KeyTestsData.AssertDataIntegrity(empCol.InnerList.Cast<IEmployee>());
        }
    }
}
