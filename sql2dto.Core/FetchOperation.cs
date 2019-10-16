using System;
using System.Collections.Generic;
using System.Text;

namespace sql2dto.Core
{
    public class FetchOperation<TDto> : IIncludeOperation<TDto>
        
    {
        private class CollectResult
        {
            public TDto Current { get; set; }
            public int CurrentIndex { get; set; }
            public List<TDto> List { get; set; }
        }

        private ReadHelper _readHelper;
        private Func<CollectResult> _collectFunc;
        private List<Action<CollectResult>> _includes;
        private Dictionary<int, Dictionary<int, HashSet<int>>> _parentChildrenBounds;
        internal Dictionary<string, object> InjectedValues { get; private set; }

        private FetchOperation(ReadHelper readHelper, Dictionary<string, object> injectedValues) 
        {
            _readHelper = readHelper;
            _includes = new List<Action<CollectResult>>();
            _parentChildrenBounds = new Dictionary<int, Dictionary<int, HashSet<int>>>();
            InjectedValues = injectedValues;
        }

        private static DtoCollection<TDto> CreateKeyedDtoCollection(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper,
            Dictionary<string, object> injectedValues = null)
        {
            var keyPropNames = mapper?.OrderedKeyPropNames ?? DtoMapper<TDto>.Default.OrderedKeyPropNames;
            var genericArguments = new List<Type>
            {
                typeof(TDto)
            };
            foreach (string keyPropName in keyPropNames)
            {
                var keyInnerPropTypeName = mapper?.PropMapConfigs[keyPropName].InnerPropType ?? DtoMapper<TDto>.Default.PropMapConfigs[keyPropName].InnerPropType;
                genericArguments.Add(keyInnerPropTypeName);
            }

            Type collectionGenericType;
            switch (genericArguments.Count)
            {
                case 1:
                    collectionGenericType = typeof(DtoCollection<>); // no key
                    break;
                case 2:
                    collectionGenericType = typeof(DtoCollection<,>); // 1 key
                    break;
                case 3:
                    collectionGenericType = typeof(DtoCollection<,,>); // 2 keyes
                    break;
                case 4:
                    collectionGenericType = typeof(DtoCollection<,,,>); // 3 keyes
                    break;
                case 5:
                    collectionGenericType = typeof(DtoCollection<,,,,>); // 4 keyes
                    break;
                case 6:
                    collectionGenericType = typeof(DtoCollection<,,,,,>); // 5 keyes
                    break;
                case 7:
                    collectionGenericType = typeof(DtoCollection<,,,,,,>); // 6 keyes
                    break;
                default:
                    throw new NotImplementedException($"Could not create DtoCollection with {genericArguments.Count-1} keyes! Not implemented!");
            }

            var collectionType = collectionGenericType.MakeGenericType(genericArguments.ToArray());
            if (mapper != null)
            {
                if (columnsPrefix != null)
                {
                    return (DtoCollection<TDto>)Activator.CreateInstance(collectionType, columnsPrefix, readHelper, mapper, injectedValues);
                }
                else
                {
                    return (DtoCollection<TDto>)Activator.CreateInstance(collectionType, readHelper, mapper, injectedValues);
                }
            }
            else
            {
                if (columnsPrefix != null)
                {
                    return (DtoCollection<TDto>)Activator.CreateInstance(collectionType, columnsPrefix, readHelper, injectedValues);
                }
                else
                {
                    return (DtoCollection<TDto>)Activator.CreateInstance(collectionType, readHelper, injectedValues);
                }
            }
        }

        private static DtoCollection<TDto> CreateDtoCollection(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper,
            Dictionary<string, object> injectedValues = null)
        {
            DtoCollection<TDto> collection;
            var orderedKeyPropNames = mapper?.OrderedKeyPropNames ?? DtoMapper<TDto>.Default.OrderedKeyPropNames;
            if (orderedKeyPropNames != null 
                && orderedKeyPropNames.Length > 0)
            {
                collection = CreateKeyedDtoCollection(columnsPrefix, readHelper, mapper, injectedValues);
            }
            else
            {
                if (mapper != null)
                {
                    if (columnsPrefix != null)
                    {
                        collection = new DtoCollection<TDto>(columnsPrefix, readHelper, mapper, injectedValues);
                    }
                    else
                    {
                        collection = new DtoCollection<TDto>(readHelper, mapper, injectedValues);
                    }
                }
                else
                {
                    if (columnsPrefix != null)
                    {
                        collection = new DtoCollection<TDto>(columnsPrefix, readHelper, injectedValues);
                    }
                    else
                    {
                        collection = new DtoCollection<TDto>(readHelper, injectedValues);
                    }
                }
            }
            return collection;
        }

        #region CREATE
        
        #region SETUP FETCH AND COLLECT FUNCS
        private static void SetupDefaultCollectFunc(FetchOperation<TDto> fetchOp, DtoCollection<TDto> collection)
        {
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.Fetch();
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyValueFunc<TKey>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey> collection, Func<ReadHelper, TKey> keyReadFunc)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyValueFunc<TKey1, TKey2>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2> collection, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyValueFunc<TKey1, TKey2, TKey3>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3> collection, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyValueFunc<TKey1, TKey2, TKey3, TKey4>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4> collection, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyValueFunc<TKey1, TKey2, TKey3, TKey4, TKey5>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5> collection, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyValueFunc<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> collection, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)> keyReadFunc)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            fetchOp._collectFunc = () =>
            {
                var key = keyReadFunc(fetchOp._readHelper);
                TDto current = collection.FetchByKeyValue(key);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyPropNamesFunc<TKey>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey> collection, string keyPropName)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyPropNamesFunc<TKey1, TKey2>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2> collection, string keyPropName1, string keyPropName2)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyPropNamesFunc<TKey1, TKey2, TKey3>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3> collection, string keyPropName1, string keyPropName2, string keyPropName3)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2, keyPropName3);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyPropNamesFunc<TKey1, TKey2, TKey3, TKey4>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4> collection, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2, keyPropName3, keyPropName4);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyPropNamesFunc<TKey1, TKey2, TKey3, TKey4, TKey5>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5> collection, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }

        private static void SetupCollectByKeyPropNamesFunc<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(FetchOperation<TDto> fetchOp, DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6> collection, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string keyPropName6)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            fetchOp._collectFunc = () =>
            {
                TDto current = collection.FetchByKeyProps(keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5, keyPropName6);
                return new CollectResult { List = collection.InnerList, Current = current, CurrentIndex = collection.LastFetchedIndex };
            };
        }
        #endregion

        #region No Keys
        public static FetchOperation<TDto> Create(ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            DtoCollection<TDto> collection = CreateDtoCollection(
                columnsPrefix: null, 
                readHelper: readHelper, 
                mapper: null,
                injectedValues: injectedValues
            );
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create(ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            DtoCollection<TDto> collection = CreateDtoCollection(
                columnsPrefix: null, 
                readHelper: readHelper, 
                mapper: mapper,
                injectedValues: injectedValues
            );
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            DtoCollection<TDto> collection = CreateDtoCollection(
                columnsPrefix: columnsPrefix,
                readHelper: readHelper, 
                mapper: mapper,
                injectedValues: injectedValues
            );
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create(string columnsPrefix, ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            DtoCollection<TDto> collection = CreateDtoCollection(
                columnsPrefix: columnsPrefix,
                readHelper: readHelper, 
                mapper: null,
                injectedValues: injectedValues
            );
            SetupDefaultCollectFunc(result, collection);
            return result;
        }
        #endregion

        #region 1 Key
        public static FetchOperation<TDto> Create<TKey>(ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey>(readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey>(ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey>(readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey>(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey>(columnsPrefix, readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey>(string columnsPrefix, ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey>(columnsPrefix, readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey>(ReadHelper readHelper, Func<ReadHelper, TKey> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey>(readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, TKey> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey>(ReadHelper readHelper, string keyPropName, Dictionary<string, object> injectedValues = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey>(readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey>(string columnsPrefix, ReadHelper readHelper, string keyPropName, Dictionary<string, object> injectedValues = null)
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName);
            return result;
        }
        #endregion

        #region 2 Keys
        public static FetchOperation<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(columnsPrefix, readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(columnsPrefix, readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2>(ReadHelper readHelper, string keyPropName1, string keyPropName2, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2);
            return result;
        }
        #endregion

        #region 3 Keys
        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(columnsPrefix, readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(columnsPrefix, readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3>(ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2, keyPropName3);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2, keyPropName3);
            return result;
        }
        #endregion

        #region 4 Keys
        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4>(ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4>(readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4>(ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4>(readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4>(columnsPrefix, readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4>(columnsPrefix, readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4>(readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4>(ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4>(readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2, keyPropName3, keyPropName4);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2, keyPropName3, keyPropName4);
            return result;
        }
        #endregion

        #region 5 Keys
        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5>(ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5>(readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5>(ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5>(readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5>(columnsPrefix, readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5>(columnsPrefix, readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5>(readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5>(ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5>(readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5);
            return result;
        }
        #endregion

        #region 6 Keys
        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, ReadHelper readHelper, DtoMapper<TDto> mapper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(columnsPrefix, readHelper, mapper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, ReadHelper readHelper, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(columnsPrefix, readHelper, injectedValues);
            SetupDefaultCollectFunc(result, collection);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, ReadHelper readHelper, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)> keyReadFunc, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyValueFunc(result, collection, keyReadFunc);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string keyPropName6, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5, keyPropName6);
            return result;
        }

        public static FetchOperation<TDto> Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, ReadHelper readHelper, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string keyPropName6, Dictionary<string, object> injectedValues = null)
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var result = new FetchOperation<TDto>(readHelper, injectedValues);
            var collection = new DtoCollection<TDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(columnsPrefix, readHelper, injectedValues);
            SetupCollectByKeyPropNamesFunc(result, collection, keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5, keyPropName6);
            return result;
        }
        #endregion

        #endregion

        #region INCLUDE

        #region MAIN INCLUDE
        internal FetchOperation<TDto> Include<TChildDto>(FetchOperation<TChildDto> childOp, Action<TDto, TChildDto> map)
            
        {
            int boundIndex = _parentChildrenBounds.Count;
            _parentChildrenBounds.Add(boundIndex, new Dictionary<int, HashSet<int>>());
            var include = new Action<CollectResult>((parent) =>
            {
                var child = childOp.FetchFromCurrentRow();
                if (child.Current == null)
                {
                    // no child to map
                    return;
                }

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

        public FetchOperation<TDto> Include<TChildDto>(Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
        {
            var childFetchOp = FetchOperation<TChildDto>.Create(this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto>(string columnsPrefix, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
        {
            var childFetchOp = FetchOperation<TChildDto>.Create(columnsPrefix, this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto>(DtoMapper<TChildDto> childMapper,  Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
        {
            var childFetchOp = FetchOperation<TChildDto>.Create(this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
        {
            var childFetchOp = FetchOperation<TChildDto>.Create(columnsPrefix, this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }
        #endregion

        #region 1 Key
        public FetchOperation<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey>(this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey>(columnsPrefix, this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey>(this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey>(columnsPrefix, this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> includer, string keyPropName, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey>(this._readHelper, keyPropName, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> includer, string keyPropName, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey>(columnsPrefix, this._readHelper, keyPropName, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey>(Action<TDto, TChildDto> includer, Func<ReadHelper, TKey> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey>(this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey>(string columnsPrefix, Action<TDto, TChildDto> includer, Func<ReadHelper, TKey> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey : IComparable, IConvertible, IEquatable<TKey>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey>(columnsPrefix, this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }
        #endregion

        #region 2 Keys
        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2>(this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2>(columnsPrefix, this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2>(this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2>(columnsPrefix, this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2>(this._readHelper, keyPropName1, keyPropName2, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2>(columnsPrefix, this._readHelper, keyPropName1, keyPropName2, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2>(this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2>(string columnsPrefix, Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2>(columnsPrefix, this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }
        #endregion

        #region 3 Keys
        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, string keyPropName3, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, keyPropName1, keyPropName2, keyPropName3, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, string keyPropName3, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this._readHelper, keyPropName1, keyPropName2, keyPropName3, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3>(this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3>(string columnsPrefix, Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2, TKey3)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3>(columnsPrefix, this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }
        #endregion

        #region 4 Keys
        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4>(this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4>(columnsPrefix, this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4>(this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4>(columnsPrefix, this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4>(this._readHelper, keyPropName1, keyPropName2, keyPropName3, keyPropName4, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4>(columnsPrefix, this._readHelper, keyPropName1, keyPropName2, keyPropName3, keyPropName4, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4>(this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4>(string columnsPrefix, Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4>(columnsPrefix, this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }
        #endregion

        #region 5 Keys
        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5>(this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5>(columnsPrefix, this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5>(this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5>(columnsPrefix, this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5>(this._readHelper, keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5>(columnsPrefix, this._readHelper, keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5>(this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5>(string columnsPrefix, Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5>(columnsPrefix, this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }
        #endregion

        #region 6 Keys
        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(columnsPrefix, this._readHelper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, DtoMapper<TChildDto> childMapper, Action<TDto, TChildDto> includer, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(columnsPrefix, this._readHelper, childMapper, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string keyPropName6, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(this._readHelper, keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5, keyPropName6, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, Action<TDto, TChildDto> includer, string keyPropName1, string keyPropName2, string keyPropName3, string keyPropName4, string keyPropName5, string keyPropName6, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(columnsPrefix, this._readHelper, keyPropName1, keyPropName2, keyPropName3, keyPropName4, keyPropName5, keyPropName6, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }

        public FetchOperation<TDto> Include<TChildDto, TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(string columnsPrefix, Action<TDto, TChildDto> includer, Func<ReadHelper, (TKey1, TKey2, TKey3, TKey4, TKey5, TKey6)> keyReadFunc, Action<IIncludeOperation<TChildDto>> then = null)
            
            where TKey1 : IComparable, IConvertible, IEquatable<TKey1>
            where TKey2 : IComparable, IConvertible, IEquatable<TKey2>
            where TKey3 : IComparable, IConvertible, IEquatable<TKey3>
            where TKey4 : IComparable, IConvertible, IEquatable<TKey4>
            where TKey5 : IComparable, IConvertible, IEquatable<TKey5>
            where TKey6 : IComparable, IConvertible, IEquatable<TKey6>
        {
            var childFetchOp = FetchOperation<TChildDto>.Create<TKey1, TKey2, TKey3, TKey4, TKey5, TKey6>(columnsPrefix, this._readHelper, keyReadFunc, this.InjectedValues);
            then?.Invoke(childFetchOp);
            return this.Include(childFetchOp, includer);
        }
        #endregion

        #endregion

        private CollectResult FetchFromCurrentRow()
        {
            CollectResult result = _collectFunc();
            foreach (var include in _includes)
            {
                include(result);
            }
            return result;
        }

        public List<TDto> All()
        {
            List<TDto> list = new List<TDto>();

            while (_readHelper.Read())
            {
                var collectResult = _collectFunc();

                // - collectResult always contains the same .List instance
                // - at the of the while loop, the list var will have the same instance filled with all DTOs
                list = collectResult.List;

                if (collectResult.Current != null)
                {
                    foreach (var include in _includes)
                    {
                        include(collectResult);
                    }
                }
            }

            return list;
        }
    }
}
