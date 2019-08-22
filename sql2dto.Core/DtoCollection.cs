using sql2dto.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

namespace sql2dto.Core
{
    public class DtoCollection<TDto> where TDto : new()
    {
        public List<TDto> InnerList { get; private set; }
        public int LastFetchedIndex { get; protected set; }

        protected ReadHelper _helper;
        protected DtoMapper<TDto> _mapper;
        protected Func<TDto> _mapFunc;
        protected string _columnsPrefix;

        private void SetupMapFunc()
        {
            _mapFunc = _mapper?.CreateMapFunc(_helper, _columnsPrefix) 
                ?? DtoMapper<TDto>.CreateDefaultMapFunc(_helper, _columnsPrefix);
        }

        protected void SetupColumnNamesToOrdinalsChangedHandler()
        {
            _helper.ColumnMappingsChanged += _helper_ColumnMappingsChanged;
        }

        protected virtual void _helper_ColumnMappingsChanged(object sender, EventArgs e)
        {
            SetupMapFunc();
        }

        public DtoCollection(ReadHelper helper) 
        {
            InnerList = new List<TDto>();
            _helper = helper;
            _mapper = null;
            _columnsPrefix = null;
            SetupMapFunc();
            SetupColumnNamesToOrdinalsChangedHandler();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper)
        {
            InnerList = new List<TDto>();
            _helper = helper;
            _mapper = null;
            _columnsPrefix = columnsPrefix;
            SetupMapFunc();
            SetupColumnNamesToOrdinalsChangedHandler();
        }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper)
        {
            InnerList = new List<TDto>();
            _helper = helper;
            _mapper = mapper;
            _columnsPrefix = null;
            SetupMapFunc();
            SetupColumnNamesToOrdinalsChangedHandler();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper, DtoMapper<TDto> mapper)
        {
            InnerList = new List<TDto>();
            _helper = helper;
            _mapper = mapper;
            _columnsPrefix = columnsPrefix;
            SetupMapFunc();
            SetupColumnNamesToOrdinalsChangedHandler();
        }

        public virtual TDto Fetch()
        {
            var dto = _mapFunc();

            if (DtoMapper<TDto>.ImplementsIOnDtoRead)
            {
                var currentData = _helper.FetchCurrentRowDataAsDictionary();
                ((IOnDtoRead)dto).OnDtoRead(currentData);
            }

            InnerList.Add(dto);
            LastFetchedIndex = InnerList.Count - 1;
            return dto;
        }

        protected string GetConfiguredColumnsPrefix()
        {
            return _columnsPrefix ?? _mapper?.ColumnsPrefix ?? DtoMapper<TDto>.DefaultColumnsPrefix;
        }

        protected string GetConfiguredKeyPropNameByIndex(int keyItemsCount, int index)
        {
            if (_mapper == null && DtoMapper<TDto>.DefaultOrderedKeyPropNames != null)
            {
                if (DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length == keyItemsCount)
                {
                    return DtoMapper<TDto>.DefaultOrderedKeyPropNames[index];
                }

                throw new InvalidOperationException($"This collection's key has {keyItemsCount} items while the map config has {DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length}");
            }
            else if (_mapper != null && _mapper.OrderedKeyPropNames != null)
            {
                if (_mapper.OrderedKeyPropNames.Length == keyItemsCount)
                {
                    return _mapper.OrderedKeyPropNames[index];
                }

                throw new InvalidOperationException($"This collection's key has {keyItemsCount} items while the map config has {_mapper.OrderedKeyPropNames.Length}");
            }

            throw new InvalidOperationException("No key properties configuration was made for current map");
        }

        protected bool IsDBNullKeyPart(string keyPartPropName, string columnPrefix = null)
        {
            try
            {
                if (_mapper == null)
                {
                    return _helper.IsDBNull<TDto>(keyPartPropName, columnPrefix ?? GetConfiguredColumnsPrefix());
                }
                else
                {
                    return _helper.IsDBNull(_mapper, keyPartPropName, columnPrefix ?? GetConfiguredColumnsPrefix());
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not check if DBNull for key property {keyPartPropName}. See inner exception for details.", ex);
            }
        }

        protected bool IsDBNullAllowedKeyPart(string keyPartPropName)
        {
            try
            {
                if (_mapper == null)
                {
                    if (DtoMapper<TDto>.DefaultPropMapConfigs.TryGetValue(keyPartPropName, out PropMapConfig conf))
                    {
                        return conf.IsNullableKey;
                    }

                    return false;
                }
                else
                {
                    if (_mapper.PropMapConfigs.TryGetValue(keyPartPropName, out PropMapConfig conf))
                    {
                        return conf.IsNullableKey;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Could not check if key property {keyPartPropName} is allowed to be DBNull. See inner exception for details.", ex);
            }
        }

        protected TKeyPart FetchKeyPart<TKeyPart>(string keyPartPropName, string columnPrefix = null)
        {
            try
            {
                if (_mapper == null)
                {
                    return (TKeyPart)_helper.GetNullableValue<TDto>(keyPartPropName, columnPrefix ?? GetConfiguredColumnsPrefix());
                }
                else
                {
                    return (TKeyPart)_helper.GetNullableValue(_mapper, keyPartPropName, columnPrefix ?? GetConfiguredColumnsPrefix());
                }
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Could not retrieve value for key property {keyPartPropName}. See inner exception for details.", ex);
            }
        }
    }

    public class DtoCollection<TDto, TKey> : DtoCollection<TDto> where TDto : new()
        where TKey : IComparable, IConvertible, IEquatable<TKey>
    {
        private const int KeyItemsCount = 1;
        public Dictionary<TKey, int> KeyesToIndexes { get; private set; }

        public DtoCollection(ReadHelper helper)
            : base(helper)
        {
            KeyesToIndexes = new Dictionary<TKey, int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper)
            : base(columnsPrefix, helper)
        {
            KeyesToIndexes = new Dictionary<TKey, int>();
        }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<TKey, int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper, DtoMapper<TDto> mapper)
            : base(columnsPrefix, helper, mapper)
        {
            KeyesToIndexes = new Dictionary<TKey, int>();
        }

        public TDto FetchByKeyValue(TKey key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
                LastFetchedIndex = index;
            }
            else
            {
                dto = base.Fetch(); // here LastFetchedIndex is actualised as InnerList.Count - 1
                KeyesToIndexes.Add(key, LastFetchedIndex);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName, string columnPrefix = null)
        {
            if (!IsDBNullAllowedKeyPart(keyPropName) && IsDBNullKeyPart(keyPropName, columnPrefix))
            {
                return default(TDto);
            }
            TKey key = FetchKeyPart<TKey>(keyPropName, columnPrefix);
            return FetchByKeyValue(key);
        }

        public override TDto Fetch()
        {
            return FetchByKeyProps(
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0),
                GetConfiguredColumnsPrefix()
            );
        }
    }

    public class DtoCollection<TDto, TKey1, TKey2> : DtoCollection<TDto> where TDto : new()
        where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
        where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
    {
        private const int KeyItemsCount = 2;
        public Dictionary<(TKey1, TKey2), int> KeyesToIndexes { get; private set; }

        public DtoCollection(ReadHelper helper)
            : base(helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper)
            : base(columnsPrefix, helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2), int>();
        }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper, DtoMapper<TDto> mapper)
            : base(columnsPrefix, helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2), int>();
        }

        public TDto FetchByKeyValue((TKey1, TKey2) key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
                LastFetchedIndex = index;
            }
            else
            {
                dto = base.Fetch(); // here LastFetchedIndex is actualised as InnerList.Count - 1
                KeyesToIndexes.Add(key, LastFetchedIndex);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName1, string keyPropName2, string columnsPrefix = null)
        {
            if ((!IsDBNullAllowedKeyPart(keyPropName1) && IsDBNullKeyPart(keyPropName1, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName2) && IsDBNullKeyPart(keyPropName2, columnsPrefix)))
            {
                return default(TDto);
            }
            TKey1 key1 = FetchKeyPart<TKey1>(keyPropName1, columnsPrefix);
            TKey2 key2 = FetchKeyPart<TKey2>(keyPropName2, columnsPrefix);
            return FetchByKeyValue((key1, key2));
        }

        public override TDto Fetch()
        {
            return FetchByKeyProps(
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 1),
                GetConfiguredColumnsPrefix()
            );
        }
    }

    public class DtoCollection<TDto, TKey1, TKey2, TKey3> : DtoCollection<TDto> where TDto : new()
        where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
        where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
    {
        private const int KeyItemsCount = 3;
        public Dictionary<(TKey1, TKey2, TKey3), int> KeyesToIndexes { get; private set; }

        public DtoCollection(ReadHelper helper)
            : base(helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper)
            : base(columnsPrefix, helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3), int>();
        }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper, DtoMapper<TDto> mapper)
            : base(columnsPrefix, helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3), int>();
        }

        public TDto FetchByKeyValue((TKey1, TKey2, TKey3) key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
                LastFetchedIndex = index;
            }
            else
            {
                dto = base.Fetch(); // here LastFetchedIndex is actualised as InnerList.Count - 1
                KeyesToIndexes.Add(key, LastFetchedIndex);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName1, string keyPropName2, string keyPropName3, string columnsPrefix = null)
        {
            if ((!IsDBNullAllowedKeyPart(keyPropName1) && IsDBNullKeyPart(keyPropName1, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName2) && IsDBNullKeyPart(keyPropName2, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName3) && IsDBNullKeyPart(keyPropName3, columnsPrefix)))
            {
                return default(TDto);
            }
            TKey1 key1 = FetchKeyPart<TKey1>(keyPropName1, columnsPrefix);
            TKey2 key2 = FetchKeyPart<TKey2>(keyPropName2, columnsPrefix);
            TKey3 key3 = FetchKeyPart<TKey3>(keyPropName3, columnsPrefix);
            return FetchByKeyValue((key1, key2, key3));
        }

        public override TDto Fetch()
        {
            return FetchByKeyProps(
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 1),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 2),
                GetConfiguredColumnsPrefix()
            );
        }
    }

    public class DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4> : DtoCollection<TDto> where TDto : new()
        where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
        where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
    {
        private const int KeyItemsCount = 4;
        public Dictionary<(TKey1, TKey2, TKey3, TKey4), int> KeyesToIndexes { get; private set; }

        public DtoCollection(ReadHelper helper)
            : base(helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper)
            : base(columnsPrefix, helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4), int>();
        }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper, DtoMapper<TDto> mapper)
            : base(columnsPrefix, helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4), int>();
        }

        public TDto FetchByKeyValue((TKey1, TKey2, TKey3, TKey4) key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
                LastFetchedIndex = index;
            }
            else
            {
                dto = base.Fetch(); // here LastFetchedIndex is actualised as InnerList.Count - 1
                KeyesToIndexes.Add(key, LastFetchedIndex);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string columnsPrefix = null)
        {
            if ((!IsDBNullAllowedKeyPart(keyPropName1) && IsDBNullKeyPart(keyPropName1, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName2) && IsDBNullKeyPart(keyPropName2, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName3) && IsDBNullKeyPart(keyPropName3, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName4) && IsDBNullKeyPart(keyPropName4, columnsPrefix)))
            {
                return default(TDto);
            }
            TKey1 key1 = FetchKeyPart<TKey1>(keyPropName1, columnsPrefix);
            TKey2 key2 = FetchKeyPart<TKey2>(keyPropName2, columnsPrefix);
            TKey3 key3 = FetchKeyPart<TKey3>(keyPropName3, columnsPrefix);
            TKey4 key4 = FetchKeyPart<TKey4>(keyPropName4, columnsPrefix);
            return FetchByKeyValue((key1, key2, key3, key4));
        }

        public override TDto Fetch()
        {
            return FetchByKeyProps(
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 1),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 2),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 3),
                GetConfiguredColumnsPrefix()
            );
        }
    }

    public class DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5> : DtoCollection<TDto> where TDto : new()
        where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
        where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
    {
        private const int KeyItemsCount = 5;
        public Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5), int> KeyesToIndexes { get; private set; }

        public DtoCollection(ReadHelper helper)
            : base(helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper)
            : base(columnsPrefix, helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5), int>();
        }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper, DtoMapper<TDto> mapper)
            : base(columnsPrefix, helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5), int>();
        }

        public TDto FetchByKeyValue((TKey1, TKey2, TKey3, TKey4, TKey5) key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
                LastFetchedIndex = index;
            }
            else
            {
                dto = base.Fetch(); // here LastFetchedIndex is actualised as InnerList.Count - 1
                KeyesToIndexes.Add(key, LastFetchedIndex);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string columnsPrefix = null)
        {
            if ((!IsDBNullAllowedKeyPart(keyPropName1) && IsDBNullKeyPart(keyPropName1, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName2) && IsDBNullKeyPart(keyPropName2, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName3) && IsDBNullKeyPart(keyPropName3, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName4) && IsDBNullKeyPart(keyPropName4, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName5) && IsDBNullKeyPart(keyPropName5, columnsPrefix)))
            {
                return default(TDto);
            }
            TKey1 key1 = FetchKeyPart<TKey1>(keyPropName1, columnsPrefix);
            TKey2 key2 = FetchKeyPart<TKey2>(keyPropName2, columnsPrefix);
            TKey3 key3 = FetchKeyPart<TKey3>(keyPropName3, columnsPrefix);
            TKey4 key4 = FetchKeyPart<TKey4>(keyPropName4, columnsPrefix);
            TKey5 key5 = FetchKeyPart<TKey5>(keyPropName5, columnsPrefix);
            return FetchByKeyValue((key1, key2, key3, key4, key5));
        }

        public override TDto Fetch()
        {
            return FetchByKeyProps(
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 1),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 2),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 3),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 4),
                GetConfiguredColumnsPrefix()
            );
        }
    }

    public class DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> : DtoCollection<TDto> where TDto : new()
        where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
        where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
    {
        private const int KeyItemsCount = 6;
        public Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6), int> KeyesToIndexes { get; private set; }

        public DtoCollection(ReadHelper helper)
            : base(helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper)
            : base(columnsPrefix, helper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6), int>();
        }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6), int>();
        }

        public DtoCollection(string columnsPrefix, ReadHelper helper, DtoMapper<TDto> mapper)
            : base(columnsPrefix, helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4, TKey5, TKey6), int>();
        }

        public TDto FetchByKeyValue((TKey1, TKey2, TKey3, TKey4, TKey5, TKey6) key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
                LastFetchedIndex = index;
            }
            else
            {
                dto = base.Fetch(); // here LastFetchedIndex is actualised as InnerList.Count - 1
                KeyesToIndexes.Add(key, LastFetchedIndex);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string keyPropName6, string columnsPrefix = null)
        {
            if ((!IsDBNullAllowedKeyPart(keyPropName1) && IsDBNullKeyPart(keyPropName1, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName2) && IsDBNullKeyPart(keyPropName2, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName3) && IsDBNullKeyPart(keyPropName3, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName4) && IsDBNullKeyPart(keyPropName4, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName5) && IsDBNullKeyPart(keyPropName5, columnsPrefix))
                || (!IsDBNullAllowedKeyPart(keyPropName6) && IsDBNullKeyPart(keyPropName6, columnsPrefix)))
            {
                return default(TDto);
            }
            TKey1 key1 = FetchKeyPart<TKey1>(keyPropName1, columnsPrefix);
            TKey2 key2 = FetchKeyPart<TKey2>(keyPropName2, columnsPrefix);
            TKey3 key3 = FetchKeyPart<TKey3>(keyPropName3, columnsPrefix);
            TKey4 key4 = FetchKeyPart<TKey4>(keyPropName4, columnsPrefix);
            TKey5 key5 = FetchKeyPart<TKey5>(keyPropName5, columnsPrefix);
            TKey6 key6 = FetchKeyPart<TKey6>(keyPropName6, columnsPrefix);
            return FetchByKeyValue((key1, key2, key3, key4, key5, key6));
        }

        public override TDto Fetch()
        {
            return FetchByKeyProps(
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 1),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 2),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 3),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 4),
                GetConfiguredKeyPropNameByIndex(KeyItemsCount, 5),
                GetConfiguredColumnsPrefix()
            );
        }
    }
}
