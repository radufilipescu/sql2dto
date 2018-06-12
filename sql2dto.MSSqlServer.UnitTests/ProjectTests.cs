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
        //public class NullableIntToBooleanConverterAttribute : ConverterAttribute
        //{
        //    public override Func<object, object> Converter => (v) => Convert.ToBoolean(v ?? 0);
        //}

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
                        IsMain = DefineColumn(nameof(IsMain));
                    }

                    public SqlColumn Id;
                    public SqlColumn UserId;
                    public SqlColumn Street;
                    public SqlColumn IsMain;
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

            //[NullableIntToBooleanConverter]
            public bool IsCapitalCity { get; set; }
        }

        [Fact]
        public async void Test1()
        {
            var param1 = sql2dto.SqlBuilder.Parameter("p_1", 1);
            var param2 = sql2dto.SqlBuilder.Parameter("p_2", 2);
            var param3 = sql2dto.SqlBuilder.Parameter("p_3", 3);

            var u = sql2dto.dbo.Users.As("u");
            var a = sql2dto.dbo.Addresses.As("a");
            var r = sql2dto.dbo.Users.As("r");

            var uSub = sql2dto.dbo.Users.As("uSub");

            var query = sql2dto.SqlBuilder.Query()

                .With("ulp_cte",
                () =>
                {
                    var usr = sql2dto.dbo.Users.As("usr");

                    return sql2dto.SqlBuilder.Query()
                        .Select(usr.Id)
                        .Select(Sql.Case()
                                    .When(Sql.IsNull(usr.Age), then: "UNKOWN")
                                    .When(usr.Age >= Sql.Const(18), then: "Adult")
                                    .Else("Teenage")
                                .End())
                        .From(usr)
                        .Where(Sql.Const(2) == param2);

                }, nameof(User.Id), nameof(User.LifePeriod))

                .Project<User>(u)
                .Project<Address>(a)
                .Project<Address>(
                    (Sql.Case(a.Street)
                        .When("B. Colentina", then: true)
                        .When("B. Stefan Cel Mare", then: true)
                        .Else(false)
                    .End(), nameof(Address.IsCapitalCity))
                )
                //.Project<Address>(
                //    (Sql.Cast(Sql.Case()
                //        .When(Sql.Like(a.Street, "B.%"), then: true)
                //        .Else(false)
                //    .End(), to: "BIT"), dto => dto.IsCapitalCity)
                //)
                .Project<Address>(
                    (Sql.Case()
                        .When(Sql.Like(a.Street, "B.%"), then: true)
                        .Else(false)
                    .End(), dto => dto.IsCapitalCity)
                )
                .Project<User>("ReportsToUser", r, exceptColumns: r.ReportsToId)
                .Project<User>((Sql.CTEColumn("ulp_cte", nameof(User.LifePeriod)), nameof(User.LifePeriod)))

                .SelectSubQuery(_ => _.Select(Sql.Const(1)), "CONST_1")
                .SelectSubQuery(_ => _.Select(uSub.FirstName).From(uSub).Where(uSub.Id == u.Id), "SUBQ_FIRST_NAME")

                .Select(Sql.Sum(u.Age, Sql.Over()), "AGE_SUM")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType)), "AGE_SUM_PART_TYPE")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id)), "AGE_SUM_ORD_ID")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id)), "AGE_SUM_PART_TYPE_ORD_ID")

                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).Rows(_ => _.CurrentRow())), "AGE_SUM_ORD_ID_ROWS_CURRENT_ROW")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).Range(_ => _.CurrentRow())), "AGE_SUM_ORD_ID_RANGE_CURRENT_ROW")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).Rows(_ => _.PrecedingUnbounded())), "AGE_SUM_ORD_ID_ROWS_PRECEDING_UNBOUNDED")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).Range(_ => _.PrecedingUnbounded())), "AGE_SUM_ORD_ID_RANGE_PRECEDING_UNBOUNDED")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).Rows(_ => _.PrecedingCount(5))), "AGE_SUM_ORD_ID_ROWS_PRECEDING_5")

                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).RowsBetween(_ => _.CurrentRow(), _ => _.CurrentRow())), "AGE_SUM_ORD_ID_ROWS_BETWEEN_CURRENT_ROW_AND_CURRENT_ROW")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).RangeBetween(_ => _.CurrentRow(), _ => _.CurrentRow())), "AGE_SUM_ORD_ID_RANGE_BETWEEN_CURRENT_ROW_AND_CURRENT_ROW")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).RowsBetween(_ => _.PrecedingUnbounded(), _ => _.FollowingUnbounded())), "AGE_SUM_ORD_ID_ROWS_BETWEEN_PRECEDING_UNBOUNDED_AND_FOLLOWING_UNBOUNDED")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).RangeBetween(_ => _.PrecedingUnbounded(), _ => _.FollowingUnbounded())), "AGE_SUM_ORD_ID_RANGE_BETWEEN_PRECEDING_UNBOUNDED_AND_FOLLOWING_UNBOUNDED")
                .Select(Sql.Sum(u.Age, Sql.Over().OrderBy(u.Id).RowsBetween(_ => _.PrecedingCount(5), (_ => _.FollowingCount(5)))), "AGE_SUM_ORD_ID_ROWS_BETWEEN_PRECEDING_5_AND_FOLLOWING_5")

                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).Rows(_ => _.CurrentRow())), "AGE_SUM_PART_TYPE_ORD_ID_ROWS_CURRENT_ROW")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).Range(_ => _.CurrentRow())), "AGE_SUM_PART_TYPE_ORD_ID_RANGE_CURRENT_ROW")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).Rows(_ => _.PrecedingUnbounded())), "AGE_SUM_PART_TYPE_ORD_ID_ROWS_PRECEDING_UNBOUNDED")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).Range(_ => _.PrecedingUnbounded())), "AGE_SUM_PART_TYPE_ORD_ID_RANGE_PRECEDING_UNBOUNDED")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).Rows(_ => _.PrecedingCount(5))), "AGE_SUM_PART_TYPE_ORD_ID_ROWS_PRECEDING_5")

                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).RowsBetween(_ => _.CurrentRow(), _ => _.CurrentRow())), "AGE_SUM_PART_TYPE_ORD_ID_ROWS_BETWEEN_CURRENT_ROW_AND_CURRENT_ROW")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).RangeBetween(_ => _.CurrentRow(), _ => _.CurrentRow())), "AGE_SUM_PART_TYPE_ORD_ID_RANGE_BETWEEN_CURRENT_ROW_AND_CURRENT_ROW")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).RowsBetween(_ => _.PrecedingUnbounded(), _ => _.FollowingUnbounded())), "AGE_SUM_PART_TYPE_ORD_ID_ROWS_BETWEEN_PRECEDING_UNBOUNDED_AND_FOLLOWING_UNBOUNDED")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).RangeBetween(_ => _.PrecedingUnbounded(), _ => _.FollowingUnbounded())), "AGE_SUM_PART_TYPE_ORD_ID_RANGE_BETWEEN_PRECEDING_UNBOUNDED_AND_FOLLOWING_UNBOUNDED")
                .Select(Sql.Sum(u.Age, Sql.Over().PartitionBy(u.UserType).OrderBy(u.Id).RowsBetween(_ => _.PrecedingCount(5), (_ => _.FollowingCount(5)))), "AGE_SUM_PART_TYPE_ORD_ID_ROWS_BETWEEN_PRECEDING_5_AND_FOLLOWING_5")

                .Select(Sql.Concat(u.FirstName, Sql.Const("---"), u.LastName), "CONCAT_NAMES")

                .From(u)
                .LeftJoin(a, on: u.Id == a.UserId)
                .LeftJoin(r, on: u.ReportsToId == r.Id)
                .Join(Sql.CTE("ulp_cte"), on: Sql.CTEColumn("ulp_cte", nameof(User.Id)) == u.Id & Sql.Const(3) == param3)
                .JoinSubquery(
                    _ => _ 
                        .Select(uSub.Id, "id")
                        .From(uSub).As("jU"), 
                    on: _ => _.GetColumn("id") == u.Id
                )
                .Where(
                    Sql.Const(1) == param1
                    & Sql.Const("abc").Like("%abc%")
                    & Sql.Const("%abc%").Like("!%abc!%", escapeChar: "!")
                );

                //.OrderBy(u.Id)
                //.SkipRows(1)
                //.TakeRows(2);

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Server=srv-db;Database=sql2dto;User Id=sa;Password=@PentaQuark;"))
            using (var h = await query.ExecReadHelperAsync(conn))
            {
                var fetch = h.Fetch<User>()
                                .Include<Address>((user, address) => { user.Addresses.Add(address); address.User = user; })
                                .Include<User>("ReportsToUser", (user, reportsToUser) => { user.ReportsToUser = reportsToUser; });

                var result = fetch.All();

                Assert.Equal(result.Count, 3);
                Assert.Equal(result[0].Addresses.Count, 2);
                Assert.Equal(result[0].Addresses[0].IsCapitalCity, false);
                Assert.Equal(result[0].Addresses[1].IsCapitalCity, true);
            }
        }
    }
}
