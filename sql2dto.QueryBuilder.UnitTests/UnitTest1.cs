using System;
using Xunit;

namespace sql2dto.QueryBuilder.UnitTests
{
    public class UnitTest1
    {
        public class Customer : SqlTable
        {
            public SqlColumn Id { get; private set; }
            public SqlColumn Name { get; private set; }

            public Customer()
                : base("dbo", "CUSTOMER")
            {
                Id = NewColumn(nameof(Id));
                Name = NewColumn(nameof(Name));
            }
        }

        public class Order : SqlTable
        {
            public SqlColumn Id { get; private set; }
            public SqlColumn CustomerId { get; private set; }

            public Order()
                : base("dbo", "ORDER")
            {
                Id = NewColumn(nameof(Id));
                CustomerId = NewColumn(nameof(CustomerId));
            }
        }

        public class OrderItem : SqlTable
        {
            public SqlColumn Id { get; private set; }
            public SqlColumn OrderId { get; private set; }
            public SqlColumn ProductId { get; private set; }
            public SqlColumn Quantity { get; private set; }
            public SqlColumn CurrentPrice { get; private set; }

            public OrderItem()
                : base("dbo", "ORDER_ITEM")
            {
                Id = NewColumn(nameof(Id));
                OrderId = NewColumn(nameof(OrderId));
                ProductId = NewColumn(nameof(ProductId));
                Quantity = NewColumn(nameof(Quantity));
                CurrentPrice = NewColumn(nameof(CurrentPrice));
            }
        }

        public class Product : SqlTable
        {
            public SqlColumn Id { get; private set; }
            public SqlColumn Name { get; private set; }

            public Product()
                : base("dbo", "PRODUCT")
            {
                Id = NewColumn(nameof(Id));
                Name = NewColumn(nameof(Name));
            }
        }

        public static class Tbl
        {
            public static readonly Customer CUSTOMER = new Customer();
            public static readonly Order ORDER = new Order();
            public static readonly OrderItem ORDERITEM = new OrderItem();
            public static readonly Product PRODUCT = new Product();
        }

        [Fact]
        public void Test1()
        {
            var q = Query
                .From(Tbl.ORDER, "o")

                .Join(Tbl.CUSTOMER).On("c", Tbl.CUSTOMER.Id).IsEqualTo("paramName")
                                    .And("c", Tbl.CUSTOMER.Id).IsEqualTo("o", Tbl.ORDER.CustomerId)
                                    .Or(._

                .Join(Tbl.CUSTOMER, "c", Tbl.CUSTOMER.Id == Tbl.ORDER.CustomerId, "o")
                .Join(Tbl.ORDERITEM, "i", Tbl.ORDERITEM.OrderId == Tbl.ORDER.Id, "o")
                .Join(Tbl.PRODUCT, "p", Tbl.PRODUCT.Id == Tbl.ORDERITEM.ProductId, "i")
                    .Select("c", Tbl.CUSTOMER.Id, Tbl.CUSTOMER.Name)
                    .Select("o", Tbl.ORDER.Id)
                    .Select("i", Tbl.ORDERITEM.Id, Tbl.ORDERITEM.Quantity, Tbl.ORDERITEM.CurrentPrice)
                .BuildQuery();
        }
    }
}
