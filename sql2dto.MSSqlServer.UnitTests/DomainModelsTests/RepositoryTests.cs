using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;
using System.Text;
using Xunit;

namespace sql2dto.MSSqlServer.UnitTests.DomainModelsTests
{
    public class RepositoryTests
    {
        #region DOMAIN MODELS
        public class Person
        {
            protected Person()
            {
                _phoneNumbers = new List<string>();
                _addresses = new List<Address>();
            }

            public Person(string name)
                : this()
            {
                FirstName = name;
            }

            public int Id { get; protected set; }

            public virtual string FirstName 
            { 
                get => _firstName; 
                set
                {
                    if (String.IsNullOrWhiteSpace(value) || value.Length < 3)
                    {
                        throw new InvalidOperationException("First name too small");
                    }

                    _firstName = value;
                }
            }
            protected string _firstName;

            public virtual string LastName
            {
                get => _lastName;
                set
                {
                    if (String.IsNullOrWhiteSpace(value) || value.Length < 3)
                    {
                        throw new InvalidOperationException("Last name too small");
                    }

                    _lastName = value;
                }
            }
            protected string _lastName;

            public IReadOnlyList<string> PhoneNumbers => _phoneNumbers;
            protected List<string> _phoneNumbers;

            public IReadOnlyList<Address> Addresses => _addresses;
            protected List<Address> _addresses;

            public virtual void AddPhoneNumber(string phoneNumber)
            {
                if (_phoneNumbers.Contains(phoneNumber))
                {
                    throw new ArgumentException("Phone number already assigned", nameof(phoneNumber));
                }

                _phoneNumbers.Add(phoneNumber);
            }

            public virtual void AddAddress(Address address)
            {
                if (_addresses.Count > 3)
                {
                    throw new ArgumentOutOfRangeException(nameof(address), "Too many addreses");
                }

                _addresses.Add(address);
            }
        }

        public class Address
        {
            public int Id { get; protected set; }

            public virtual int PersonId { get; protected set; }

            public virtual string Street { get => _street; set => _street = value; }
            protected string _street;

            public virtual int Number { get => _number; set => _number = value; }
            protected int _number;
        }
        #endregion

        public class PhoneNumber
        {
            public int PersonId { get; set; }
            public string Value { get; set; }
        }

        #region PERSISTENCE MODELS
        public class PersonPM : Person
        {
            private SqlBuilder _sqlBuilder;
            private HashSet<string> _changedColumnNames = new HashSet<string>();
            private SqlUpdate _updateStatement;
            private List<ISqlStatement> _statements;

            public PersonPM(SqlBuilder sqlBuilder, List<ISqlStatement> statements)
                : base()
            {
                _sqlBuilder = sqlBuilder;
                _statements = statements;
            }

            public override string FirstName
            {
                get => base.FirstName;
                set
                {
                    base.FirstName = value;
                    if (_changedColumnNames.Add(nameof(sql2dto.dbo.Persons.FirstName)))
                    {
                        var p = sql2dto.dbo.Persons.As("p");
                        if (_updateStatement == null)
                        {
                            _updateStatement = _sqlBuilder.Update(p).Where(p.Id == Sql.Const(this.Id));
                            _statements.Add(_updateStatement);
                        }
                        _updateStatement.Set(p.FirstName, Sql.Const(value));
                    }
                }
            }

            public override string LastName
            {
                get => base.LastName;
                set
                {
                    base.LastName = value;
                    if (_changedColumnNames.Add(nameof(sql2dto.dbo.Persons.LastName)))
                    {
                        var p = sql2dto.dbo.Persons.As("p");
                        if (_updateStatement == null)
                        {
                            _updateStatement = _sqlBuilder.Update(p).Where(p.Id == Sql.Const(this.Id));
                            _statements.Add(_updateStatement);
                        }
                        _updateStatement.Set(p.LastName, Sql.Const(value));
                    }
                }
            }

            public static Action<PersonPM, PhoneNumber> AttachPhoneNumber = (person, phoneNumber) =>
            {
                person._phoneNumbers.Add(phoneNumber.Value);
            };

            public static Action<PersonPM, AddressPM> AttachAddress = (person, address) =>
            {
                person._addresses.Add(address);
            };

            public void Attach(AddressPM address)
            {
                this._addresses.Add(address);
            }

            public override void AddPhoneNumber(string phoneNumber)
            {
                base.AddPhoneNumber(phoneNumber);

                var pr = sql2dto.dbo.PersonPhoneNumbers.As("pr");
                var insert = _sqlBuilder.InsertInto(pr)
                    .Set(pr.PersonId, Sql.Const(this.Id))
                    .Set(pr.Value, Sql.Const(phoneNumber));

                _statements.Add(insert);
            }

            public override void AddAddress(Address address)
            {
                base.AddAddress(address);

                var a = sql2dto.dbo.PersonAddresses.As("a");
                var insert = _sqlBuilder.InsertInto(a)
                    .Set(a.PersonId, Sql.Const(this.Id))
                    .Set(a.Street, Sql.Const(address.Street))
                    .Set(a.Number, Sql.Const(address.Number));

                _statements.Add(insert);
            }
        }

        public class AddressPM : Address
        {
            private SqlBuilder _sqlBuilder;
            private HashSet<string> _changedColumnNames = new HashSet<string>();
            private SqlUpdate _updateStatement;
            private List<ISqlStatement> _statements;

            public AddressPM(SqlBuilder sqlBuilder, List<ISqlStatement> statements)
                : base()
            {
                _sqlBuilder = sqlBuilder;
                _statements = statements;
            }

            public override string Street
            {
                get => base.Street;
                set
                {
                    base.Street = value;
                    if (_changedColumnNames.Add(nameof(sql2dto.dbo.PersonAddresses.Street)))
                    {
                        var a = sql2dto.dbo.PersonAddresses.As("a");
                        if (_updateStatement == null)
                        {
                            _updateStatement = _sqlBuilder.Update(a).Where(a.Id == Sql.Const(this.Id));
                            _statements.Add(_updateStatement);
                        }
                        _updateStatement.Set(a.Street, Sql.Const(value));
                    }
                }
            }

            public override int Number
            {
                get => base.Number;
                set
                {
                    base.Number = value;
                    if (_changedColumnNames.Add(nameof(sql2dto.dbo.PersonAddresses.Number)))
                    {
                        var a = sql2dto.dbo.PersonAddresses.As("a");
                        if (_updateStatement == null)
                        {
                            _updateStatement = _sqlBuilder.Update(a).Where(a.Id == Sql.Const(this.Id));
                            _statements.Add(_updateStatement);
                        }
                        _updateStatement.Set(a.Number, Sql.Const(value));
                    }
                }
            }
        }
        #endregion

        [Fact]
        public async void Simple_inheritence_test()
        {
            DtoMapper<PersonPM>.Default
                .SetColumnsPrefix("Person_")
                .BackingField(_ => _.FirstName, "_firstName")
                .BackingField(_ => _.LastName, "_lastName")
                .SetKeyProps(_ => _.Id);

            DtoMapper<AddressPM>.Default
                .SetColumnsPrefix("Address_")
                .BackingField(_ => _.Number, "_number")
                .BackingField(_ => _.Street, "_street")
                .SetKeyProps(_ => _.Id);

            DtoMapper<PhoneNumber>.Default
                .SetColumnsPrefix("PhoneNumber_")
                .SetKeyProps(_ => _.PersonId, _ => _.Value);

            var p = sql2dto.dbo.Persons.As("p");
            var pp = sql2dto.dbo.PersonPhoneNumbers.As("pp");
            var a = sql2dto.dbo.PersonAddresses.As("a");

            using (var conn = await sql2dto.SqlBuilder.ConnectAsync("Server=srv-db;Database=sql2dto;User Id=sa;Password=@PentaQuark;"))
            {
                var query =  sql2dto.SqlBuilder
                    .FetchQuery<PersonPM>(p)
                    .Include<PhoneNumber>(pp, PersonPM.AttachPhoneNumber)
                    .Include<AddressPM>(a, PersonPM.AttachAddress)

                    .From(p)
                    .LeftJoin(pp, on: p.Id == pp.PersonId)
                    .LeftJoin(a, on: p.Id == a.PersonId);

                var str = query.BuildQueryString();

                var statements = new List<ISqlStatement>();

                var list = (IReadOnlyList<Person>) await query
                        .ExecAsync(conn, new Dictionary<string, object>
                        {
                            { "sqlBuilder", sql2dto.SqlBuilder },
                            { "statements", statements }
                        });

                list[0].LastName += "_1";
                list[0].Addresses[0].Street += "_1";
                list[0].Addresses[1].Street += "_1";

                list[1].LastName += "_1";
                list[1].LastName += "_1";
                list[1].LastName += "_1";
                list[1].LastName += "_1";
                list[1].LastName += "_1";
                list[1].LastName += "_1";
                list[1].LastName += "_1";
                list[1].AddPhoneNumber("xxx");

                var dbCmds = new List<string>();
                foreach (dynamic s in statements)
                {
                    var cmd = sql2dto.SqlBuilder.BuildDbCommand(s);
                    dbCmds.Add(cmd.CommandText);
                }

                var finalCmd = String.Join("\n\n", dbCmds);
            }
        }

        public class sql2dto
        {
            public static readonly SqlBuilder SqlBuilder = new TSqlBuilder();

            public class dbo
            {
                public class Persons : SqlTable
                {
                    public static Persons As(string alias)
                    {
                        return new Persons(alias);
                    }

                    private Persons(string alias)
                        : base(nameof(dbo), nameof(Persons), alias)
                    {
                        Id = DefineColumn(nameof(Id));
                        FirstName = DefineColumn(nameof(FirstName));
                        LastName = DefineColumn(nameof(LastName));
                    }

                    public SqlColumn Id;
                    public SqlColumn FirstName;
                    public SqlColumn LastName;
                }

                public class PersonPhoneNumbers : SqlTable
                {
                    public static PersonPhoneNumbers As(string alias)
                    {
                        return new PersonPhoneNumbers(alias);
                    }

                    private PersonPhoneNumbers(string alias)
                        : base(nameof(dbo), nameof(PersonPhoneNumbers), alias)
                    {
                        PersonId = DefineColumn(nameof(PersonId));
                        Value = DefineColumn(nameof(Value));
                    }

                    public SqlColumn PersonId;
                    public SqlColumn Value;
                }

                public class PersonAddresses : SqlTable
                {
                    public static PersonAddresses As(string alias)
                    {
                        return new PersonAddresses(alias);
                    }

                    private PersonAddresses(string alias)
                        : base(nameof(dbo), nameof(PersonAddresses), alias)
                    {
                        Id = DefineColumn(nameof(Id));
                        PersonId = DefineColumn(nameof(PersonId));
                        Street = DefineColumn(nameof(Street));
                        Number = DefineColumn(nameof(Number));
                    }

                    public SqlColumn Id;
                    public SqlColumn PersonId;
                    public SqlColumn Street;
                    public SqlColumn Number;
                }
            }
        }
    }
}
