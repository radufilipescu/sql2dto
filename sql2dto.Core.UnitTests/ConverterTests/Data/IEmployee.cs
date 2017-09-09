using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core.UnitTests.ConverterTests.Data
{
    public interface IEmployee
    {
        int Id { get; set; }
        string Name { get; set; }
        bool IsActive { get; set; }
    }
}
