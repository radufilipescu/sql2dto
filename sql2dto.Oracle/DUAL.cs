using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Oracle
{
    public class DUAL : SqlTable
    {
        public DUAL() 
            : base("", "DUAL", "_D_U_A_L_")
        {
        }
    }
}
