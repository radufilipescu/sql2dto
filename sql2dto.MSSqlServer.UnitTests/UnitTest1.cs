using sql2dto.Attributes;
using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Xunit;

namespace sql2dto.MSSqlServer.UnitTests
{
    public class UnitTest1
    {
        [ColumnsPrefix("c_")]
        [KeyProps(nameof(Id))]
        public class Customer
        {
            public long Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        [ColumnsPrefix("p_")]
        [KeyProps(nameof(Id))]
        public class Product
        {
            public long Id { get; set; }
            public string Name { get; set; }
        }

        [ColumnsPrefix("i_")]
        [KeyProps(nameof(Id))]
        public class OrderItem
        {
            public long Id { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }

            public Order Order { get; set; }
            public Product Product { get; set; }
        }

        [ColumnsPrefix("o_")]
        [KeyProps(nameof(Id))]
        public class Order
        {
            public Order()
            {
                Items = new List<OrderItem>();
            }

            public long Id { get; set; }
            public DateTime CreatedDateTime { get; set; }
            public DateTime? LastUpdatedTime { get; set; }

            public Customer Customer { get; set; }
            public List<OrderItem> Items { get; set; }
        }

        [Fact]
        public async void SELECT_1()
        {
            var query = Query
                .SelectColumns("o", "Id", "CreatedTime", "LastUpdatedTime")
                .AndColumns("i", "Id", "Quantity", "Price")
                .AndColumns("p", "Id", "Name")
                .AndColumns("c", "Id", "FirstName", "LastName")._
                    .From("Orders").As("o")._
                    .Join("OrderItems").As("i").On("o", "Id").IsEqualTo("i", "OrderId")._
                    .Join("Products").As("p").On("p", "Id").IsEqualTo("i", "ProductId")._
                    .Join("Customers").As("c").On("c", "Id").IsEqualTo("o", "CustomerId")._
                    .Where("c", "FirstName").IsEqualTo("p_FirstName").And("c", "LastName").IsEqualTo("p_LastName")._

                .UsingParametersRange(
                    ("p_FirstName", "Radu"),
                    ("p_LastName", "Filipescu")
                );

            using (var conn = new SqlConnection("Server=.;Database=sql2dto;Trusted_Connection=True;"))
            {
                await conn.OpenAsync();
                var h = await conn.ExecQueryReadHelperAsync(query);

                var result = h.Fetch<Order>()
                    .Include<OrderItem>((o, i) => { o.Items.Add(i); i.Order = o; },
                        (fetchOp) => fetchOp.Include<Product>((i, p) => i.Product = p))
                    .Include<Customer>((o, c) => o.Customer = c)
                .All();
            }
        }


        [Fact]
        public void SELECT_2()
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
                    );


            var sqlText = query.ToString();
        }
    }
}
