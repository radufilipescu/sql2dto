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
                    }

                    public SqlColumn Id;
                    public SqlColumn FirstName;
                    public SqlColumn LastName;
                    public SqlColumn UserType;
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

            public List<Address> Addresses { get; set; }
        }

        [KeyProps(nameof(Id))]
        [ColumnsPrefix(nameof(Address))]
        public class Address
        {
            public Int64 Id { get; set; }
            public Int64 UserId { get; set; }
            public string Street { get; set; }

            public User User { get; set; }
            public int IsCapitalCity { get; set; }
        }

        [Fact]
        public async void Test1()
        {
            var u = sql2dto.dbo.Users.As("u");
            var a = sql2dto.dbo.Addresses.As("a");

            var query = sql2dto.Query()
                .Project<User>(u)
                .Project<Address>(a)
                .Project<Address>(
                    (Sql.Case(a.Street)
                        .When("Colentina", 1)
                        .When("Stefan Cel Mare", 1)
                        .Else(0)
                    .End(), nameof(Address.IsCapitalCity))
                )
                .From(u)
                .LeftJoin(a, on: u.Id == a.UserId);

            using (var conn = new SqlConnection("Server=srv-db;Database=sql2dto;User Id=sa;Password=@PentaQuark;"))
            {
                await conn.OpenAsync();
                using (var cmd = query.BuildSqlCommand(conn))
                using (var h = await cmd.ExecReadHelperAsync())
                {
                    var fetch = h.Fetch<User>()
                        .Include<Address>((user, address) => { user.Addresses.Add(address); address.User = user; });

                    var result = fetch.All();
                }
            }
        }

    }
}
