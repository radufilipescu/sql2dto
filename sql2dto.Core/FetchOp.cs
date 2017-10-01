﻿using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class FetchOp<TDto>
        where TDto : new()
    {
        private class FetchResult<T>
            where T : new()
        {
            public T Current { get; set; }
            public int CurrentIndex { get; set; }
        }

        private class CollectResult<T> : FetchResult<T>
            where T : new()
        {
            public List<T> List { get; set; }
        }

        private ReadHelper _readHelper;
        private Func<FetchResult<TDto>> _fetchFunc;
        private Func<CollectResult<TDto>> _collectFunc;
        private List<Action<FetchResult<TDto>>> _includes;
        private Dictionary<int, Dictionary<int, HashSet<int>>> _parentChildrenBounds;

        private FetchOp(ReadHelper readHelper) 
        {
            _readHelper = readHelper;
            _includes = new List<Action<FetchResult<TDto>>>();
            _parentChildrenBounds = new Dictionary<int, Dictionary<int, HashSet<int>>>();
        }

        private static DtoCollection<TDto> CreateKeyedCollection(ReadHelper readHelper, string columnsPrefix = null, DtoMapper<TDto> mapper = null)
        {
            var keyPropNames = mapper?.OrderedKeyPropNames ?? DtoMapper<TDto>.DefaultOrderedKeyPropNames;
            var genericArguments = new List<Type>
            {
                typeof(TDto)
            };
            foreach (string keyPropName in keyPropNames)
            {
                var keyInnerPropTypeName = mapper?.GetInnerPropType(keyPropName) ?? DtoMapper<TDto>.GetDefaultInnerPropType(keyPropName);
                genericArguments.Add(keyInnerPropTypeName);
            }

            Type collectionGenericType;
            switch (genericArguments.Count)
            {
                case 1:
                    collectionGenericType = typeof(DtoCollection<>);
                    break;
                case 2:
                    collectionGenericType = typeof(DtoCollection<,>);
                    break;
                case 3:
                    collectionGenericType = typeof(DtoCollection<,,>);
                    break;
                case 4:
                    collectionGenericType = typeof(DtoCollection<,,,>);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var collectionType = collectionGenericType.MakeGenericType(genericArguments.ToArray());
            if (mapper != null)
            {
                return (DtoCollection<TDto>)Activator.CreateInstance(collectionType, readHelper, mapper);
            }
            if (columnsPrefix != null)
            {
                return (DtoCollection<TDto>)Activator.CreateInstance(collectionType, readHelper, columnsPrefix);
            }
            else
            {
                return (DtoCollection<TDto>)Activator.CreateInstance(collectionType, readHelper);
            }
        }

        private static DtoCollection<TDto> CreateCollection(ReadHelper readHelper, DtoMapper<TDto> mapper = null, string columnsPrefix = null)
        {
            DtoCollection<TDto> collection;
            var orderedKeyPropNames = mapper?.OrderedKeyPropNames ?? DtoMapper<TDto>.DefaultOrderedKeyPropNames;
            if (orderedKeyPropNames.Length > 0)
            {
                collection = CreateKeyedCollection(readHelper, columnsPrefix, mapper);
            }
            else
            {
                if (mapper != null)
                {
                    collection = new DtoCollection<TDto>(readHelper, mapper);
                }
                else if (columnsPrefix != null)
                {
                    collection = new DtoCollection<TDto>(readHelper, columnsPrefix);
                }
                else
                {
                    collection = new DtoCollection<TDto>(readHelper);
                }
            }
            return collection;
        }

        #region CREATE
        
        #region SETUP FETCH AND COLLECT FUNCS
        private static void SetupDefaultFetchAndCollectFuncs(FetchOp<TDto> fetchOp, DtoCollection<TDto> collection)
        {
            fetchOp._fetchFunc = () =>
            {
                TDto current = collection.Fetch();
                return new FetchResult<TDto> { Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.Fetch();
                return new CollectResult<TDto> { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupFetchAndCollectByKeyValueFuncs<TKey>(FetchOp<TDto> fetchOp, DtoCollection<TDto, TKey> collection, Func<ReadHelper, TKey> keyReadFunc)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            fetchOp._fetchFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new FetchResult<TDto> { Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult<TDto> { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupFetchAndCollectByKeyValueFuncs<TKey1, TKey2>(FetchOp<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2> collection, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            fetchOp._fetchFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new FetchResult<TDto> { Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult<TDto> { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupFetchAndCollectByKeyValueFuncs<TKey1, TKey2, TKey3>(FetchOp<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3> collection, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            fetchOp._fetchFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new FetchResult<TDto> { Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult<TDto> { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupFetchAndCollectByKeyPropNamesFuncs<TKey>(FetchOp<TDto> fetchOp, DtoCollection<TDto, TKey> collection, string keyPropName)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            fetchOp._fetchFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName);
                return new FetchResult<TDto> { Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName);
                return new CollectResult<TDto> { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupFetchAndCollectByKeyPropNamesFuncs<TKey1, TKey2>(FetchOp<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2> collection, string keyPropName1, string keyPropName2)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            fetchOp._fetchFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2);
                return new FetchResult<TDto> { Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2);
                return new CollectResult<TDto> { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupFetchAndCollectByKeyPropNamesFuncs<TKey1, TKey2, TKey3>(FetchOp<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3> collection, string keyPropName1, string keyPropName2, string keyPropName3)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            fetchOp._fetchFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2, keyPropName3);
                return new FetchResult<TDto> { Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2, keyPropName3);
                return new CollectResult<TDto> { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }
        #endregion

        #region No Keys
        public static FetchOp<TDto> Create(ReadHelper readHelper)
        {
            var result = new FetchOp<TDto>(readHelper);
            DtoCollection<TDto> collection = CreateCollection(readHelper, null, null);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create(ReadHelper readHelper, DtoMapper<TDto> mapper)
        {
            var result = new FetchOp<TDto>(readHelper);
            DtoCollection<TDto> collection = CreateCollection(readHelper, mapper, null);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create(string columnsPrefix, ReadHelper readHelper)
        {
            var result = new FetchOp<TDto>(readHelper);
            DtoCollection<TDto> collection = CreateCollection(readHelper, null, columnsPrefix);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }
        #endregion

        #region 1 Key
        public static FetchOp<TDto> Create<TKey>(ReadHelper readHelper)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey>(readHelper);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey>(readHelper, mapper);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(string columnsPrefix, ReadHelper readHelper)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey>(readHelper, columnsPrefix);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(ReadHelper readHelper, Func<ReadHelper, TKey> keyReadFunc)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey>(readHelper);
            SetupFetchAndCollectByKeyValueFuncs(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, TKey> keyReadFunc)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey>(readHelper, columnsPrefix);
            SetupFetchAndCollectByKeyValueFuncs(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(ReadHelper readHelper, string keyPropName)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey>(readHelper);
            SetupFetchAndCollectByKeyPropNamesFuncs(result, collection, keyPropName);
            return result;
        }

        public static FetchOp<TDto> Create<TKey>(string columnsPrefix, ReadHelper readHelper, string keyPropName)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey>(readHelper, columnsPrefix);
            SetupFetchAndCollectByKeyPropNamesFuncs(result, collection, keyPropName);
            return result;
        }
        #endregion

        #region 2 Keys
        public static FetchOp<TDto> Create<TKey1, TKey2>(ReadHelper readHelper)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper, mapper);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper, columnsPrefix);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper);
            SetupFetchAndCollectByKeyValueFuncs(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper, columnsPrefix);
            SetupFetchAndCollectByKeyValueFuncs(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, string keyPropName1, string keyPropName2)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper);
            SetupFetchAndCollectByKeyPropNamesFuncs(result, collection, keyPropName1, keyPropName2);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper, columnsPrefix);
            SetupFetchAndCollectByKeyPropNamesFuncs(result, collection, keyPropName1, keyPropName2);
            return result;
        }
        #endregion

        #region 3 Keys
        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, DtoMapper<TDto> mapper)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, mapper);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, columnsPrefix);
            SetupDefaultFetchAndCollectFuncs(result, collection);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper);
            SetupFetchAndCollectByKeyValueFuncs(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, columnsPrefix);
            SetupFetchAndCollectByKeyValueFuncs(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper);
            SetupFetchAndCollectByKeyPropNamesFuncs(result, collection, keyPropName1, keyPropName2, keyPropName3);
            return result;
        }

        public static FetchOp<TDto> Create<TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOp<TDto>(readHelper);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, columnsPrefix);
            SetupFetchAndCollectByKeyPropNamesFuncs(result, collection, keyPropName1, keyPropName2, keyPropName3);
            return result;
        }
        #endregion

        #endregion

        #region INCLUDE

        #region MAIN INCLUDE
        public FetchOp<TDto> Include<TChildDto>(FetchOp<TChildDto> childOp, Action<TDto, TChildDto> map)
            where TChildDto : new()
        {
            int boundIndex = _parentChildrenBounds.Count;
            _parentChildrenBounds.Add(boundIndex, new Dictionary<int, HashSet<int>>());
            var include = new Action<FetchResult<TDto>>((parent) =>
            {
                var child = childOp.FetchFromCurrentRow();
                if (_parentChildrenBounds[boundIndex].ContainsKey(parent.CurrentIndex))
                {
                    if (_parentChildrenBounds[boundIndex][parent.CurrentIndex].Contains(child.CurrentIndex))
                    {
                        // child already attached to parent
                        return;
                    }
                    else
                    {
                        _parentChildrenBounds[boundIndex][parent.CurrentIndex].Add(child.CurrentIndex);
                    }
                }
                else
                {
                    _parentChildrenBounds[boundIndex].Add(parent.CurrentIndex, new HashSet<int> { child.CurrentIndex });
                }
                map(parent.Current, child.Current);
            });
            _includes.Add(include);
            return this;
        }
        #endregion

        #region No Keys
        public FetchOp<TDto> Include<TChildDto>(Action<TDto, TChildDto> map, Action<FetchOp<TChildDto>> childIncludeConfig = null)
            where TChildDto : new()
        {
            var childFetchOp = FetchOp<TChildDto>.Create(this._readHelper);
            childIncludeConfig?.Invoke(childFetchOp);
            return this.Include(childFetchOp, map);
        }

        public FetchOp<TDto> Include<TChildDto>(Action<TDto, TChildDto> map, DtoMapper<TChildDto> childMapper, Action<FetchOp<TChildDto>> childIncludeConfig = null)
            where TChildDto : new()
        {
            var childFetchOp = FetchOp<TChildDto>.Create(this._readHelper, childMapper);
            childIncludeConfig?.Invoke(childFetchOp);
            return this.Include(childFetchOp, map);
        }

        public FetchOp<TDto> Include<TChildDto>(string columnsPrefix, Action<TDto, TChildDto> map, Action<FetchOp<TChildDto>> childIncludeConfig = null)
            where TChildDto : new()
        {
            var childFetchOp = FetchOp<TChildDto>.Create(columnsPrefix, this._readHelper);
            childIncludeConfig?.Invoke(childFetchOp);
            return this.Include(childFetchOp, map);
        }
        #endregion

        #region 1 Key
        public FetchOp<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(this._readHelper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, DtoMapper<TChildDto> childMapper)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(this._readHelper, childMapper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> map)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(columnsPrefix, this._readHelper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, string keyPropName)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(this._readHelper, keyPropName), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(columnsPrefix, this._readHelper, keyPropName), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> map, Func<ReadHelper, TKey> keyReadFunc)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(this._readHelper, keyReadFunc), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, TKey> keyReadFunc)
            where TChildDto : new()
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey>(columnsPrefix, this._readHelper, keyReadFunc), map);
        }
        #endregion

        #region 2 Keys
        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(this._readHelper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, DtoMapper<TChildDto> childMapper)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(this._readHelper, childMapper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> map)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(columnsPrefix, this._readHelper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(this._readHelper, keyPropName1, keyPropName2), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(columnsPrefix, this._readHelper, keyPropName1, keyPropName2), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(this._readHelper, keyReadFunc), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2>(columnsPrefix, this._readHelper, keyReadFunc), map);
        }
        #endregion

        #region 3 Keys
        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, DtoMapper<TChildDto> childMapper)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, childMapper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> map)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this._readHelper), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, keyPropName1, keyPropName2, keyPropName3), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> map, string keyPropName1, string keyPropName2, string keyPropName3)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this._readHelper, keyPropName1, keyPropName2, keyPropName3), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, keyReadFunc), map);
        }

        public FetchOp<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> map, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc)
            where TChildDto : new()
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            return this.Include(FetchOp<TChildDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this._readHelper, keyReadFunc), map);
        }
        #endregion

        #endregion

        private FetchResult<TDto> FetchFromCurrentRow()
        {
            FetchResult<TDto> result = _fetchFunc();
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
                return FetchFromCurrentRow().Current;
            }
            return default(TDto);
        }

        public List<TDto> All()
        {
            List<TDto> list = null;
            if (_readHelper.Read())
            {
                var collectResult = _collectFunc();
                list = collectResult.List;
                foreach (var include in _includes)
                {
                    if (collectResult.Current == null)
                    {
                        continue;
                    }
                    include(collectResult);
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
                        include(collectResult2);
                    }
                }
            }
            else
            {
                list = new List<TDto>();
            }
            return list;
        }
    }
}
