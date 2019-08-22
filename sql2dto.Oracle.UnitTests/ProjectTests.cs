using sql2dto.Attributes;
using sql2dto.Core;
using System;
using System.Collections.Generic;
using Xunit;

namespace sql2dto.Oracle.UnitTests
{
    public class ProjectTests
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

                public class ZZZ_SQL2DTO_ADDRESSES : SqlTable
                {
                    public static ZZZ_SQL2DTO_ADDRESSES As(string alias)
                    {
                        return new ZZZ_SQL2DTO_ADDRESSES(alias);
                    }

                    private ZZZ_SQL2DTO_ADDRESSES(string alias)
                        : base(nameof(ADMINEMMETT), nameof(ZZZ_SQL2DTO_ADDRESSES), alias)
                    {
                        ID = DefineColumn(nameof(ID));
                        USERID = DefineColumn(nameof(USERID));
                        STREET = DefineColumn(nameof(STREET));
                        ISMAIN = DefineColumn(nameof(ISMAIN));
                    }

                    public SqlColumn ID;
                    public SqlColumn USERID;
                    public SqlColumn STREET;
                    public SqlColumn ISMAIN;
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

            public Decimal Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string UserType { get; set; }
            public Decimal? ReportsToId { get; set; }
            public Decimal? Age { get; set; }

            public string LifePeriod { get; set; }

            public List<Address> Addresses { get; set; }

            public User ReportsToUser { get; set; }
        }

        [KeyProps(nameof(Id))]
        [ColumnsPrefix(nameof(Address))]
        public class Address : IOnDtoRead
        {
            public Decimal Id { get; set; }
            public Decimal UserId { get; set; }
            [PropMap(ColumnName = "_STREET_")]
            public string Street { get; set; }

            public User User { get; set; }

            //[NullableIntToBooleanConverter]
            public bool IsCapitalCity { get; set; }

            public void OnDtoRead(Dictionary<string, object> rowValues)
            {

            }
        }

        public class UsersLifePeriod : SqlCommonTable
        {
            public static UsersLifePeriod As(string alias, SqlParameterExpression param2)
            {
                return new UsersLifePeriod(alias, param2);
            }

            private UsersLifePeriod(string tableAlias, SqlParameterExpression param2)
                : base(nameof(UsersLifePeriod), tableAlias)
            {
                ID = DefineColumn(nameof(ID));
                LIFEPERIOD = DefineColumn(nameof(LIFEPERIOD));

                _param2 = param2;
            }

            public SqlColumn ID;
            public SqlColumn LIFEPERIOD;

            private SqlParameterExpression _param2;

            public override SqlQuery Query()
            {
                var usr = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.As("usr");
                // !!! SELECT ORDER IN QUERY MUST MATCH THE ORDER OF DefineColumn CALLS IN CONSTRUCTOR
                return sql2dto.SqlBuilder.Query()
                        .Select(usr.ID) //Id
                        .Select(Sql.Case()
                                    .When(Sql.IsNull(usr.AGE), then: "UNKOWN")
                                    .When(usr.AGE >= Sql.Const(18), then: "Adult")
                                    .Else("Teenage")
                                .End()) //LifePeriod
                        .From(usr)
                        .Where(Sql.Const(2) == this._param2);
            }
        }

        [Fact]
        public async void Test1()
        {
            var param1 = sql2dto.SqlBuilder.Parameter("p_1", 1);
            var param2 = sql2dto.SqlBuilder.Parameter("p_2", 2);
            var param3 = sql2dto.SqlBuilder.Parameter("p_3", 3);

            var ulpCTE = UsersLifePeriod.As("ulpCTE", param2);

            var u = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.As("u");
            var a = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_ADDRESSES.As("a");
            var r = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.As("r");

            var uSub = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.As("uSub");

            var query = sql2dto.SqlBuilder.Query()

                //.With("ulp_cte",
                //() =>
                //{
                //    var usr = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.As("usr");

                //    return sql2dto.SqlBuilder.Query()
                //        .Select(usr.ID)
                //        .Select(Sql.Case()
                //                    .When(Sql.IsNull(usr.AGE), then: "UNKOWN")
                //                    .When(usr.AGE >= Sql.Const(18), then: "Adult")
                //                    .Else("Teenage")
                //                .End())
                //        .From(usr)
                //        .Where(Sql.Const(2) == param2);

                //}, nameof(User.Id), nameof(User.LifePeriod))

                .With(ulpCTE)

                .Project<User>(u)
                .Project<Address>(a)
                .Project<Address>(
                    (Sql.Case(a.STREET)
                        .When("B. Colentina", then: true)
                        .When("B. Stefan Cel Mare", then: true)
                        .Else(false)
                    .End(), nameof(Address.IsCapitalCity))
                )

                //.Project<Address>(
                //    (Sql.Cast(Sql.Case()
                //        .When(Sql.Like(a.STREET, "B.%"), then: true)
                //        .Else(false)
                //    .End(), to: "NUMBER(*)"), dto => dto.IsCapitalCity)
                //)

                .Project<Address>(
                    (Sql.Case()
                        .When(Sql.Like(a.STREET, "B.%"), then: true)
                        .Else(false)
                    .End(), dto => dto.IsCapitalCity)
                )
                .Project<User>("ReportsToUser", r, exceptColumns: r.REPORTSTOID)
                //.Project<User>((Sql.CTEColumn("ulp_cte", nameof(User.LifePeriod)), nameof(User.LifePeriod)))
                .Project<User>((ulpCTE.LIFEPERIOD, nameof(User.LifePeriod)))
                //

                .SelectSubQuery(_ => _.Select(Sql.Const(1)), "CONST_1")
                .SelectSubQuery(_ => _.Select(uSub.FIRSTNAME).From(uSub).Where(uSub.ID == u.ID), "SUBQ_FIRST_NAME")

                .Select(Sql.Sum(u.AGE, Sql.Over()), "AGE_SUM")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE)), "AGE_SUM_PART_TYPE")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID)), "AGE_SUM_ORD_ID")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID)), "AGE_SUM_PART_TYPE_ORD_ID")

                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).Rows(_ => _.CurrentRow())), "SUM_1")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).Range(_ => _.CurrentRow())), "SUM_2")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).Rows(_ => _.PrecedingUnbounded())), "SUM_3")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).Range(_ => _.PrecedingUnbounded())), "SUM_4")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).Rows(_ => _.PrecedingCount(5))), "SUM_5")

                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).RowsBetween(_ => _.CurrentRow(), _ => _.CurrentRow())), "SUM_6")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).RangeBetween(_ => _.CurrentRow(), _ => _.CurrentRow())), "SUM_7")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).RowsBetween(_ => _.PrecedingUnbounded(), _ => _.FollowingUnbounded())), "SUM_8")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).RangeBetween(_ => _.PrecedingUnbounded(), _ => _.FollowingUnbounded())), "SUM_9")
                .Select(Sql.Sum(u.AGE, Sql.Over().OrderBy(u.ID).RowsBetween(_ => _.PrecedingCount(5), (_ => _.FollowingCount(5)))), "SUM_10")

                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).Rows(_ => _.CurrentRow())), "SUM_11")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).Range(_ => _.CurrentRow())), "SUM_12")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).Rows(_ => _.PrecedingUnbounded())), "SUM_13")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).Range(_ => _.PrecedingUnbounded())), "SUM_14")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).Rows(_ => _.PrecedingCount(5))), "SUM_15")

                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).RowsBetween(_ => _.CurrentRow(), _ => _.CurrentRow())), "SUM_16")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).RangeBetween(_ => _.CurrentRow(), _ => _.CurrentRow())), "SUM_17")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).RowsBetween(_ => _.PrecedingUnbounded(), _ => _.FollowingUnbounded())), "SUM_18")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).RangeBetween(_ => _.PrecedingUnbounded(), _ => _.FollowingUnbounded())), "SUM_19")
                .Select(Sql.Sum(u.AGE, Sql.Over().PartitionBy(u.USERTYPE).OrderBy(u.ID).RowsBetween(_ => _.PrecedingCount(5), (_ => _.FollowingCount(5)))), "SUM_20")

                //TODO
                .Select(Sql.Concat(Sql.Concat(u.FIRSTNAME, Sql.Const("---")), u.LASTNAME), "CONCAT_NAMES")

                .From(u)
                .LeftJoin(a, on: u.ID == a.USERID)
                .LeftJoin(r, on: u.REPORTSTOID == r.ID)
                //.Join(Sql.CTE("ulp_cte"), on: Sql.CTEColumn("ulp_cte", nameof(User.Id)) == u.ID & Sql.Const(3) == param3)
                .Join(ulpCTE, on: ulpCTE.ID == u.ID & Sql.Const(3) == param3)
                //
                .JoinSubquery(
                    _ => _
                        .Select(uSub.ID, "id")
                        .From(uSub).As("jU"),
                    on: _ => _.GetColumn("id") == u.ID
                )
                .Where(
                    Sql.Const(1) == param1
                    & Sql.Const("abc").Like("%abc%")
                    & Sql.Const("%abc%").Like("!%abc!%", escapeChar: "!")
                )

                .OrderBy(u.ID);
                //.SkipRows(1)
                //.TakeRows(2);

            var sqlStr = query.BuildQueryString();

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=srv-db)(PORT=1521))(CONNECT_DATA=(SID=orcl)));User Id=ADMINEMMETT; Password=adminemmett;"))
            using (var h = await query.ExecReadHelperAsync(conn))
            {
                var fetch = h.Fetch<User>()
                                .Include<Address>((user, address) => { user.Addresses.Add(address); address.User = user; })
                                .Include<User>("ReportsToUser", (user, reportsToUser) => { user.ReportsToUser = reportsToUser; });

                var result = fetch.All();

                Assert.Equal(3, result.Count);
                Assert.Equal(2, result[0].Addresses.Count);
                Assert.False(result[0].Addresses[0].IsCapitalCity);
                Assert.True(result[0].Addresses[1].IsCapitalCity);
            }
        }

        [Fact]
        public async void Test2()
        {
            var u = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.As("u");
            var a = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_ADDRESSES.As("a");
            var r = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_USERS.As("r");

            var query = sql2dto.SqlBuilder
                    .FetchQuery<User>(u)
                        .Include<Address>(a, (user, address) => { user.Addresses.Add(address); address.User = user; })
                        .Include<User>(r, "ReportsToUser", (user, reportsToUser) => { user.ReportsToUser = reportsToUser; })

                    .Project<Address>(
                        (Sql.Case()
                            .When(Sql.Like(a.STREET, "B.%"), then: true)
                            .Else(false)
                        .End(), dto => dto.IsCapitalCity)
                    )

                    .From(u)
                    .LeftJoin(a, on: u.ID == a.USERID)
                    .LeftJoin(r, on: u.REPORTSTOID == r.ID)

                    .OrderBy(u.ID);

            var queryStr = query.BuildQueryString();

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=srv-db)(PORT=1521))(CONNECT_DATA=(SID=orcl)));User Id=ADMINEMMETT; Password=adminemmett;"))
            {
                var result = await query.ExecAsync(conn);

                Assert.Equal(3, result.Count);
                Assert.Equal(2, result[0].Addresses.Count);
                Assert.False(result[0].Addresses[0].IsCapitalCity);
                Assert.True(result[0].Addresses[1].IsCapitalCity);
            }
        }
    }
}
