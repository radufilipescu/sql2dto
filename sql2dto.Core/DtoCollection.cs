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
            _helper.ColumnNamesToOrdinalsChanged += _helper_ColumnNamesToOrdinalsChanged;
        }

        protected virtual void _helper_ColumnNamesToOrdinalsChanged(object sender, EventArgs e)
        {
            SetupMapFunc();
        }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper = null) 
        {
            InnerList = new List<TDto>();
            _helper = helper;
            _mapper = mapper;
            SetupMapFunc();
            SetupColumnNamesToOrdinalsChangedHandler();
        }

        public DtoCollection(ReadHelper helper, string columnsPrefix)
        {
            InnerList = new List<TDto>();
            _helper = helper;
            _mapper = null;
            _columnsPrefix = columnsPrefix;
            SetupMapFunc();
            SetupColumnNamesToOrdinalsChangedHandler();
        }

        public virtual TDto Fetch()
        {
            var dto = _mapFunc();
            InnerList.Add(dto);
            return dto;
        }

        protected TKeyPart FetchKeyPart<TKeyPart>(string keyPartPropName, string columnPrefix = null)
        {
            try
            {
                if (_mapper == null)
                {
                    return (TKeyPart)_helper.GetValue<TDto>(keyPartPropName, columnPrefix);
                }
                else
                {
                    return (TKeyPart)_helper.GetValue(_mapper, keyPartPropName, columnPrefix);
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

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper = null)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<TKey, int>();
        }

        public DtoCollection(ReadHelper helper, string columnsPrefix)
            : base(helper, columnsPrefix)
        {
            KeyesToIndexes = new Dictionary<TKey, int>();
        }

        public TDto FetchByKeyValue(TKey key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
            }
            else
            {
                dto = base.Fetch();
                KeyesToIndexes.Add(key, InnerList.Count - 1);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName, string columnPrefix = null)
        {
            TKey key = FetchKeyPart<TKey>(keyPropName, columnPrefix);
            return FetchByKeyValue(key);
        }

        public override TDto Fetch()
        {
            if (_mapper == null && DtoMapper<TDto>.DefaultOrderedKeyPropNames != null)
            {
                if (DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length != KeyItemsCount)
                {
                    return FetchByKeyProps(
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[0], 
                        DtoMapper<TDto>.DefaultColumnsPrefix
                    );
                }

                throw new InvalidOperationException($"This collection's key has {KeyItemsCount} items while the map config has {DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length}");
            }
            else if (_mapper != null && _mapper.OrderedKeyPropNames != null)
            {
                if (_mapper.OrderedKeyPropNames.Length != KeyItemsCount)
                {
                    return FetchByKeyProps(
                        _mapper.OrderedKeyPropNames[0], 
                        _mapper.ColumnsPrefix
                    );
                }

                throw new InvalidOperationException($"This collection's key has {KeyItemsCount} items while the map config has {_mapper.OrderedKeyPropNames.Length}");
            }

            throw new InvalidOperationException("No key properties configuration was made for current map");
        }
    }

    public class DtoCollection<TDto, TKey1, TKey2> : DtoCollection<TDto> where TDto : new()
        where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
        where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
    {
        private const int KeyItemsCount = 2;
        public Dictionary<(TKey1, TKey2), int> KeyesToIndexes { get; private set; }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper = null)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2), int>();
        }

        public DtoCollection(ReadHelper helper, string columnsPrefix)
            : base(helper, columnsPrefix)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2), int>();
        }

        public TDto FetchByKeyValue((TKey1, TKey2) key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
            }
            else
            {
                dto = base.Fetch();
                KeyesToIndexes.Add(key, InnerList.Count - 1);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName1, string keyPropName2, string columnsPrefix = null)
        {
            TKey1 key1 = FetchKeyPart<TKey1>(keyPropName1, columnsPrefix);
            TKey2 key2 = FetchKeyPart<TKey2>(keyPropName2, columnsPrefix);
            return FetchByKeyValue((key1, key2));
        }

        public override TDto Fetch()
        {
            if (_mapper == null && DtoMapper<TDto>.DefaultOrderedKeyPropNames != null)
            {
                if (DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length != KeyItemsCount)
                {
                    return FetchByKeyProps(
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[0],
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[1],
                        DtoMapper<TDto>.DefaultColumnsPrefix
                    );
                }

                throw new InvalidOperationException($"This collection's key has {KeyItemsCount} items while the map config has {DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length}");
            }
            else if (_mapper != null && _mapper.OrderedKeyPropNames != null)
            {
                if (_mapper.OrderedKeyPropNames.Length != KeyItemsCount)
                {
                    return FetchByKeyProps(
                        _mapper.OrderedKeyPropNames[0],
                        _mapper.OrderedKeyPropNames[1],
                        _mapper.ColumnsPrefix
                    );
                }

                throw new InvalidOperationException($"This collection's key has {KeyItemsCount} items while the map config has {_mapper.OrderedKeyPropNames.Length}");
            }

            throw new InvalidOperationException("No key properties configuration was made for current map");
        }
    }

    public class DtoCollection<TDto, TKey1, TKey2, TKey3> : DtoCollection<TDto> where TDto : new()
        where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
        where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
    {
        private const int KeyItemsCount = 3;
        public Dictionary<(TKey1, TKey2, TKey3), int> KeyesToIndexes { get; private set; }

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper = null)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3), int>();
        }

        public DtoCollection(ReadHelper helper, string columnsPrefix)
            : base(helper, columnsPrefix)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3), int>();
        }

        public TDto FetchByKeyValue((TKey1, TKey2, TKey3) key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
            }
            else
            {
                dto = base.Fetch();
                KeyesToIndexes.Add(key, InnerList.Count - 1);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName1, string keyPropName2, string keyPropName3, string columnsPrefix = null)
        {
            TKey1 key1 = FetchKeyPart<TKey1>(keyPropName1, columnsPrefix);
            TKey2 key2 = FetchKeyPart<TKey2>(keyPropName2, columnsPrefix);
            TKey3 key3 = FetchKeyPart<TKey3>(keyPropName3, columnsPrefix);
            return FetchByKeyValue((key1, key2, key3));
        }

        public override TDto Fetch()
        {
            if (_mapper == null && DtoMapper<TDto>.DefaultOrderedKeyPropNames != null)
            {
                if (DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length != KeyItemsCount)
                {
                    return FetchByKeyProps(
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[0],
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[1],
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[3],
                        DtoMapper<TDto>.DefaultColumnsPrefix
                    );
                }

                throw new InvalidOperationException($"This collection's key has {KeyItemsCount} items while the map config has {DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length}");
            }
            else if (_mapper != null && _mapper.OrderedKeyPropNames != null)
            {
                if (_mapper.OrderedKeyPropNames.Length != KeyItemsCount)
                {
                    return FetchByKeyProps(
                        _mapper.OrderedKeyPropNames[0],
                        _mapper.OrderedKeyPropNames[1],
                        _mapper.OrderedKeyPropNames[3],
                        _mapper.ColumnsPrefix
                    );
                }

                throw new InvalidOperationException($"This collection's key has {KeyItemsCount} items while the map config has {_mapper.OrderedKeyPropNames.Length}");
            }

            throw new InvalidOperationException("No key properties configuration was made for current map");
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

        public DtoCollection(ReadHelper helper, DtoMapper<TDto> mapper = null)
            : base(helper, mapper)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4), int>();
        }

        public DtoCollection(ReadHelper helper, string columnsPrefix)
            : base(helper, columnsPrefix)
        {
            KeyesToIndexes = new Dictionary<(TKey1, TKey2, TKey3, TKey4), int>();
        }

        public TDto FetchByKeyValue((TKey1, TKey2, TKey3, TKey4) key)
        {
            TDto dto;
            if (KeyesToIndexes.TryGetValue(key, out int index))
            {
                dto = InnerList[index];
            }
            else
            {
                dto = base.Fetch();
                KeyesToIndexes.Add(key, InnerList.Count - 1);
            }
            return dto;
        }

        public TDto FetchByKeyProps(string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string columnsPrefix = null)
        {
            TKey1 key1 = FetchKeyPart<TKey1>(keyPropName1, columnsPrefix);
            TKey2 key2 = FetchKeyPart<TKey2>(keyPropName2, columnsPrefix);
            TKey3 key3 = FetchKeyPart<TKey3>(keyPropName3, columnsPrefix);
            TKey4 key4 = FetchKeyPart<TKey4>(keyPropName4, columnsPrefix);
            return FetchByKeyValue((key1, key2, key3, key4));
        }

        public override TDto Fetch()
        {
            if (_mapper == null && DtoMapper<TDto>.DefaultOrderedKeyPropNames != null)
            {
                if (DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length != KeyItemsCount)
                {
                    return FetchByKeyProps(
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[0],
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[1],
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[2],
                        DtoMapper<TDto>.DefaultOrderedKeyPropNames[3],
                        DtoMapper<TDto>.DefaultColumnsPrefix
                    );
                }

                throw new InvalidOperationException($"This collection's key has {KeyItemsCount} items while the map config has {DtoMapper<TDto>.DefaultOrderedKeyPropNames.Length}");
            }
            else if (_mapper != null && _mapper.OrderedKeyPropNames != null)
            {
                if (_mapper.OrderedKeyPropNames.Length != KeyItemsCount)
                {
                    return FetchByKeyProps(
                        _mapper.OrderedKeyPropNames[0],
                        _mapper.OrderedKeyPropNames[1],
                        _mapper.OrderedKeyPropNames[2],
                        _mapper.OrderedKeyPropNames[3],
                        _mapper.ColumnsPrefix
                    );
                }

                throw new InvalidOperationException($"This collection's key has {KeyItemsCount} items while the map config has {_mapper.OrderedKeyPropNames.Length}");
            }

            throw new InvalidOperationException("No key properties configuration was made for current map");
        }
    }
}
