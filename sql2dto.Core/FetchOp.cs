using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class FetchOp<TDto>
        where TDto : new()
    {
        private class CollectResult<T>
            where T : new()
        {
            public List<T> List { get; set; }
            public T Current { get; set; }
        }

        private ReadHelper _readHelper;
        private Func<TDto> _fetchFunc;
        private Func<CollectResult<TDto>> _collectFunc;
        private List<Action<TDto>> _includes;

        private FetchOp(ReadHelper readHelper) 
        {
            _readHelper = readHelper;
            _includes = new List<Action<TDto>>();
        }

        public static FetchOp<TDto> Create(ReadHelper readHelper, string columnsPrefix = null)
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.Fetch();
            };
            result._collectFunc = () =>
            {
                var current = list.Fetch();
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create(ReadHelper readHelper, DtoMapper<TDto> mapper)
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto>(readHelper, mapper);
            result._fetchFunc = () =>
            {
                return list.Fetch();
            };
            result._collectFunc = () =>
            {
                var current = list.Fetch();
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(ReadHelper readHelper, string columnsPrefix = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.Fetch();
            };
            result._collectFunc = () =>
            {
                var current = list.Fetch();
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(ReadHelper readHelper, Func<ReadHelper, TKey> keyReadFunc, string columnsPrefix = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.FetchByKeyValue(keyReadFunc(result._readHelper));
            };
            result._collectFunc = () =>
            {
                var current = list.FetchByKeyValue(keyReadFunc(result._readHelper));
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(ReadHelper readHelper, string keyPropName, string columnsPrefix = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.FetchByKeyProps(keyPropName);
            };
            result._collectFunc = () =>
            {
                var current = list.FetchByKeyProps(keyPropName);
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey>(readHelper, mapper);
            result._fetchFunc = () =>
            {
                return list.Fetch();
            };
            result._collectFunc = () =>
            {
                var current = list.Fetch();
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, string columnsPrefix = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey1, TKey2>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.Fetch();
            };
            result._collectFunc = () =>
            {
                var current = list.Fetch();
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc, string columnsPrefix = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey1, TKey2>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.FetchByKeyValue(keyReadFunc(result._readHelper));
            };
            result._collectFunc = () =>
            {
                var current = list.FetchByKeyValue(keyReadFunc(result._readHelper));
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, string keyPropName1, string keyPropName2, string columnsPrefix = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey1, TKey2>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.FetchByKeyProps(keyPropName1, keyPropName2, columnsPrefix);
            };
            result._collectFunc = () =>
            {
                var current = list.FetchByKeyProps(keyPropName1, keyPropName2, columnsPrefix);
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey1, TKey2>(readHelper, mapper);
            result._fetchFunc = () =>
            {
                return list.Fetch();
            };
            result._collectFunc = () =>
            {
                var current = list.Fetch();
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, string columnsPrefix = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.Fetch();
            };
            result._collectFunc = () =>
            {
                var current = list.Fetch();
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc, string columnsPrefix = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.FetchByKeyValue(keyReadFunc(result._readHelper));
            };
            result._collectFunc = () =>
            {
                var current = list.FetchByKeyValue(keyReadFunc(result._readHelper));
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, string columnsPrefix = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, columnsPrefix);
            result._fetchFunc = () =>
            {
                return list.FetchByKeyProps(keyPropName1, keyPropName2, keyPropName3, columnsPrefix);
            };
            result._collectFunc = () =>
            {
                var current = list.FetchByKeyProps(keyPropName1, keyPropName2, keyPropName3, columnsPrefix);
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var list = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, mapper);
            result._fetchFunc = () =>
            {
                return list.Fetch();
            };
            result._collectFunc = () =>
            {
                var current = list.Fetch();
                return new CollectResult<TDto> { List = list.InnerList, Current = current };
            };
            return result;
        }








        public FetchOp<TDto> Include<TChildDto>(FetchOp<TChildDto> childOp, Action<TDto, TChildDto> map)
            where TChildDto : new()
        {
            var include = new Action<TDto>((root) =>
            {
                var inner = childOp.FetchFromCurrentRow();
                if (inner == null)
                {
                    return;
                }
                map(root, inner);
            });
            _includes.Add(include);
            return this;
        }

        public FetchOp<TDto> Include<TChildDto>(Action<TDto, TChildDto> map, string columnsPrefix = null)
            where TChildDto : new()
        {
            return this.Include(FetchOp<TChildDto>.Create(this._readHelper, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, string columnsPrefix = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(this._readHelper, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, string keyPropName, string columnsPrefix = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(this._readHelper, keyPropName, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, Func<ReadHelper, TKey> keyReadFunc, string columnsPrefix = null)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(this._readHelper, keyReadFunc, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, string columnsPrefix = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(this._readHelper, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string columnsPrefix = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(this._readHelper, keyPropName1, keyPropName2, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc, string columnsPrefix = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(this._readHelper, keyReadFunc, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, string columnsPrefix = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3, string columnsPrefix = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, keyPropName1, keyPropName2, keyPropName3, columnsPrefix), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc, string columnsPrefix = null)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, keyReadFunc, columnsPrefix), map);
        }

        private TDto FetchFromCurrentRow()
        {
            var result = _fetchFunc();
            if (result == null)
            {
                return result;
            }
            foreach (var include in _includes)
            {
                include(result);
            }
            return result;
        }

        public TDto One()
        {
            if (_readHelper.Read())
            {
                return FetchFromCurrentRow();
            }
            return default(TDto);
        }

        public List<TDto> All()
        {
            List<TDto> result = null;
            if (_readHelper.Read())
            {
                var collectResult = _collectFunc();
                result = collectResult.List;
                foreach (var include in _includes)
                {
                    if (collectResult.Current == null)
                    {
                        continue;
                    }
                    include(collectResult.Current);
                }
                while (_readHelper.Read())
                {
                    var collectResult2 = _collectFunc();
                    foreach (var include in _includes)
                    {
                        if (collectResult2.Current == null)
                        {
                            continue;
                        }
                        include(collectResult2.Current);
                    }
                }
            }
            else
            {
                result = new List<TDto>();
            }
            return result;
        }
    }
}
