using Oracle.ManagedDataAccess.Client;
using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Xunit;

namespace sql2dto.Oracle.UnitTests
{
    public class CRUDTests
    {
        public class sql2dto
        {
            public static readonly SqlBuilder SqlBuilder = new PLSqlBuilder();

            public class ADMINEMMETT
            {
                public class ZZZ_SQL2DTO_DUMMIES : SqlTable
                {
                    public static ZZZ_SQL2DTO_DUMMIES As(string alias)
                    {
                        return new ZZZ_SQL2DTO_DUMMIES(alias);
                    }

                    private ZZZ_SQL2DTO_DUMMIES(string alias)
                        : base(nameof(ADMINEMMETT), nameof(ZZZ_SQL2DTO_DUMMIES), alias)
                    {
                        COLINT = DefineColumn(nameof(COLINT));
                        COLSTRING = DefineColumn(nameof(COLSTRING));
                    }

                    public SqlColumn COLINT;
                    public SqlColumn COLSTRING;
                }
            }
        }

        [Fact]
        public async void Test1()
        {
            var d = sql2dto.ADMINEMMETT.ZZZ_SQL2DTO_DUMMIES.As("d");

            var insert1 = sql2dto.SqlBuilder.InsertInto(d)
                .Set(d.COLINT, Sql.Const(1))
                .Set(d.COLSTRING, Sql.Const("abc"));

            var insert2 = sql2dto.SqlBuilder.InsertInto(d)
                .Set(d.COLINT, Sql.Const(2))
                .Set(d.COLSTRING, sql2dto.SqlBuilder.Query().Select(Sql.Const("def")));

            var update1 = sql2dto.SqlBuilder.Update(d)
                .Set(d.COLSTRING, Sql.Const("XXX"));

            var update2 = sql2dto.SqlBuilder.Update(d)
                .Set(d.COLSTRING, Sql.Concat(d.COLINT, Sql.Const("XXX")))
                .Where(d.COLINT == sql2dto.SqlBuilder.Parameter("param_1", 1));

            var delete1 = sql2dto.SqlBuilder.DeleteFrom(d)
                .Where(d.COLINT == Sql.Const(2));

            var delete2 = sql2dto.SqlBuilder.DeleteFrom(d);

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=srv-db)(PORT=1521))(CONNECT_DATA=(SID=orcl)));User Id=ADMINEMMETT; Password=adminemmett;"))
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
                .Select(Sql.Case().When(!subQ.GetColumn("ColInt") > Sql.Any(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))

                // ALL
                .Select(Sql.Case().When(subQ.GetColumn("ColInt") > Sql.All(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(Sql.Not(subQ.GetColumn("ColInt") > Sql.All(sql2dto.SqlBuilder.Query().Select(Sql.Const(1)))), then: "YES").Else("NO"))
                .Select(Sql.Case().When(!subQ.GetColumn("ColInt") > Sql.All(sql2dto.SqlBuilder.Query().Select(Sql.Const(1))), then: "YES").Else("NO"))

                .From(subQ);

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=srv-db)(PORT=1521))(CONNECT_DATA=(SID=orcl)));User Id=ADMINEMMETT; Password=adminemmett;"))
            using (var cmd = q.BuildDbCommand(conn))
            using (var resultSet = new DataSet())
            using (var adapter = new OracleDataAdapter((OracleCommand)cmd))
            {
                adapter.Fill(resultSet);
                var tbl = resultSet.Tables[0];
            }
        }

        [Fact]
        public async void Test3()
        {
            var q = sql2dto.SqlBuilder.Query()
                .Distinct()
                .Select(Sql.FuncCall("SUM", Sql.Const(1) + Sql.Const(10) - Sql.Const(6) * Sql.Const(2)));

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=srv-db)(PORT=1521))(CONNECT_DATA=(SID=orcl)));User Id=ADMINEMMETT; Password=adminemmett;"))
            using (var cmd = q.BuildDbCommand(conn))
            using (var resultSet = new DataSet())
            using (var adapter = new OracleDataAdapter((OracleCommand)cmd))
            {
                adapter.Fill(resultSet);
                DataTable tbl = resultSet.Tables[0];
            }
        }
    }
}
