﻿using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core.UnitTests.KeyTests.Data
{
    public interface IEmployee
    {
        int Id { get; set; }
        string Name { get; set; }
        double Ratio { get; set; }
    }
}
