using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.SetterTests
{
    public class PrivateSetterTests
    {
        public class Dto
        {
            public int Id { get; set; }

            public string Name { get; }

            public int Age { get; private set; }
        }

        [Fact]
        public void No_ctor_private_props()
        {
            //var obj = new Dto() { Age = 4 };

            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            fakeReader.AddRow(1, "Bob", 27);
            fakeReader.AddRow(2, "Mihaita", 30);
            fakeReader.AddRow(3, "Cosmin", 26);

            using (var h = new ReadHelper(fakeReader))
            {
                var result = h.Fetch<Dto>().All();

                Assert.Equal(1, result[0].Id);
                Assert.Equal(2, result[1].Id);
                Assert.Equal(3, result[2].Id);

                // because Name has no setter, it will be skipped
                Assert.Null(result[0].Name);
                Assert.Null(result[1].Name);
                Assert.Null(result[2].Name);

                Assert.Equal(27, result[0].Age);
                Assert.Equal(30, result[1].Age);
                Assert.Equal(26, result[2].Age);
            }
        }

        public class CtorDto
        {
            public CtorDto(int id, string name, int age)
            {
                Id = id;
                Name = name;
                Age = age;
            }

            public int Id { get; set; }

            public string Name { get; }

            public int Age { get; private set; }

            public int Grade { get; set; }

            public string NickName { get; }

            public int Performance { get; private set; }
        }

        [Fact]
        public void Ctor_private_props()
        {
            //var obj = new Dto() { Age = 4 };

            var fakeReader = new FakeDataReader("Id", "Name", "Age", "Grade", "NickName", "Performance");
            fakeReader.AddRow(1, "Radu", 27, 10, "Bob", 100);
            fakeReader.AddRow(2, "Mihaita", 30, 8, "Mike", 100);
            fakeReader.AddRow(3, null, 26, 5, null, 60);

            using (var h = new ReadHelper(fakeReader))
            {
                var result = h.Fetch<CtorDto>().All();

                Assert.Equal(1, result[0].Id);
                Assert.Equal(2, result[1].Id);
                Assert.Equal(3, result[2].Id);

                Assert.Equal("Radu", result[0].Name);
                Assert.Equal("Mihaita", result[1].Name);
                Assert.Null(result[2].Name);

                Assert.Equal(27, result[0].Age);
                Assert.Equal(30, result[1].Age);
                Assert.Equal(26, result[2].Age);

                Assert.Equal(10, result[0].Grade);
                Assert.Equal(8, result[1].Grade);
                Assert.Equal(5, result[2].Grade);

                // because NickName has no setter, it will be skipped
                Assert.Null(result[0].NickName);
                Assert.Null(result[1].NickName);
                Assert.Null(result[2].NickName);

                Assert.Equal(100, result[0].Performance);
                Assert.Equal(100, result[1].Performance);
                Assert.Equal(60, result[2].Performance);
            }
        }
    }
}
