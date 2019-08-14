using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public interface IIncludeOperation<TDto>
        where TDto : new()
    {
        // Once the "Covariant Return Type" is implemented in C#, then we can change the return types
        // from all include methods in this interface to return IIncludeOperation<TDto> instead
        // https://github.com/dotnet/csharplang/issues/49

        #region No Keys
        FetchOperation<TDto> Include<TChildDto>(Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new();

        FetchOperation<TDto> Include<TChildDto>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new();

        FetchOperation<TDto> Include<TChildDto>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new();

        FetchOperation<TDto> Include<TChildDto>(string columnsPrefix, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new();
        #endregion

        #region 1 Key
        FetchOperation<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>;

        FetchOperation<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>;

        FetchOperation<TDto> Include<TChildDto, TKey>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>;

        FetchOperation<TDto> Include<TChildDto, TKey>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>;

        FetchOperation<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, string keyPropName, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>;

        FetchOperation<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>;

        FetchOperation<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, Func<ReadHelper, TKey> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>;

        FetchOperation<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, TKey> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>;
        #endregion

        #region 2 Keys
        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>;
        #endregion

        #region 3 Keys
        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>;
        #endregion

        #region 4 Keys
        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>;
        #endregion

        #region 5 Keys
        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>;
        #endregion

        #region 6 Keys
        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> map, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string keyPropName6, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string keyPropName6, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>;

        FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>;
        #endregion
    }
}
