using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Attributes
{
    public interface IOnDtoRead
    {
        void OnDtoRead(Dictionary<string, object> rowValues);
    }
}
