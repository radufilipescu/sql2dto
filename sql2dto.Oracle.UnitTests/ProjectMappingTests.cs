using sql2dto.Attributes;
using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace sql2dto.Oracle.UnitTests
{
    public class ProjectMappingTests
    {
        public class sql2dto
        {
            public static readonly SqlBuilder SqlBuilder = new PLSqlBuilder();

            public class ADMINEMMETT
            {
                public class ZZZ_SQL2DTO_USERS : SqlTable
                {
                    public static ZZZ_SQL2DTO_USERS As(string alias)
                    {
                        return new ZZZ_SQL2DTO_USERS(alias);
                    }

                    private ZZZ_SQL2DTO_USERS(string alias)
                        : base(nameof(ADMINEMMETT), nameof(ZZZ_SQL2DTO_USERS), alias)
                    {
                        ID = DefineColumn(nameof(ID));
                        FIRSTNAME = DefineColumn(nameof(FIRSTNAME));
                        LASTNAME = DefineColumn(nameof(LASTNAME));
                        USERTYPE = DefineColumn(nameof(USERTYPE), "USER_TYPE");
                        REPORTSTOID = DefineColumn(nameof(REPORTSTOID));
                        AGE = DefineColumn(nameof(AGE));
                    }

                    public SqlColumn ID;
                    public SqlColumn FIRSTNAME;
                    public SqlColumn LASTNAME;
                    public SqlColumn USERTYPE;
                    public SqlColumn REPORTSTOID;
                    public SqlColumn AGE;
                }
            }
        }

        public class User
        {
            [PropMap(IsKey = true, ColumnName = nameof(sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.ID))]
            public decimal Identity { get; set; }

            [PropMap(ColumnName = nameof(sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.FIRSTNAME))]
            public string Forename { get; set; }

            [PropMap(ColumnName = nameof(sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.LASTNAME))]
            public string Surname { get; set; }

            public string FirstName { get; set; }

            [PropMap(ColumnName = "LAST_NAME")]
            public string LastName { get; set; }

            public string UserType { get; set; }
            public decimal? ReportsToId { get; set; }
            public int? Age { get; set; }
        }

        [Fact]
        public async void Test1()
        {
            var u = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.As("u");

            var query = sql2dto.SqlBuilder
                    .FetchQuery<User>(u)
                    .From(u);

            var queryStr = query.BuildQueryString();

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=srv-db)(PORT=1521))(CONNECT_DATA=(SID=orcl)));User Id=ADMINEMMETT; Password=adminemmett;"))
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
