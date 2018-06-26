using sql2dto.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Oracle
{
    public class PLSqlReadHelperSettings : IReadHelperSettings
    {
        public PLSqlReadHelperSettings()
        {
            BooleanTranslator = new PLSqlBooleanTranslator();
        }

        private BooleanTranslator _booleanTranslator;
        public virtual BooleanTranslator BooleanTranslator
        {
            get => _booleanTranslator;
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("BooleanTranslator cannot be set to null");
                }

                _booleanTranslator = value;
            }
        }
    }
}
