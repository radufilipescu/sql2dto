using sql2dto.Attributes;
using sql2dto.Core.UnitTests.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace sql2dto.Core.UnitTests.FetchOpTests
{
    public class DeepHierarchyTests
    {
        [KeyProps(nameof(Id))]
        public abstract class IdentifiableById
        {
            public int Id { get; set; }
        }

        [KeyProps(nameof(Name))]
        public abstract class IdentifiableByName
        {
            public string Name { get; set; }
        }

        public class Person : IdentifiableById
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        [ColumnsPrefix("exec")]
        public class Executive : Person
        {
            public string Title { get; set; }
        }

        [ColumnsPrefix("emp")]
        public class Employee : Person
        {
            public Employee()
            {
                HolidayRequests = new List<HolidayRequest>();
            }

            public Executive Direct { get; set; }
            public Department Department { get; set; }
            public List<HolidayRequest> HolidayRequests { get; set; }
        }

        [ColumnsPrefix("dep")]
        public class Department : IdentifiableByName
        {
            public Department()
            {
                Locations = new List<Location>();
            }

            public Employee Head { get; set; }
            public List<Location> Locations { get; set; }
        }

        [ColumnsPrefix("hreq")]
        [KeyProps(nameof(EmployeeId), nameof(Start), nameof(End))]
        public class HolidayRequest
        {
            public int EmployeeId { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        [ColumnsPrefix("loc")]
        [KeyProps(nameof(Country), nameof(City))]
        public class Location
        {
            public string Country { get; set; }
            public string City { get; set; }
        }

        [Fact]
        public void Test1()
        {
            var fakeReader = new FakeDataReader(
                "empId", "empFirstName", "empLastName", 
                "execId", "execFirstName", "execLastName", "execTitle", 
                "depName", 
                "headId", "headFirstName", "headLastName", 
                "locCountry", "locCity",
                "hreqEmployeeId", "hreqStart", "hreqEnd");

            fakeReader.AddRow(1, "Radu", "Filipescu", 1, "John", "Plows", "CEO", "R&D", 1, "Radu", "Filipescu", "RO", "Bucharest", 1, DateTime.Today.AddDays(-7), DateTime.Today.AddDays(-3));
            fakeReader.AddRow(2, "Mihaita", "Iacob", 1, "John", "Plows", "CEO", "R&D", 1, "Radu", "Filipescu", "RO", "Bucharest", 2, DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-3));
            fakeReader.AddRow(3, "Florin", "Dumitriu", 1, "John", "Plows", "CEO", "R&D", 1, "Radu", "Filipescu", "RO", "Bucharest", 3, DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-3));

            var h = new ReadHelper(fakeReader);

            var fetch = 
                h.Fetch<Employee>()
                    .Include<HolidayRequest>((emp, hreq) => { emp.HolidayRequests.Add(hreq); })
                    .Include<Department>((emp, dep) => { emp.Department = dep; }, (depFetchOp) => { depFetchOp
                        .Include<Location>((dep, loc) => { dep.Locations.Add(loc); })
                        .Include<Employee>("head", (dep, head) => { dep.Head = head; }, (empFetchOp) => { empFetchOp
                            .Include<Executive>((emp, ex) => { emp.Direct = ex; });
                        });
                    });

            var result = fetch.All();
        }
    }
}
