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

        public virtual bool IsDtoCached()
        {
            return false;
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

        protected TKeyPart FetchKeyPart<TKeyPart>(string keyPartPropName, string columnPrefix = null)
        {
            try
            {
                if (_mapper == null)
                {
                    return (TKeyPart)_helper.GetValue<TDto>(keyPartPropName, columnPrefix ?? GetConfiguredColumnsPrefix());
                }
                else
                {
                    return (TKeyPart)_helper.GetValue(_mapper, keyPartPropName, columnPrefix ?? GetConfiguredColumnsPrefix());
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
            if (IsDBNullKeyPart(keyPropName, columnPrefix))
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

        public bool IsDtoCached(TKey key)
        {
            return KeyesToIndexes.ContainsKey(key);
        }

        public bool IsDtoCached(string keyPropName, string columnPrefix = null)
        {
            if (IsDBNullKeyPart(keyPropName, columnPrefix))
            {
                return false;
            }
            TKey key = FetchKeyPart<TKey>(
                keyPropName,
                columnPrefix
            );
            return IsDtoCached(key);
        }

        public override bool IsDtoCached()
        {
            string columnPrefix = GetConfiguredColumnsPrefix();
            string keyPropName = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0);
            return IsDtoCached(keyPropName, columnPrefix);
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
            if (IsDBNullKeyPart(keyPropName1, columnsPrefix)
                || IsDBNullKeyPart(keyPropName2, columnsPrefix))
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

        public bool IsDtoCached((TKey1, TKey2) key)
        {
            return KeyesToIndexes.ContainsKey(key);
        }

        public bool IsDtoCached(string keyPropName1, string keyPropName2, string columnPrefix = null)
        {
            if (IsDBNullKeyPart(keyPropName1, columnPrefix)
                || IsDBNullKeyPart(keyPropName2, columnPrefix))
            {
                return false;
            }
            TKey1 key1 = FetchKeyPart<TKey1>(
                keyPropName1,
                columnPrefix
            );
            TKey2 key2 = FetchKeyPart<TKey2>(
                keyPropName2,
                columnPrefix
            );
            return IsDtoCached((key1, key2));
        }

        public override bool IsDtoCached()
        {
            string columnPrefix = GetConfiguredColumnsPrefix();
            string keyPropName1 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0);
            string keyPropName2 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 1);
            return IsDtoCached(keyPropName1, keyPropName2, columnPrefix);
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
            if (IsDBNullKeyPart(keyPropName1, columnsPrefix)
                || IsDBNullKeyPart(keyPropName2, columnsPrefix)
                || IsDBNullKeyPart(keyPropName3, columnsPrefix))
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

        public bool IsDtoCached((TKey1, TKey2, TKey3) key)
        {
            return KeyesToIndexes.ContainsKey(key);
        }

        public bool IsDtoCached(string keyPropName1, string keyPropName2, string keyPropName3, string columnPrefix = null)
        {
            if (IsDBNullKeyPart(keyPropName1, columnPrefix)
                || IsDBNullKeyPart(keyPropName2, columnPrefix)
                || IsDBNullKeyPart(keyPropName3, columnPrefix))
            {
                return false;
            }
            TKey1 key1 = FetchKeyPart<TKey1>(
                keyPropName1,
                columnPrefix
            );
            TKey2 key2 = FetchKeyPart<TKey2>(
                keyPropName2,
                columnPrefix
            );
            TKey3 key3 = FetchKeyPart<TKey3>(
                keyPropName3,
                columnPrefix
            );
            return IsDtoCached((key1, key2, key3));
        }

        public override bool IsDtoCached()
        {
            string columnPrefix = GetConfiguredColumnsPrefix();
            string keyPropName1 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0);
            string keyPropName2 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 1);
            string keyPropName3 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 2);
            return IsDtoCached(keyPropName1, keyPropName2, keyPropName3, columnPrefix);
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
            if (IsDBNullKeyPart(keyPropName1, columnsPrefix)
                || IsDBNullKeyPart(keyPropName2, columnsPrefix)
                || IsDBNullKeyPart(keyPropName3, columnsPrefix)
                || IsDBNullKeyPart(keyPropName4, columnsPrefix))
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

        public bool IsDtoCached((TKey1, TKey2, TKey3, TKey4) key)
        {
            return KeyesToIndexes.ContainsKey(key);
        }

        public bool IsDtoCached(string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string columnPrefix = null)
        {
            if (IsDBNullKeyPart(keyPropName1, columnPrefix)
                || IsDBNullKeyPart(keyPropName2, columnPrefix)
                || IsDBNullKeyPart(keyPropName3, columnPrefix)
                || IsDBNullKeyPart(keyPropName4, columnPrefix))
            {
                return false;
            }
            TKey1 key1 = FetchKeyPart<TKey1>(
                keyPropName1,
                columnPrefix
            );
            TKey2 key2 = FetchKeyPart<TKey2>(
                keyPropName2,
                columnPrefix
            );
            TKey3 key3 = FetchKeyPart<TKey3>(
                keyPropName3,
                columnPrefix
            );
            TKey4 key4 = FetchKeyPart<TKey4>(
                keyPropName4,
                columnPrefix
            );
            return IsDtoCached((key1, key2, key3, key4));
        }

        public override bool IsDtoCached()
        {
            string columnPrefix = GetConfiguredColumnsPrefix();
            string keyPropName1 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 0);
            string keyPropName2 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 1);
            string keyPropName3 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 2);
            string keyPropName4 = GetConfiguredKeyPropNameByIndex(KeyItemsCount, 3);
            return IsDtoCached(keyPropName1, keyPropName2, keyPropName3, keyPropName4, columnPrefix);
        }
    }
}
