using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public interface IReadHelperSettings
    {
        BooleanTranslator BooleanTranslator { get; }
    }
}
