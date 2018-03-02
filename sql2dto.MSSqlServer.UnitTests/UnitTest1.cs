using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Xunit;

namespace sql2dto.MSSqlServer.UnitTests
{
    public class UnitTest1
    {
        public class Users : SqlTable
        {
            public static Users As(string alias)
            {
                return new Users(alias);
            }

            private Users(string alias)
                : base("dbo", "Users", alias)
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
                : base("dbo", "Addresses", alias)
            {
                Id = DefineColumn(nameof(Id));
                UserId = DefineColumn(nameof(UserId));
                Street = DefineColumn(nameof(Street));
            }

            public SqlColumn Id;
            public SqlColumn UserId;
            public SqlColumn Street;
        }

        [Fact]
        public void Test1()
        {
            var param1 = Sql.Parameter(new SqlParameter("param1", 5));
            var param2 = Sql.Parameter(new SqlParameter("param2", "foo"));

            var u = Users.As("u");
            var a = Addresses.As("a");

            var q = new SqlQuery(TSqlBuilder.Instance)
                .Select(a.Street)
                .Select(param1, "PARAM_1")
                .Select(Sql.Sum(a.Id), "SUM_AdrressId")
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
                    (Sql.Sum(u.Id & u.Name), "SUM_UserId_AND_Name"),
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

                .Where(a.Street - a.Id == u.Id | u.Id * u.Name == param1 & a.Street == param2 + Sql.Const("FOO"))

                .GroupBy(a.Street, u.Name)
                .Having(Sql.Sum(u.Id) == Sql.Const(5) | Sql.SumDistinct(a.Id) == param2);

            string result = q.BuildQueryString();
        }
    }
}
