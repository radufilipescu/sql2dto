using sql2dto.Attributes;
using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace sql2dto.MSSqlServer.UnitTests
{
    public class ProjectMappingTests
    {
        public class sql2dto
        {
            public static readonly SqlBuilder SqlBuilder = new TSqlBuilder();

            public class dbo
            {
                public class Users : SqlTable
                {
                    public static Users As(string alias)
                    {
                        return new Users(alias);
                    }

                    private Users(string alias)
                        : base(nameof(dbo), nameof(Users), alias)
                    {
                        Id = DefineColumn(nameof(Id));
                        FirstName = DefineColumn(nameof(FirstName));
                        LastName = DefineColumn(nameof(LastName));
                        UserType = DefineColumn(nameof(UserType), "USER_TYPE");
                        ReportsToId = DefineColumn(nameof(ReportsToId));
                        Age = DefineColumn(nameof(Age));
                    }

                    public SqlColumn Id;
                    public SqlColumn FirstName;
                    public SqlColumn LastName;
                    public SqlColumn UserType;
                    public SqlColumn ReportsToId;
                    public SqlColumn Age;
                }
            }
        }

        public class User
        {
            [PropMap(IsKey = true, ColumnName = nameof(sql2dto.dbo.Users.Id))]
            public Int64 Identity { get; set; }

            [PropMap(ColumnName = nameof(sql2dto.dbo.Users.FirstName))]
            public string Forename { get; set; }

            [PropMap(ColumnName = nameof(sql2dto.dbo.Users.LastName))]
            public string Surname { get; set; }

            public string FirstName { get; set; }

            [PropMap(ColumnName = "LAST_NAME")]
            public string LastName { get; set; }

            public string UserType { get; set; }
            public Int64? ReportsToId { get; set; }
            public int? Age { get; set; }
        }

        [Fact]
        public async void Test1()
        {
            var u = sql2dto.dbo.Users.As("u");

            var query = sql2dto.SqlBuilder
                    .FetchQuery<User>(u)
                    .From(u);

            var queryStr = query.BuildQueryString();

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Server=srv-db;Database=sql2dto;User Id=sa;Password=@PentaQuark;"))
            {
                var result = await query.ExecAsync(conn);

                Assert.Equal(3, result.Count);

                Assert.Equal(1, result[0].Identity);
                Assert.Equal("Radu", result[0].Forename);
                Assert.Equal("Filipescu", result[0].Surname);
                Assert.Equal("Radu", result[0].FirstName);
                Assert.Equal("Filipescu", result[0].LastName);

                Assert.Equal(2, result[1].Identity);
                Assert.Equal("Cosmin", result[1].Forename);
                Assert.Equal("Ion", result[1].Surname);
                Assert.Equal("Cosmin", result[1].FirstName);
                Assert.Equal("Ion", result[1].LastName);

                Assert.Equal(3, result[2].Identity);
                Assert.Equal("Mihaita", result[2].Forename);
                Assert.Equal("Iacob", result[2].Surname);
                Assert.Equal("Mihaita", result[2].FirstName);
                Assert.Equal("Iacob", result[2].LastName);
            }
        }
    }
}
