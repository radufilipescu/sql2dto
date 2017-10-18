using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Xunit;

namespace sql2dto.MSSqlServer.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void SELECT_1()
        {
            var query = Query

                .SelectAll()
                .AndColumns("o", "*")
                .AndColumns("i", "DATE", "QUANTITY", "PRICE")._
                    .From("ORDERS").As("o")._
                    .Join("ORDER_ITEMS").As("i").On("i", "ORDER_ID").IsEqualTo("o", "ID").AndSub("i", "ORDER_ID").IsEqualTo("o", "ID").EndSub()._
                    .Join("CUSTOMER").As("c").On("c", "ORDER_ID").IsEqualTo("o", "ID")._
                    .WhereSub()
                        .Sub("c", "FIRSTNAME").IsEqualTo("p_FirstName").EndSub()
                        .AndSub("c", "LASTNAME").IsEqualTo("p_LastName").EndSub()
                    .EndSub()._

                .UsingParametersRange(
                    ("p_FirstName", "Radu"), 
                    ("p_LastName", "Filipescu")
                )

                .ToString();
        }
    }
}
