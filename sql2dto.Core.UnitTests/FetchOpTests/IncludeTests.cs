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

            public LevelA ALevel;
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

            public LevelB BLevel;
            public List<LevelD> DLevels { get; set; }
        }

        [ColumnsPrefix("d_")]
        [KeyProps(nameof(Id))]
        public class LevelD
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public LevelC CLevel;
        }

        [Fact]
        public void Do_Not_Include_Null_Children()
        {
            var fakeReader = new FakeDataReader("a_Id", "a_Name", "b_Id", "b_Name", "c_Id", "c_Name", "d_Id", "d_Name");

            fakeReader.AddRow(1, "A", 1, "B", 1, "C", 1, "D");
            fakeReader.AddRow(1, "A", 2, "B", 2, "C", 2, "D");

            fakeReader.AddRow(2, "A", null, null, null, null, null, null);
            fakeReader.AddRow(2, "A", null, null, null, null, null, null);

            fakeReader.AddRow(3, "A", 3, "B", null, null, null, null);
            fakeReader.AddRow(3, "A", 4, "B", 4, "C", null, null);

            fakeReader.AddRow(4, "A", 5, "B", 5, "C", null, null);
            fakeReader.AddRow(4, "A", 6, "B", null, null, null, null);

            fakeReader.AddRow(5, "A", 7, "B", null, null, null, null);
            fakeReader.AddRow(5, "A", 7, "B", null, null, null, null);

            fakeReader.AddRow(6, "A", 8, "B", null, null, null, null);
            fakeReader.AddRow(6, "A", 8, "B", 6, "C", null, null);
            fakeReader.AddRow(6, "A", 8, "B", 7, "C", null, null);

            using (var h = new ReadHelper(fakeReader))
            {
                var fetch = 
                    h.Fetch<LevelA>()
                        .Include<LevelB>((a, b) => { a.BLevels.Add(b); b.ALevel = a; }, (bLevelOp) => { bLevelOp
                            .Include<LevelC>((b, c) => { b.CLevels.Add(c); c.BLevel = b; }, (cLevelOp) => { cLevelOp
                                .Include<LevelD>((c, d) => { c.DLevels.Add(d); d.CLevel = c; });
                            });
                        });

                var result = fetch.All();

                Assert.True(result.Count == 6);

                Assert.True(result[0].BLevels.Count == 2);
                Assert.True(result[0].BLevels[0].CLevels.Count == 1);
                Assert.True(result[0].BLevels[1].CLevels.Count == 1);

                Assert.True(result[1].BLevels.Count == 0);

                Assert.True(result[2].BLevels.Count == 2);
                Assert.True(result[2].BLevels[0].CLevels.Count == 0);
                Assert.True(result[2].BLevels[1].CLevels.Count == 1);

                Assert.True(result[3].BLevels.Count == 2);
                Assert.True(result[3].BLevels[0].CLevels.Count == 1);
                Assert.True(result[3].BLevels[1].CLevels.Count == 0);

                Assert.True(result[4].BLevels.Count == 1);

                Assert.True(result[5].BLevels.Count == 1);
                Assert.True(result[5].BLevels[0].CLevels.Count == 2);
            }
        }

        [Fact]
        public void Four_Levels_Include()
        {
            var dataReader = CreateDataReader();

            using (var h = new ReadHelper(dataReader))
            {
                var fetch = 
                    h.Fetch<LevelA>()
                        .Include<LevelB>((a, b) => { a.BLevels.Add(b); b.ALevel = a; }, (bLevelOp) => { bLevelOp
                            .Include<LevelC>((b, c) => { b.CLevels.Add(c); c.BLevel = b; }, (cLevelOp) => { cLevelOp
                                .Include<LevelD>((c, d) => { c.DLevels.Add(d); d.CLevel = c; });
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
