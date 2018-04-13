using sql2dto.Attributes;
using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using Xunit;

namespace sql2dto.MSSqlServer.UnitTests
{
    public class ProjectTests
    {
        public class sql2dto
        {
            public static readonly SqlBuilder DefaultSqlBuilder = new TSqlBuilder();
            public static SqlParameterExpression Parameter(string name, object value)
            {
                return new SqlParameterExpression(new SqlParameter(name, value));
            }
            public static SqlQuery Query()
            {
                return new SqlQuery(DefaultSqlBuilder);
            }

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
                        Id = DefineColumn(nameof(Id), nameof(Id));
                        FirstName = DefineColumn(nameof(FirstName), nameof(FirstName));
                        LastName = DefineColumn(nameof(LastName), nameof(LastName));
                        UserType = DefineColumn(nameof(UserType), "USER_TYPE");
                        ReportsToId = DefineColumn(nameof(ReportsToId), nameof(ReportsToId));
                        Age = DefineColumn(nameof(Age), nameof(Age));
                    }

                    public SqlColumn Id;
                    public SqlColumn FirstName;
                    public SqlColumn LastName;
                    public SqlColumn UserType;
                    public SqlColumn ReportsToId;
                    public SqlColumn Age;
                }

                public class Addresses : SqlTable
                {
                    public static Addresses As(string alias)
                    {
                        return new Addresses(alias);
                    }

                    private Addresses(string alias)
                        : base(nameof(dbo), nameof(Addresses), alias)
                    {
                        Id = DefineColumn(nameof(Id), nameof(Id));
                        UserId = DefineColumn(nameof(UserId), nameof(UserId));
                        Street = DefineColumn(nameof(Street), nameof(Street));
                    }

                    public SqlColumn Id;
                    public SqlColumn UserId;
                    public SqlColumn Street;
                }
            }
        }

        [KeyProps(nameof(Id))]
        [ColumnsPrefix(nameof(User))]
        public class User
        {
            public User()
            {
                Addresses = new List<Address>();
            }

            public Int64 Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string UserType { get; set; }
            public Int64? ReportsToId { get; set; }
            public int? Age { get; set; }

            public string LifePeriod { get; set; }

            public List<Address> Addresses { get; set; }

            public User ReportsToUser { get; set; }
        }

        [KeyProps(nameof(Id))]
        [ColumnsPrefix(nameof(Address))]
        public class Address
        {
            public Int64 Id { get; set; }
            public Int64 UserId { get; set; }
            [PropMap(ColumnName = "_STREET_")]
            public string Street { get; set; }

            public User User { get; set; }
            public int IsCapitalCity { get; set; }
        }

        [Fact]
        public async void Test1()
        {
            //var ulp = sql2dto.dbo.Users.As("ulp");

            //var innerQuery = sql2dto.Query()
            //    .Select(ulp.Id, "ULPId")
            //    .Select(Sql.Case()
            //               .When(Sql.IsNull(ulp.Age), then: "UNKOWN")
            //               .When(ulp.Age >= Sql.Const(18), then: "Adult")
            //               .Else("Teenage")
            //            .End(), nameof(User.LifePeriod))
            //    .From(ulp)
            //    .As("ulp");

            var u = sql2dto.dbo.Users.As("u");
            var a = sql2dto.dbo.Addresses.As("a");
            var r = sql2dto.dbo.Users.As("r");

            var query = sql2dto.Query()

                .With("ulp_cte",
                () =>
                {
                    var usr = sql2dto.dbo.Users.As("usr");

                    return sql2dto.Query()
                        .Select(usr.Id)
                        .Select(Sql.Case()
                                    .When(Sql.IsNull(usr.Age), then: "UNKOWN")
                                    .When(usr.Age >= Sql.Const(18), then: "Adult")
                                    .Else("Teenage")
                                .End())
                        .From(usr)
                        .As("ulp");

                }, "ULPId", nameof(User.LifePeriod))

                .Project<User>(u)
                .Project<Address>(a)

                .Project<Address>(
                    (Sql.Case(a.Street)
                        .When("B. Colentina", then: 1)
                        .When("B. Stefan Cel Mare", then: 1)
                        .Else(0)
                    .End(), nameof(Address.IsCapitalCity))
                )

                .Project<Address>(
                    (Sql.Case()
                        .When(Sql.Like(a.Street, "B.%"), then: 1)
                        .Else(0)
                    .End(), dto => dto.IsCapitalCity)
                )

                .Project<User>("ReportsToUser", r, exceptColumns: r.ReportsToId)

                //.Project<User>((innerQuery.GetColumn(nameof(User.LifePeriod)), nameof(User.LifePeriod)))
                .Project<User>((Sql.CTEColumn("ulp_cte", nameof(User.LifePeriod)), nameof(User.LifePeriod)))


                .From(u)
                .LeftJoin(a, on: u.Id == a.UserId)
                .LeftJoin(r, on: u.ReportsToId == r.Id)

                .Join(Sql.CTE("ulp_cte"), on: Sql.CTEColumn("ulp_cte", "ULPId") == u.Id)
                ;
                //.Join(innerQuery, on: innerQuery.GetColumn("ULPId") == u.Id);

            using (var conn = new SqlConnection("Server=srv-db;Database=sql2dto;User Id=sa;Password=@PentaQuark;"))
            {
                await conn.OpenAsync();
                using (var cmd = query.BuildSqlCommand(conn))
                using (var h = await cmd.ExecReadHelperAsync())
                {
                    var fetch = h.Fetch<User>()
                        .Include<Address>((user, address) => { user.Addresses.Add(address); address.User = user; })
                        .Include<User>("ReportsToUser", (user, reportsToUser) => { user.ReportsToUser = reportsToUser; });

                    var result = fetch.All();
                }
            }
        }
    }
}
