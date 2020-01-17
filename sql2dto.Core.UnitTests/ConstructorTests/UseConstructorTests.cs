using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.ConstructorTests
{
    public class UseConstructorTests
    {
        public class SomeDto
        {
            public SomeDto(int id, string name)
            {
                Id = id;
                _name = name;
            }

            public SomeDto(int id, string name, int age)
            {
                Id = id;
                _name = name;
                Age = age;
            }

            public int Id { get; private set; }

            private string _name;
            public string Name 
            { 
                get => _name;
                set 
                {
                    if (Id > 1)
                    {
                        throw new Exception("You are not allowed to change your name");
                    }
                    _name = value; 
                }
            }

            public int Age { get; set; }
        }

        [Fact]
        public void Use_default_constructor()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            fakeReader.AddRow(1, "Bob", 27);
            fakeReader.AddRow(2, "Mihaita", 30);
            fakeReader.AddRow(3, "Cosmin", 26);

            using (var h = new ReadHelper(fakeReader))
            {
                var result = h.Fetch<SomeDto>().All();
            }
        }

        [Fact]
        public void Use_specific_constructor()
        {
            var ctorInfo = typeof(SomeDto).GetConstructor(new Type[] { typeof(int), typeof(string), typeof(int) });

            var mapper = DtoMapper<SomeDto>.Default.Clone()
                .UseConstructor(ctorInfo);

            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            fakeReader.AddRow(1, "Bob", 27);
            fakeReader.AddRow(2, "Mihaita", 30);
            fakeReader.AddRow(3, "Cosmin", 26);

            using (var h = new ReadHelper(fakeReader))
            {
                var result = h.Fetch<SomeDto>(mapper).All();
            }
        }

        public class PrivateCtorOnlyClass
        {
            private PrivateCtorOnlyClass()
            {

            }

            public int Id { get; private set; }

            public string Name { get; private set; }

            public int Age { get; set; }
        }

        [Fact]
        public void Using_default_private_constructor_will_throw_exception()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            fakeReader.AddRow(1, "Bob", 27);
            fakeReader.AddRow(2, "Mihaita", 30);
            fakeReader.AddRow(3, "Cosmin", 26);

            using (var h = new ReadHelper(fakeReader))
            {
                try
                {
                    h.Fetch<PrivateCtorOnlyClass>().All();
                    Assert.False(true, "Exception must be thrown when Dto class contains onyly private constructors");
                }
                catch (Exception ex)
                {
                    Assert.NotNull(ex.Message);
                }
            }
        }

        public class PrivateAndPublicCtorClass
        {
            private PrivateAndPublicCtorClass()
            {

            }

            public PrivateAndPublicCtorClass(int id)
            {
                Id = id;
            }

            public int Id { get; private set; }

            public string Name { get; private set; }

            public int Age { get; set; }
        }

        [Fact]
        public void Using_specific_private_constructor_will_throw_exception()
        {
            var privateCtorInfo = typeof(PrivateAndPublicCtorClass).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];

            var mapper = DtoMapper<PrivateAndPublicCtorClass>.Default.Clone();

            try
            {
                mapper.UseConstructor(privateCtorInfo);
                Assert.False(true, "Exception must be thrown when mapper uses a specific private contructor from Dto class");
            }
            catch (Exception ex)
            {
                Assert.NotNull(ex.Message);
            }
        }

        public class UnmatchingCtorParamsClass
        {
            private readonly string _name;

            public UnmatchingCtorParamsClass(int id, string name)
            {
                Id = id;
                _name = name;
            }

            public int Id { get; private set; }

            public int Age { get; set; }
        }

        [Fact]
        public void Unmatching_ctor_parameters_with_props_will_throw_exception()
        {
            var fakeReader = new FakeDataReader("Id", "Name", "Age");
            fakeReader.AddRow(1, "Bob", 27);
            fakeReader.AddRow(2, "Mihaita", 30);
            fakeReader.AddRow(3, "Cosmin", 26);

            using (var h = new ReadHelper(fakeReader))
            {
                try
                {
                    h.Fetch<UnmatchingCtorParamsClass>().All();
                    Assert.False(true, "Exception must be thrown when Dto constructor parameters do not have matching properties");
                }
                catch (Exception ex)
                {
                    Assert.NotNull(ex.Message);
                }
            }
        }
    }
}
