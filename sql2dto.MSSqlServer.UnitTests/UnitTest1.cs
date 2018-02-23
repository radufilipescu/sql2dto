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
            var u = Users.As("u");
            var a = Addresses.As("a");

            var q = new SqlQuery(TSqlBuilder.Instance)
                .Select(a.Street)
                .Select(
                    u.Id, 
                    u.Name
                )
                .Select(u.Id & u.Name)
                .Select(
                    (u.Id & u.Name, "exp1"), 
                    (a.UserId == u.Id, "exp2"), 
                    ((a.UserId == u.Id & u.Id == a.UserId) | a.Street, "exp3")
                )
                .From(u)
                .InnerJoin(a, on: (a.UserId == u.Id & u.Id == a.UserId) | a.Street);

            string result = q.BuildQueryString();
        }
    }
}
