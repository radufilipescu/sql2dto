using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Xunit;

namespace sql2dto.MSSqlServer.UnitTests
{
    public class UnitTest1
    {
        public class DataBaseName
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
                        Id = DefineColumn(nameof(Id));
                        Name = DefineColumn(nameof(Name));
                    }

                    public SqlColumn Id;
                    public SqlColumn Name;
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
                        Id = DefineColumn(nameof(Id));
                        UserId = DefineColumn(nameof(UserId));
                        Street = DefineColumn(nameof(Street));
                    }

                    public SqlColumn Id;
                    public SqlColumn UserId;
                    public SqlColumn Street;
                }
            }
        }

        [Fact]
        public void Test1()
        {
            var param1 = DataBaseName.Parameter("param1", 5);
            var param2 = DataBaseName.Parameter("param2", "foo");
            var innerParam1 = DataBaseName.Parameter("innerParam1", -1);
            var innerParam2 = DataBaseName.Parameter("innerParam2", -2);

            var u = DataBaseName.dbo.Users.As("u");
            var a = DataBaseName.dbo.Addresses.As("a");

            var innerQuery = DataBaseName.Query()
                .Select(a.Street)
                .From(a)
                .Where(a.Id == innerParam1)
                .As("INNER_Q");

            var query = DataBaseName.Query()
                .Select(a.Street)
                .Select(param1, "PARAM_1")
                .Select(Sql.Sum(a.Id), "SUM_AdrressId")
                .Select(Sql.Sum(a.Id))
                .Select(
                    u.Id,
                    Sql.Case()
                        .When(Sql.Const("panda") == a.Street, "bear")
                    .End()
                )
                .Select(u.Id & u.Name)
                .Select(
                    (Sql.Sum(u.Id + a.Id), "SUM_UserId_PLUS_AddressId"),
                    (Sql.SumDistinct(u.Id), "SUM_DISTINCT_UserId"),
                    (Sql.Sum(u.Id * u.Name), "SUM_UserId_AND_Name"),
                    (u.Id & u.Name, "exp1"),
                    (a.UserId == u.Id, "exp2"),
                    ((a.UserId == u.Id & u.Id == a.UserId) | a.Street, "exp3"),
                    (Sql.Case(u.Name)
                        .When("panda", "bear")
                        .When("koala", "bear")
                        .When("mastiff", "dog")
                        .When("terra nova", "dog")
                        .Else("unkown")
                    .End(), "ANIMAL_TYPE")
                )
                .From(u)
                .Join(a, on: a.UserId == u.Id & u.Id == a.UserId | a.Street == param2 + param1)
                .LeftJoin(a, on: (a.UserId == u.Id & u.Id == a.UserId) | a.Street == param2 + param1)
                .FullJoin(a, on: a.UserId == u.Id & (u.Id == a.UserId | a.Street == param2 + param1))
                .CrossJoin(a)
                .RightJoin(innerQuery, innerParam2 == Sql.Const(-1)) //TODO

                .Where(a.Street - a.Id == u.Id | u.Id * u.Name == param1 & a.Street == param2 + Sql.Const("FOO"))

                .GroupBy(a.Street, u.Name)
                .Having(Sql.Sum(u.Id) == Sql.Const(5) | Sql.SumDistinct(a.Id) == param2 & Sql.Sum(param1));

            var cmd = query.BuildDbCommand();

            string cmdText = cmd.CommandText;
            var parameters = cmd.Parameters;
        }
    }
}
