using sql2dto.Attributes;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.FetchOpTests
{
    public class IncludeTests
    {
        [ColumnsPrefix("a_")]
        [KeyProps(nameof(Id))]
        public class LevelA
        {
            public LevelA()
            {
                BLevels = new List<LevelB>();
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public List<LevelB> BLevels { get; set; }
        }

        [ColumnsPrefix("b_")]
        [KeyProps(nameof(Id))]
        public class LevelB
        {
            public LevelB()
            {
                CLevels = new List<LevelC>();
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public List<LevelC> CLevels { get; set; }
        }

        [ColumnsPrefix("c_")]
        [KeyProps(nameof(Id))]
        public class LevelC
        {
            public LevelC()
            {
                DLevels = new List<LevelD>();
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public List<LevelD> DLevels { get; set; }
        }

        [ColumnsPrefix("d_")]
        [KeyProps(nameof(Id))]
        public class LevelD
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        [Fact]
        public void Four_Levels_Include()
        {
            var dataReader = CreateDataReader();

            using (var h = new ReadHelper(dataReader))
            {
                var fetch = 
                    h.Fetch<LevelA>()
                        .Include<LevelB>((a, b) => { a.BLevels.Add(b); }, (bLevelOp) => { bLevelOp
                            .Include<LevelC>((b, c) => { b.CLevels.Add(c); }, (cLevelOp) => { cLevelOp
                                .Include<LevelD>((c, d) => { c.DLevels.Add(d); });
                            });
                        });

                var result = fetch.All();

                Assert.True(result.Count == 4);
                Assert.True(result.Count(a => a.Name == "A") == 4);
                foreach (var a in result)
                {
                    Assert.True(a.BLevels.Count == 4);
                    Assert.True(a.BLevels.Count(b => b.Name == "B") == 4);
                    foreach (var b in a.BLevels)
                    {
                        Assert.True(b.CLevels.Count == 4);
                        Assert.True(b.CLevels.Count(c => c.Name == "C") == 4);
                        foreach (var c in b.CLevels)
                        {
                            Assert.True(c.DLevels.Count == 4);
                            Assert.True(c.DLevels.Count(d => d.Name == "D") == 4);
                        }
                    }
                }
            }
        }

        public FakeDataReader CreateDataReader()
        {
            var fakeReader = new FakeDataReader("a_Id", "a_Name", "b_Id", "b_Name", "c_Id", "c_Name", "d_Id", "d_Name");

            fakeReader.AddRow(1, "A", 1, "B", 1, "C", 1, "D");
            fakeReader.AddRow(1, "A", 1, "B", 1, "C", 2, "D");
            fakeReader.AddRow(1, "A", 1, "B", 1, "C", 3, "D");
            fakeReader.AddRow(1, "A", 1, "B", 1, "C", 4, "D");
            fakeReader.AddRow(1, "A", 1, "B", 2, "C", 1, "D");
            fakeReader.AddRow(1, "A", 1, "B", 2, "C", 2, "D");
            fakeReader.AddRow(1, "A", 1, "B", 2, "C", 3, "D");
            fakeReader.AddRow(1, "A", 1, "B", 2, "C", 4, "D");
            fakeReader.AddRow(1, "A", 1, "B", 3, "C", 1, "D");
            fakeReader.AddRow(1, "A", 1, "B", 3, "C", 2, "D");
            fakeReader.AddRow(1, "A", 1, "B", 3, "C", 3, "D");
            fakeReader.AddRow(1, "A", 1, "B", 3, "C", 4, "D");
            fakeReader.AddRow(1, "A", 1, "B", 4, "C", 1, "D");
            fakeReader.AddRow(1, "A", 1, "B", 4, "C", 2, "D");
            fakeReader.AddRow(1, "A", 1, "B", 4, "C", 3, "D");
            fakeReader.AddRow(1, "A", 1, "B", 4, "C", 4, "D");

            fakeReader.AddRow(1, "A", 2, "B", 1, "C", 1, "D");
            fakeReader.AddRow(1, "A", 2, "B", 1, "C", 2, "D");
            fakeReader.AddRow(1, "A", 2, "B", 1, "C", 3, "D");
            fakeReader.AddRow(1, "A", 2, "B", 1, "C", 4, "D");
            fakeReader.AddRow(1, "A", 2, "B", 2, "C", 1, "D");
            fakeReader.AddRow(1, "A", 2, "B", 2, "C", 2, "D");
            fakeReader.AddRow(1, "A", 2, "B", 2, "C", 3, "D");
            fakeReader.AddRow(1, "A", 2, "B", 2, "C", 4, "D");
            fakeReader.AddRow(1, "A", 2, "B", 3, "C", 1, "D");
            fakeReader.AddRow(1, "A", 2, "B", 3, "C", 2, "D");
            fakeReader.AddRow(1, "A", 2, "B", 3, "C", 3, "D");
            fakeReader.AddRow(1, "A", 2, "B", 3, "C", 4, "D");
            fakeReader.AddRow(1, "A", 2, "B", 4, "C", 1, "D");
            fakeReader.AddRow(1, "A", 2, "B", 4, "C", 2, "D");
            fakeReader.AddRow(1, "A", 2, "B", 4, "C", 3, "D");
            fakeReader.AddRow(1, "A", 2, "B", 4, "C", 4, "D");

            fakeReader.AddRow(1, "A", 3, "B", 1, "C", 1, "D");
            fakeReader.AddRow(1, "A", 3, "B", 1, "C", 2, "D");
            fakeReader.AddRow(1, "A", 3, "B", 1, "C", 3, "D");
            fakeReader.AddRow(1, "A", 3, "B", 1, "C", 4, "D");
            fakeReader.AddRow(1, "A", 3, "B", 2, "C", 1, "D");
            fakeReader.AddRow(1, "A", 3, "B", 2, "C", 2, "D");
            fakeReader.AddRow(1, "A", 3, "B", 2, "C", 3, "D");
            fakeReader.AddRow(1, "A", 3, "B", 2, "C", 4, "D");
            fakeReader.AddRow(1, "A", 3, "B", 3, "C", 1, "D");
            fakeReader.AddRow(1, "A", 3, "B", 3, "C", 2, "D");
            fakeReader.AddRow(1, "A", 3, "B", 3, "C", 3, "D");
            fakeReader.AddRow(1, "A", 3, "B", 3, "C", 4, "D");
            fakeReader.AddRow(1, "A", 3, "B", 4, "C", 1, "D");
            fakeReader.AddRow(1, "A", 3, "B", 4, "C", 2, "D");
            fakeReader.AddRow(1, "A", 3, "B", 4, "C", 3, "D");
            fakeReader.AddRow(1, "A", 3, "B", 4, "C", 4, "D");

            fakeReader.AddRow(1, "A", 4, "B", 1, "C", 1, "D");
            fakeReader.AddRow(1, "A", 4, "B", 1, "C", 2, "D");
            fakeReader.AddRow(1, "A", 4, "B", 1, "C", 3, "D");
            fakeReader.AddRow(1, "A", 4, "B", 1, "C", 4, "D");
            fakeReader.AddRow(1, "A", 4, "B", 2, "C", 1, "D");
            fakeReader.AddRow(1, "A", 4, "B", 2, "C", 2, "D");
            fakeReader.AddRow(1, "A", 4, "B", 2, "C", 3, "D");
            fakeReader.AddRow(1, "A", 4, "B", 2, "C", 4, "D");
            fakeReader.AddRow(1, "A", 4, "B", 3, "C", 1, "D");
            fakeReader.AddRow(1, "A", 4, "B", 3, "C", 2, "D");
            fakeReader.AddRow(1, "A", 4, "B", 3, "C", 3, "D");
            fakeReader.AddRow(1, "A", 4, "B", 3, "C", 4, "D");
            fakeReader.AddRow(1, "A", 4, "B", 4, "C", 1, "D");
            fakeReader.AddRow(1, "A", 4, "B", 4, "C", 2, "D");
            fakeReader.AddRow(1, "A", 4, "B", 4, "C", 3, "D");
            fakeReader.AddRow(1, "A", 4, "B", 4, "C", 4, "D");







            fakeReader.AddRow(2, "A", 1, "B", 1, "C", 1, "D");
            fakeReader.AddRow(2, "A", 1, "B", 1, "C", 2, "D");
            fakeReader.AddRow(2, "A", 1, "B", 1, "C", 3, "D");
            fakeReader.AddRow(2, "A", 1, "B", 1, "C", 4, "D");
            fakeReader.AddRow(2, "A", 1, "B", 2, "C", 1, "D");
            fakeReader.AddRow(2, "A", 1, "B", 2, "C", 2, "D");
            fakeReader.AddRow(2, "A", 1, "B", 2, "C", 3, "D");
            fakeReader.AddRow(2, "A", 1, "B", 2, "C", 4, "D");
            fakeReader.AddRow(2, "A", 1, "B", 3, "C", 1, "D");
            fakeReader.AddRow(2, "A", 1, "B", 3, "C", 2, "D");
            fakeReader.AddRow(2, "A", 1, "B", 3, "C", 3, "D");
            fakeReader.AddRow(2, "A", 1, "B", 3, "C", 4, "D");
            fakeReader.AddRow(2, "A", 1, "B", 4, "C", 1, "D");
            fakeReader.AddRow(2, "A", 1, "B", 4, "C", 2, "D");
            fakeReader.AddRow(2, "A", 1, "B", 4, "C", 3, "D");
            fakeReader.AddRow(2, "A", 1, "B", 4, "C", 4, "D");

            fakeReader.AddRow(2, "A", 2, "B", 1, "C", 1, "D");
            fakeReader.AddRow(2, "A", 2, "B", 1, "C", 2, "D");
            fakeReader.AddRow(2, "A", 2, "B", 1, "C", 3, "D");
            fakeReader.AddRow(2, "A", 2, "B", 1, "C", 4, "D");
            fakeReader.AddRow(2, "A", 2, "B", 2, "C", 1, "D");
            fakeReader.AddRow(2, "A", 2, "B", 2, "C", 2, "D");
            fakeReader.AddRow(2, "A", 2, "B", 2, "C", 3, "D");
            fakeReader.AddRow(2, "A", 2, "B", 2, "C", 4, "D");
            fakeReader.AddRow(2, "A", 2, "B", 3, "C", 1, "D");
            fakeReader.AddRow(2, "A", 2, "B", 3, "C", 2, "D");
            fakeReader.AddRow(2, "A", 2, "B", 3, "C", 3, "D");
            fakeReader.AddRow(2, "A", 2, "B", 3, "C", 4, "D");
            fakeReader.AddRow(2, "A", 2, "B", 4, "C", 1, "D");
            fakeReader.AddRow(2, "A", 2, "B", 4, "C", 2, "D");
            fakeReader.AddRow(2, "A", 2, "B", 4, "C", 3, "D");
            fakeReader.AddRow(2, "A", 2, "B", 4, "C", 4, "D");

            fakeReader.AddRow(2, "A", 3, "B", 1, "C", 1, "D");
            fakeReader.AddRow(2, "A", 3, "B", 1, "C", 2, "D");
            fakeReader.AddRow(2, "A", 3, "B", 1, "C", 3, "D");
            fakeReader.AddRow(2, "A", 3, "B", 1, "C", 4, "D");
            fakeReader.AddRow(2, "A", 3, "B", 2, "C", 1, "D");
            fakeReader.AddRow(2, "A", 3, "B", 2, "C", 2, "D");
            fakeReader.AddRow(2, "A", 3, "B", 2, "C", 3, "D");
            fakeReader.AddRow(2, "A", 3, "B", 2, "C", 4, "D");
            fakeReader.AddRow(2, "A", 3, "B", 3, "C", 1, "D");
            fakeReader.AddRow(2, "A", 3, "B", 3, "C", 2, "D");
            fakeReader.AddRow(2, "A", 3, "B", 3, "C", 3, "D");
            fakeReader.AddRow(2, "A", 3, "B", 3, "C", 4, "D");
            fakeReader.AddRow(2, "A", 3, "B", 4, "C", 1, "D");
            fakeReader.AddRow(2, "A", 3, "B", 4, "C", 2, "D");
            fakeReader.AddRow(2, "A", 3, "B", 4, "C", 3, "D");
            fakeReader.AddRow(2, "A", 3, "B", 4, "C", 4, "D");

            fakeReader.AddRow(2, "A", 4, "B", 1, "C", 1, "D");
            fakeReader.AddRow(2, "A", 4, "B", 1, "C", 2, "D");
            fakeReader.AddRow(2, "A", 4, "B", 1, "C", 3, "D");
            fakeReader.AddRow(2, "A", 4, "B", 1, "C", 4, "D");
            fakeReader.AddRow(2, "A", 4, "B", 2, "C", 1, "D");
            fakeReader.AddRow(2, "A", 4, "B", 2, "C", 2, "D");
            fakeReader.AddRow(2, "A", 4, "B", 2, "C", 3, "D");
            fakeReader.AddRow(2, "A", 4, "B", 2, "C", 4, "D");
            fakeReader.AddRow(2, "A", 4, "B", 3, "C", 1, "D");
            fakeReader.AddRow(2, "A", 4, "B", 3, "C", 2, "D");
            fakeReader.AddRow(2, "A", 4, "B", 3, "C", 3, "D");
            fakeReader.AddRow(2, "A", 4, "B", 3, "C", 4, "D");
            fakeReader.AddRow(2, "A", 4, "B", 4, "C", 1, "D");
            fakeReader.AddRow(2, "A", 4, "B", 4, "C", 2, "D");
            fakeReader.AddRow(2, "A", 4, "B", 4, "C", 3, "D");
            fakeReader.AddRow(2, "A", 4, "B", 4, "C", 4, "D");




            fakeReader.AddRow(3, "A", 1, "B", 1, "C", 1, "D");
            fakeReader.AddRow(3, "A", 1, "B", 1, "C", 2, "D");
            fakeReader.AddRow(3, "A", 1, "B", 1, "C", 3, "D");
            fakeReader.AddRow(3, "A", 1, "B", 1, "C", 4, "D");
            fakeReader.AddRow(3, "A", 1, "B", 2, "C", 1, "D");
            fakeReader.AddRow(3, "A", 1, "B", 2, "C", 2, "D");
            fakeReader.AddRow(3, "A", 1, "B", 2, "C", 3, "D");
            fakeReader.AddRow(3, "A", 1, "B", 2, "C", 4, "D");
            fakeReader.AddRow(3, "A", 1, "B", 3, "C", 1, "D");
            fakeReader.AddRow(3, "A", 1, "B", 3, "C", 2, "D");
            fakeReader.AddRow(3, "A", 1, "B", 3, "C", 3, "D");
            fakeReader.AddRow(3, "A", 1, "B", 3, "C", 4, "D");
            fakeReader.AddRow(3, "A", 1, "B", 4, "C", 1, "D");
            fakeReader.AddRow(3, "A", 1, "B", 4, "C", 2, "D");
            fakeReader.AddRow(3, "A", 1, "B", 4, "C", 3, "D");
            fakeReader.AddRow(3, "A", 1, "B", 4, "C", 4, "D");

            fakeReader.AddRow(3, "A", 2, "B", 1, "C", 1, "D");
            fakeReader.AddRow(3, "A", 2, "B", 1, "C", 2, "D");
            fakeReader.AddRow(3, "A", 2, "B", 1, "C", 3, "D");
            fakeReader.AddRow(3, "A", 2, "B", 1, "C", 4, "D");
            fakeReader.AddRow(3, "A", 2, "B", 2, "C", 1, "D");
            fakeReader.AddRow(3, "A", 2, "B", 2, "C", 2, "D");
            fakeReader.AddRow(3, "A", 2, "B", 2, "C", 3, "D");
            fakeReader.AddRow(3, "A", 2, "B", 2, "C", 4, "D");
            fakeReader.AddRow(3, "A", 2, "B", 3, "C", 1, "D");
            fakeReader.AddRow(3, "A", 2, "B", 3, "C", 2, "D");
            fakeReader.AddRow(3, "A", 2, "B", 3, "C", 3, "D");
            fakeReader.AddRow(3, "A", 2, "B", 3, "C", 4, "D");
            fakeReader.AddRow(3, "A", 2, "B", 4, "C", 1, "D");
            fakeReader.AddRow(3, "A", 2, "B", 4, "C", 2, "D");
            fakeReader.AddRow(3, "A", 2, "B", 4, "C", 3, "D");
            fakeReader.AddRow(3, "A", 2, "B", 4, "C", 4, "D");

            fakeReader.AddRow(3, "A", 3, "B", 1, "C", 1, "D");
            fakeReader.AddRow(3, "A", 3, "B", 1, "C", 2, "D");
            fakeReader.AddRow(3, "A", 3, "B", 1, "C", 3, "D");
            fakeReader.AddRow(3, "A", 3, "B", 1, "C", 4, "D");
            fakeReader.AddRow(3, "A", 3, "B", 2, "C", 1, "D");
            fakeReader.AddRow(3, "A", 3, "B", 2, "C", 2, "D");
            fakeReader.AddRow(3, "A", 3, "B", 2, "C", 3, "D");
            fakeReader.AddRow(3, "A", 3, "B", 2, "C", 4, "D");
            fakeReader.AddRow(3, "A", 3, "B", 3, "C", 1, "D");
            fakeReader.AddRow(3, "A", 3, "B", 3, "C", 2, "D");
            fakeReader.AddRow(3, "A", 3, "B", 3, "C", 3, "D");
            fakeReader.AddRow(3, "A", 3, "B", 3, "C", 4, "D");
            fakeReader.AddRow(3, "A", 3, "B", 4, "C", 1, "D");
            fakeReader.AddRow(3, "A", 3, "B", 4, "C", 2, "D");
            fakeReader.AddRow(3, "A", 3, "B", 4, "C", 3, "D");
            fakeReader.AddRow(3, "A", 3, "B", 4, "C", 4, "D");

            fakeReader.AddRow(3, "A", 4, "B", 1, "C", 1, "D");
            fakeReader.AddRow(3, "A", 4, "B", 1, "C", 2, "D");
            fakeReader.AddRow(3, "A", 4, "B", 1, "C", 3, "D");
            fakeReader.AddRow(3, "A", 4, "B", 1, "C", 4, "D");
            fakeReader.AddRow(3, "A", 4, "B", 2, "C", 1, "D");
            fakeReader.AddRow(3, "A", 4, "B", 2, "C", 2, "D");
            fakeReader.AddRow(3, "A", 4, "B", 2, "C", 3, "D");
            fakeReader.AddRow(3, "A", 4, "B", 2, "C", 4, "D");
            fakeReader.AddRow(3, "A", 4, "B", 3, "C", 1, "D");
            fakeReader.AddRow(3, "A", 4, "B", 3, "C", 2, "D");
            fakeReader.AddRow(3, "A", 4, "B", 3, "C", 3, "D");
            fakeReader.AddRow(3, "A", 4, "B", 3, "C", 4, "D");
            fakeReader.AddRow(3, "A", 4, "B", 4, "C", 1, "D");
            fakeReader.AddRow(3, "A", 4, "B", 4, "C", 2, "D");
            fakeReader.AddRow(3, "A", 4, "B", 4, "C", 3, "D");
            fakeReader.AddRow(3, "A", 4, "B", 4, "C", 4, "D");





            fakeReader.AddRow(4, "A", 1, "B", 1, "C", 1, "D");
            fakeReader.AddRow(4, "A", 1, "B", 1, "C", 2, "D");
            fakeReader.AddRow(4, "A", 1, "B", 1, "C", 3, "D");
            fakeReader.AddRow(4, "A", 1, "B", 1, "C", 4, "D");
            fakeReader.AddRow(4, "A", 1, "B", 2, "C", 1, "D");
            fakeReader.AddRow(4, "A", 1, "B", 2, "C", 2, "D");
            fakeReader.AddRow(4, "A", 1, "B", 2, "C", 3, "D");
            fakeReader.AddRow(4, "A", 1, "B", 2, "C", 4, "D");
            fakeReader.AddRow(4, "A", 1, "B", 3, "C", 1, "D");
            fakeReader.AddRow(4, "A", 1, "B", 3, "C", 2, "D");
            fakeReader.AddRow(4, "A", 1, "B", 3, "C", 3, "D");
            fakeReader.AddRow(4, "A", 1, "B", 3, "C", 4, "D");
            fakeReader.AddRow(4, "A", 1, "B", 4, "C", 1, "D");
            fakeReader.AddRow(4, "A", 1, "B", 4, "C", 2, "D");
            fakeReader.AddRow(4, "A", 1, "B", 4, "C", 3, "D");
            fakeReader.AddRow(4, "A", 1, "B", 4, "C", 4, "D");

            fakeReader.AddRow(4, "A", 2, "B", 1, "C", 1, "D");
            fakeReader.AddRow(4, "A", 2, "B", 1, "C", 2, "D");
            fakeReader.AddRow(4, "A", 2, "B", 1, "C", 3, "D");
            fakeReader.AddRow(4, "A", 2, "B", 1, "C", 4, "D");
            fakeReader.AddRow(4, "A", 2, "B", 2, "C", 1, "D");
            fakeReader.AddRow(4, "A", 2, "B", 2, "C", 2, "D");
            fakeReader.AddRow(4, "A", 2, "B", 2, "C", 3, "D");
            fakeReader.AddRow(4, "A", 2, "B", 2, "C", 4, "D");
            fakeReader.AddRow(4, "A", 2, "B", 3, "C", 1, "D");
            fakeReader.AddRow(4, "A", 2, "B", 3, "C", 2, "D");
            fakeReader.AddRow(4, "A", 2, "B", 3, "C", 3, "D");
            fakeReader.AddRow(4, "A", 2, "B", 3, "C", 4, "D");
            fakeReader.AddRow(4, "A", 2, "B", 4, "C", 1, "D");
            fakeReader.AddRow(4, "A", 2, "B", 4, "C", 2, "D");
            fakeReader.AddRow(4, "A", 2, "B", 4, "C", 3, "D");
            fakeReader.AddRow(4, "A", 2, "B", 4, "C", 4, "D");

            fakeReader.AddRow(4, "A", 3, "B", 1, "C", 1, "D");
            fakeReader.AddRow(4, "A", 3, "B", 1, "C", 2, "D");
            fakeReader.AddRow(4, "A", 3, "B", 1, "C", 3, "D");
            fakeReader.AddRow(4, "A", 3, "B", 1, "C", 4, "D");
            fakeReader.AddRow(4, "A", 3, "B", 2, "C", 1, "D");
            fakeReader.AddRow(4, "A", 3, "B", 2, "C", 2, "D");
            fakeReader.AddRow(4, "A", 3, "B", 2, "C", 3, "D");
            fakeReader.AddRow(4, "A", 3, "B", 2, "C", 4, "D");
            fakeReader.AddRow(4, "A", 3, "B", 3, "C", 1, "D");
            fakeReader.AddRow(4, "A", 3, "B", 3, "C", 2, "D");
            fakeReader.AddRow(4, "A", 3, "B", 3, "C", 3, "D");
            fakeReader.AddRow(4, "A", 3, "B", 3, "C", 4, "D");
            fakeReader.AddRow(4, "A", 3, "B", 4, "C", 1, "D");
            fakeReader.AddRow(4, "A", 3, "B", 4, "C", 2, "D");
            fakeReader.AddRow(4, "A", 3, "B", 4, "C", 3, "D");
            fakeReader.AddRow(4, "A", 3, "B", 4, "C", 4, "D");

            fakeReader.AddRow(4, "A", 4, "B", 1, "C", 1, "D");
            fakeReader.AddRow(4, "A", 4, "B", 1, "C", 2, "D");
            fakeReader.AddRow(4, "A", 4, "B", 1, "C", 3, "D");
            fakeReader.AddRow(4, "A", 4, "B", 1, "C", 4, "D");
            fakeReader.AddRow(4, "A", 4, "B", 2, "C", 1, "D");
            fakeReader.AddRow(4, "A", 4, "B", 2, "C", 2, "D");
            fakeReader.AddRow(4, "A", 4, "B", 2, "C", 3, "D");
            fakeReader.AddRow(4, "A", 4, "B", 2, "C", 4, "D");
            fakeReader.AddRow(4, "A", 4, "B", 3, "C", 1, "D");
            fakeReader.AddRow(4, "A", 4, "B", 3, "C", 2, "D");
            fakeReader.AddRow(4, "A", 4, "B", 3, "C", 3, "D");
            fakeReader.AddRow(4, "A", 4, "B", 3, "C", 4, "D");
            fakeReader.AddRow(4, "A", 4, "B", 4, "C", 1, "D");
            fakeReader.AddRow(4, "A", 4, "B", 4, "C", 2, "D");
            fakeReader.AddRow(4, "A", 4, "B", 4, "C", 3, "D");
            fakeReader.AddRow(4, "A", 4, "B", 4, "C", 4, "D");

            return fakeReader;
        }
    }
}
