using SimpleJSON;
using System.Reflection;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using GimGim.Utility;


namespace GimGim.Serialization {
    public class JsonDecoder : IDecoder {
        private readonly JSONNode _rootNode;
        private JSONNode _currentNode;
        
        public JSONNode RootNode => _rootNode;
        public JSONNode CurrentNode => _currentNode;
        
        /// <summary>
        /// Used to cache the constructors of specified types to speed up instantiation.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, Func<object>> Constructors = new();
        private static readonly ConcurrentDictionary<string, Type> Types = new();
        private static readonly List<string> AllowedAssemblies = new() { "GimGim", "Assembly-CSharp" };
        
        public JsonDecoder(string encoded) : this(JSON.Parse(encoded)){}
        
        public JsonDecoder(JSONNode rootNode) {
            _rootNode = rootNode;
            _currentNode = _rootNode;
        }
        
        /// <summary>
        /// Gets the value of the specified key from the current node. Integer, string and enum keys are supported.
        /// Enum keys are converted into string keys.
        /// </summary>
        /// <param name="key">Specified key to check.</param>
        /// <param name="value">Object to be populated from the decoder.</param>
        /// <param name="defaultValue">Default value object for the type.</param>
        /// <returns>Boolean to show if the instantiation succeeded.</returns>
        public bool Get<TK, TV>(TK key, ref TV value, TV defaultValue = default) {
            bool success = false;

            switch (key) {
                case int intKey:
                    success = GetWithInt(intKey, ref value, defaultValue);
                    break;
                case string stringKey:
                    success = GetWithString(stringKey, ref value, defaultValue);
                    break;
                default: {
                    if (typeof(TK).IsEnum) {
                        string enumKey = Enum.GetName(typeof(TK), key);
                        success = GetWithString(enumKey, ref value, defaultValue);
                    }
                    else {
                        Debug.Log($"JSONDecoder - Unsupported key type: {typeof(TK).FullName}");
                    }
                    break;
                }
            }

            return success;
        }
        
        /// <summary>
        /// Gets the value of the specified integer key from the current node.
        /// </summary>
        private bool GetWithInt<T>(int intKey, ref T value, T defaultValue = default(T)) {
            bool success = false;
            if (_currentNode is JSONArray arrayNode && intKey < arrayNode.Count) {
                JSONNode subNode = arrayNode[intKey];
                if (subNode != null) {
                    JSONNode previousNode = _currentNode;
                    _currentNode = subNode;
                    success = DecodeCurrent(ref value);
                    _currentNode = previousNode;
                }
            }
            
            if (!success) {
                value = defaultValue;
            }
            return success;
        }
        
        /// <summary>
        /// Gets the value of the specified string key from the current node.
        /// </summary>
        private bool GetWithString<T>(string key, ref T value, T defaultValue = default(T))
        {
            bool success = false;

            if (_currentNode is JSONObject objectNode && objectNode.HasKey(key)) {
                JSONNode subNode = objectNode[key];
                if (subNode != null) {
                    JSONNode previousNode = _currentNode;
                    _currentNode = subNode;
                    success = DecodeCurrent(ref value);
                    _currentNode = previousNode;
                }
            }

            if (!success) {
                value = defaultValue;
            }

            return success;
        }
        
        #region Decoding data

        /// <summary>
        /// Gets the value of the specified key and populates the object with the value. Tries to find the type of the
        /// objects and then tries to decode and populate the object.
        /// </summary>
        /// <param name="obj">Object to populate.</param>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <returns>Boolean if populating the object succeeded.</returns>
        private bool DecodeCurrent<T>(ref T obj) {
            bool success;
            Type valueType = typeof(T);
            Type[] typeInterfaces = valueType.GetInterfaces();

            if (typeof(IDictionary).IsAssignableFrom(valueType)) {
                success = DecodeDictionary(out obj);
            }
            else if (typeof(IList).IsAssignableFrom(valueType) ||
                     typeInterfaces.Any(type =>
                         type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ISet<>))) {
                success = DecodeCollection(out obj, "Add");
            }
            else if (typeof(ITuple).IsAssignableFrom(valueType)) {
                success = valueType.IsValueType ? DecodeValueTuple(out obj) : DecodeTuple(out obj);
            }
            else if (typeof(ISerializable).IsAssignableFrom(valueType)) {
                success = DecodeClass(ref obj);
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Stack<>)) {
                success = DecodeCollection(out obj, "Push");
            }
            else if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Queue<>)) {
                success = DecodeCollection(out obj, "Enqueue");
            }
            else {
                T temp = default;
                success = DecodeJsonNode(ref temp);
                obj = temp;
            }

            return success;
        }

        /// <summary>
        /// Decodes a collection.
        /// </summary>
        private bool DecodeCollection<T>(out T value, string addFunctionName)
        {
            bool success = false;

            Type[] typeArguments = typeof(T).GetGenericArguments();
            Type childType = typeArguments[0];

            MethodInfo getMethod = ReflectionUtility.GetGenericMethod(GetType(), "Get", typeof(int), childType);

            object collection = CreateInstance(typeof(T));
            MethodInfo addMethod = ReflectionUtility.GetMethod(collection.GetType(), addFunctionName);

            JSONArray arrayNode = _currentNode as JSONArray;
            if (arrayNode != null && addMethod != null) {
                for (int index = 0; index < arrayNode.Count; index++) {
                    object child = InstantiateObjectForNode(arrayNode[index], childType);
                    object[] parameters = { index, child, child };
                    getMethod.Invoke(this, parameters);
                    addMethod.Invoke(collection, new[] { parameters[1] });
                }

                success = true;
            }

            value = (T)collection;
            return success;
        }

        /// <summary>
        /// Decodes a dictionary.
        /// </summary>
        private bool DecodeDictionary<T>(out T obj) {
            bool success = false;

            Type[] typeArguments = typeof(T).GetGenericArguments();
            Type keyType = typeArguments[0];
            Type valueType = typeArguments[1];

            MethodInfo getMethod = ReflectionUtility.GetGenericMethod(GetType(), "Get", typeof(string), valueType);

            object collection = CreateInstance(typeof(T));
            MethodInfo addMethod = ReflectionUtility.GetMethod(collection.GetType(), "Add");

            JSONObject objectNode = _currentNode as JSONObject;
            if (objectNode != null) {
                foreach (string key in objectNode.Keys) {
                    object convertedKey = GetConvertedKey(keyType, key);

                    object child = InstantiateObjectForNode(objectNode[key], valueType);
                    object[] parameters = { key, child, child };
                    getMethod.Invoke(this, parameters);
                    addMethod.Invoke(collection, new[] { convertedKey, parameters[1] });
                }

                success = true;
            }

            obj = (T)collection;
            return success;
        }
        
        /// <summary>
        /// Decodes a tuple.
        /// </summary>
        private bool DecodeTuple<T>(out T value) {
            bool success = false;

            Type[] typeArguments = typeof(T).GetGenericArguments();
            object[] tupleObjects = new object[typeArguments.Length];
            JSONArray arrayNode = _currentNode as JSONArray;
            if (arrayNode != null) {
                for (int index = 0; index < arrayNode.Count; index++) {
                    Type childType = typeArguments[index];
                    MethodInfo getMethod = ReflectionUtility.GetGenericMethod(GetType(), "Get", typeof(int), childType);

                    object child = InstantiateObjectForNode(arrayNode[index], childType);
                    object[] parameters = { index, child, child };
                    getMethod.Invoke(this, parameters);
                    tupleObjects[index] = parameters[1];
                }

                success = true;
            }

            MethodInfo tupleCreator = ReflectionUtility.GetGenericMethod(typeof(Tuple), "Create", typeArguments);
            value = (T)tupleCreator.Invoke(null, tupleObjects);
            return success;
        }

        /// <summary>
        /// Decodes a ValueTuple. Has to use explicit constructor creation as it does not have a parameterless constructor.
        /// </summary>
        private bool DecodeValueTuple<T>(out T value) {
            bool success = false;

            Type valueType = typeof(T);
            Type[] typeArguments = valueType.GetGenericArguments();
            object[] tupleObjects = new object[typeArguments.Length];
            JSONArray arrayNode = _currentNode as JSONArray;
            if (arrayNode != null && arrayNode.Count == typeArguments.Length) {
                for (int index = 0; index < arrayNode.Count; index++) {
                    Type childType = typeArguments[index];
                    MethodInfo getMethod = GetType().GetMethod("Get", BindingFlags.Instance | BindingFlags.Public)?.MakeGenericMethod(typeof(int), childType);

                    object child = childType.IsValueType ? Activator.CreateInstance(childType) : null;
                    object[] parameters = { index, child, child };
                    getMethod?.Invoke(this, parameters);
                    tupleObjects[index] = parameters[1];
                }
            }

            ConstructorInfo constructor = valueType.GetConstructor(typeArguments);
            if (constructor != null) {
                value = (T)constructor.Invoke(tupleObjects);
                success = true;
            } 
            else {
                Debug.LogError($"JSONDecoder - No matching constructor found for {valueType.FullName}");
                value = default;
            }
            
            return success;
        }
        
        /// <summary>
        /// Decodes a custom class that implements ISerializable.
        /// </summary>
        private bool DecodeClass<T>(ref T value) {
            ISerializable decodable;

            if (value != null) {
                decodable = (ISerializable)value;
            }
            else {
                decodable = InstantiateObjectForNode(_currentNode, typeof(T)) as ISerializable;
                value = (T)decodable;
            }

            // populate the value object data
            bool success = false;
            if (decodable != null) {
                success = decodable.Decode(this);
                if (!success) {
                    Debug.Log($"JSONDecoder - Failed to decode class of type: {decodable.GetType()}");
                }
            }

            return success;
        }
        
        /// <summary>
        /// Decodes a JSONNode into a specified type. This is used for primitive types, enums, objects and classes
        /// that do not implement the ISerializable interface.
        /// </summary>
        private bool DecodeJsonNode<T>(ref T value) {
            bool success = true;

            Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // classes (value will be null so the switch won't pick up on them)
            if (underlyingType == typeof(string)) {
                value = (T)(object)_currentNode.Value;
            }
            else if (underlyingType == typeof(RectOffset)) {
                value = (T)(object)_currentNode.ReadRectOffset();
            }
            else if (underlyingType.IsEnum) {
                success = DecodeEnum(ref value);
            }
            else if (Type.GetTypeCode(underlyingType) == TypeCode.Object) {
                success = DecodeObjectType(ref value);
            }
            else {
                success = DecodePrimitiveType(underlyingType, ref value);
            }

            return success;
        }
        
        /// <summary>
        /// Decodes an enum type.
        /// </summary>
        private bool DecodeEnum<T>(ref T value) {
            bool success = false;

            if (_currentNode is not JSONString stringNode) return false;
            
            try {
                value = (T)Enum.Parse(typeof(T), stringNode.Value);
                success = true;
            }
            catch {
                Debug.Log($"JSONDecoder - Failed to parse enum of type: {typeof(T)} with value: {stringNode.Value}");
            }

            return success;
        }
        
        /// <summary>
        /// Decodes Unity and SimpleJSON types.
        /// </summary>
        private bool DecodeObjectType<T>(ref T value) {
            bool success = true;

            Type underlyingType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            if (underlyingType == typeof(DateTime)) {
                value = (T)(object)_currentNode.AsDateTime;
            }
            else if (underlyingType == typeof(TimeSpan)) {
                value = (T)(object)_currentNode.AsTimeSpan;
            }
            else if (underlyingType == typeof(Guid)) {
                value = (T)(object)_currentNode.AsGuid;
            }
            else if (underlyingType == typeof(Vector2)) {
                value = (T)(object)_currentNode.ReadVector2();
            }
            else if (underlyingType == typeof(Vector3)) {
                value = (T)(object)_currentNode.ReadVector3();
            }
            else if (underlyingType == typeof(Vector4)) {
                value = (T)(object)_currentNode.ReadVector4();
            }
            else if (underlyingType == typeof(Color)) {
                value = (T)(object)_currentNode.ReadColor();
            }
            else if (underlyingType == typeof(Color32)) {
                value = (T)(object)_currentNode.ReadColor32();
            }
            else if (underlyingType == typeof(Quaternion)) {
                value = (T)(object)_currentNode.ReadQuaternion();
            }
            else if (underlyingType == typeof(Rect)) {
                value = (T)(object)_currentNode.ReadRect();
            }
            else if (underlyingType == typeof(JSONObject)) {
                value = (T)(object)_currentNode.AsObject;
            }
            else if (underlyingType == typeof(JSONArray)) {
                value = (T)(object)_currentNode.AsArray;
            }
            else {
                Debug.Log($"JSONDecoder - Could not convert object from JSONNode to {typeof(T).FullName}");
                success = false;
            }

            return success;
        }
        /// <summary>
        /// Decodes primitive types.
        /// </summary>
        private bool DecodePrimitiveType<T>(Type underlyingType, ref T value) {
            bool success = true;

            switch (Type.GetTypeCode(underlyingType)) {
                case TypeCode.Int32:
                    value = (T)(object)_currentNode.AsInt;
                    break;
                case TypeCode.Double:
                    value = (T)(object)_currentNode.AsDouble;
                    break;
                case TypeCode.Single:
                    value = (T)(object)_currentNode.AsFloat;
                    break;
                case TypeCode.Int64:
                    value = (T)(object)_currentNode.AsLong;
                    break;
                case TypeCode.UInt64:
                    value = (T)(object)_currentNode.AsULong;
                    break;
                case TypeCode.Boolean:
                    value = (T)(object)_currentNode.AsBool;
                    break;
                case TypeCode.Decimal:
                    value = (T)(object)_currentNode.AsDecimal;
                    break;
                case TypeCode.Char:
                    value = (T)(object)_currentNode.AsChar;
                    break;
                case TypeCode.UInt32:
                    value = (T)(object)_currentNode.AsUInt;
                    break;
                case TypeCode.Byte:
                    value = (T)(object)_currentNode.AsByte;
                    break;
                case TypeCode.SByte:
                    value = (T)(object)_currentNode.AsSByte;
                    break;
                case TypeCode.Int16:
                    value = (T)(object)_currentNode.AsShort;
                    break;
                case TypeCode.UInt16:
                    value = (T)(object)_currentNode.AsUShort;
                    break;

                default:
                    Debug.Log($"JSONDecoder - Could not convert primitive from JSONNode to {typeof(T).FullName}");
                    success = false;
                    break;
            }
            return success;
        }

        #endregion
        
        #region Helpers
        /// <summary>
        /// Instantiates an object for the specified node. If the node has a CLASS key, it will try to find the type
        /// of the class and instantiate it. If not, it will use the fallback type.
        /// </summary>
        private static object InstantiateObjectForNode(JSONNode node, Type fallbackType) {
            object result;
            
            JSONObject objectNode = node as JSONObject;
            JSONString classNode = objectNode?["CLASS"] as JSONString;
            string className = classNode != null ? classNode.Value : string.Empty;
            if (className != string.Empty) {
                result = (ISerializable)CreateInstance(className);
            }
            else {
                result = CreateInstance(fallbackType);
            }

            return result;
        }
        
        /// <summary>
        /// Gets the converted key for the specified type.
        /// </summary>
        private static object GetConvertedKey(Type keyType, string key) {
            object convertedKey;
            if (keyType == typeof(int) && int.TryParse(key, out int intKey)) {
                convertedKey = intKey;
            }
            else if (keyType.IsEnum) {
                convertedKey = Enum.Parse(keyType, key);
            }
            else {
                convertedKey = key;
            }

            return convertedKey;
        }
        
        #endregion
        
        #region Caching constructors and types
        /// <summary>
        /// Invokes the constructor of the specified type using the dictionaries of cached constructors and caches it
        /// if it is not already cached.
        /// </summary>
        private static object CreateInstance(Type type) {
            Func<object> constructor = null;
            if (type != null && !Constructors.TryGetValue(type, out constructor)) {
                constructor = CacheFunc(type);
            }

            return constructor?.Invoke();
        }

        /// <summary>
        /// Caches the constructor of the specified type. ValueTuples are handled differently.
        /// </summary>
        private static Func<object> CacheFunc(Type type) {
            bool isValueTuple = typeof(ITuple).IsAssignableFrom(type) && type.IsValueType;
            Func<object> func = isValueTuple ? CacheValueTupleFunc(type) : CacheGenericFunc(type);
            
            if (func == null && !type.IsPrimitive && !type.IsEnum && type != typeof(string) && type != typeof(decimal)) {
                Debug.LogError($"JSONDecoder - Could not find a parameterless constructor for type: {type.FullName}");
            }

            Constructors.TryAdd(type, func);
            return func;
        }

        /// <summary>
        /// Caches the constructor of the specified type.
        /// </summary>
        private static Func<object> CacheGenericFunc(Type type) {
            Func<object> func = null;
            
            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor != null) {
                Expression newExpr = Expression.New(constructor);
                LambdaExpression lambdaExpr = Expression.Lambda(typeof(Func<object>), newExpr);
                func = (Func<object>)lambdaExpr.Compile();
            }

            return func;
        }

        /// <summary>
        /// Caches the ValueTuple constructor as they don't have a parameterless constructor.
        /// </summary>
        private static Func<object> CacheValueTupleFunc(Type type) {
            Func<object> func = null;
            
            ConstructorInfo constructor = type.GetConstructors().FirstOrDefault();
            if (constructor != null) {
                ParameterInfo[] parameters = constructor.GetParameters();
                object[] defaultValues = parameters
                    .Select(p => p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null)
                    .ToArray();

                func = () => constructor.Invoke(defaultValues);
            }
            
            
            return func;
        }

        /// <summary>
        /// Invokes the constructor of the specified class name using the dictionaries of cached constructors and caches
        /// it if it is not already cached.
        /// </summary>
        private static object CreateInstance(string className) {
            Type type = null;
            if (className != null && !Types.TryGetValue(className, out type)) {
                type = CacheType(className);
            }
            return CreateInstance(type);
        }

        /// <summary>
        /// Caches the constructor of the specified class name.
        /// </summary>
        private static Type CacheType(string className) {
            Type type = null;
            
            IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(assembly =>
                AllowedAssemblies.Any(allowed => assembly.FullName.StartsWith(allowed)));
            foreach (Assembly assembly in assemblies) {
                type = assembly.GetType(className);
                if (type != null) {
                    break;
                }
            }
            if (type == null) {
                Debug.LogError($"JSONDecoder - Could not find type: {className}");
            }
            
            Types.TryAdd(className, type);
            return type;
        }

        #endregion
    }
}