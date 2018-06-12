using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public interface IGetInnerExpressionsList
    {
        List<SqlExpression> GetInnerExpressionsList();
    }
}
