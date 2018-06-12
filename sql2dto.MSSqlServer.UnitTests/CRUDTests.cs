using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using Xunit;

namespace sql2dto.MSSqlServer.UnitTests
{
    public class CRUDTests
    {
        public class sql2dto
        {
            public static readonly SqlBuilder SqlBuilder = new TSqlBuilder();

            public class dbo
            {
                public class Dummies : SqlTable
                {
                    public static Dummies As(string alias)
                    {
                        return new Dummies(alias);
                    }

                    private Dummies(string alias)
                        : base(nameof(dbo), nameof(Dummies), alias)
                    {
                        ColInt = DefineColumn(nameof(ColInt));
                        ColString = DefineColumn(nameof(ColString));
                    }

                    public SqlColumn ColInt;
                    public SqlColumn ColString;
                }
            }
        }

        [Fact]
        public async void Test1()
        {
            var d = sql2dto.dbo.Dummies.As("d");

            var insert1 = sql2dto.SqlBuilder.InsertInto(d)
                .Set(d.ColInt, Sql.Const(1))
                .Set(d.ColString, Sql.Const("abc"));

            var insert2 = sql2dto.SqlBuilder.InsertInto(d)
                .Set(d.ColInt, Sql.Const(2))
                .Set(d.ColString, sql2dto.SqlBuilder.Query().Select(Sql.Const("def")));

            var update1 = sql2dto.SqlBuilder.Update(d)
                .Set(d.ColString, Sql.Const("XXX"));

            var update2 = sql2dto.SqlBuilder.Update(d)
                .Set(d.ColString, Sql.Concat(d.ColInt, Sql.Const("XXX")))
                .Where(d.ColInt == sql2dto.SqlBuilder.Parameter("param_1", 1));

            var delete1 = sql2dto.SqlBuilder.DeleteFrom(d)
                .Where(d.ColInt == Sql.Const(2));

            var delete2 = sql2dto.SqlBuilder.DeleteFrom(d);

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Server=srv-db;Database=sql2dto;User Id=sa;Password=@PentaQuark;"))
            {
                int insertRows1 = await insert1.ExecAsync(conn);
                int insertRows2 = await insert2.ExecAsync(conn);

                int updateRows1 = await update1.ExecAsync(conn);
                int updateRows2 = await update2.ExecAsync(conn);

                int deletedRows1 = await delete1.ExecAsync(conn);
                int deletedRows2 = await delete2.ExecAsync(conn);
            }
        }

        [Fact]
        public async void Test2()
        {
            var subQ = sql2dto.SqlBuilder.Query()
                .Select(Sql.Const(1), "ColInt")
                .Select(Sql.Const("a"), "ColString")
                .As("X");

            var q = sql2dto.SqlBuilder.Query()
                
                // LIKE
                .Select(Sql.Case().When(subQ.GetColumn("ColString").Like("XXX"), then: "YES").Else("NO"))
                .Select(Sql.Case().When(subQ.GetColumn("ColString").NotLike("XXX"), then: "YES").Else("NO"))
                .Select(Sql.Case().When(Sql.Not(subQ.GetColumn("ColString").Like("XXX")), then: "YES").Else("NO"))
                .Select(Sql.Case().When(!subQ.GetColumn("ColString").Like("XXX"), then: "YES").Else("NO"))

                // BETWEEN
                .Select(Sql.Case().When(subQ.GetColumn("ColInt").Between(Sql.Const(1), Sql.Const(2)), then: "YES").Else("NO"))
                .Select(Sql.Case().When(subQ.GetColumn("ColInt").NotBetween(Sql.Const(1), Sql.Const(2)), then: "YES").Else("NO"))
                .Select(Sql.Case().When(Sql.Not(subQ.GetColumn("ColInt").Between(Sql.Const(1), Sql.Const(2))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(!subQ.GetColumn("ColInt").Between(Sql.Const(1), Sql.Const(2)), then: "YES").Else("NO"))

                // IN
                .Select(Sql.Case().When(subQ.GetColumn("ColInt").In(Sql.Const(1), Sql.Const(2)), then: "YES").Else("NO"))
                .Select(Sql.Case().When(subQ.GetColumn("ColInt").NotIn(Sql.Const(1), Sql.Const(2)), then: "YES").Else("NO"))
                .Select(Sql.Case().When(Sql.Not(subQ.GetColumn("ColInt").In(Sql.Const(1), Sql.Const(2))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(!subQ.GetColumn("ColInt").In(Sql.Const(1), Sql.Const(2)), then: "YES").Else("NO"))

                // EXISTS
                .Select(Sql.Case().When(Sql.Exists(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(Sql.NotExists(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(Sql.Not(Sql.Exists(sql2dto.SqlBuilder.Query().Select(Sql.Const(1)))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(!Sql.Exists(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))

                // ANY
                .Select(Sql.Case().When(subQ.GetColumn("ColInt") > Sql.Any(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(Sql.Not(subQ.GetColumn("ColInt") > Sql.Any(sql2dto.SqlBuilder.Query().Select(Sql.Const(1)))), then: "YES").Else("NO"))
                .Select(Sql.Case().When( ! subQ.GetColumn("ColInt") > Sql.Any(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))

                // ALL
                .Select(Sql.Case().When(subQ.GetColumn("ColInt") > Sql.All(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(Sql.Not(subQ.GetColumn("ColInt") > Sql.All(sql2dto.SqlBuilder.Query().Select(Sql.Const(1)))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(!subQ.GetColumn("ColInt") > Sql.All(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))

                .From(subQ);

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Server=srv-db;Database=sql2dto;User Id=sa;Password=@PentaQuark;"))
            using (var cmd = q.BuildDbCommand(conn))
            using (var resultSet = new DataSet())
            using (var adapter = new SqlDataAdapter((SqlCommand)cmd))
            {
                adapter.Fill(resultSet);
                var tbl = resultSet.Tables[0];
            }
        }
    }
}
