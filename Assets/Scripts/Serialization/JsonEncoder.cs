using System.Collections;
using System.Runtime.CompilerServices;
using SimpleJSON;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace GimGim.Serialization {
    public class JsonEncoder : IEncoder {
        private readonly JSONNode _rootNode;
        private JSONNode _currentNode;

        public JsonEncoder() : this(new JSONObject()) {}

        public JsonEncoder(JSONNode rootNode) {
            _rootNode = rootNode;
            _currentNode = rootNode;
        }
        
        public string GetString() {
            return _rootNode.ToString();
        }

        #region Encoding as Array
        /// <summary>
        /// Used when the encoder has been initialized as an array.
        /// </summary>
        public void Add<T>(T value) {
            if (value is null) {
                return;
            }

            if (_currentNode is not JSONArray) {
                return;
            }

            switch (value) {
                case ITuple tuple:
                    Encode(tuple);
                    break;
                case ISerializable serializable:
                    Encode(serializable);
                    break;
                case IList list:
                    Encode(list);
                    break;
                case IDictionary dictionary:
                    Encode(dictionary);
                    break;
                
                default: {
                    Type valueType = typeof(T);
                    if (valueType.IsGenericType) {
                        Type genericType = valueType.GetGenericTypeDefinition();
                        if (genericType == typeof(HashSet<>) || genericType == typeof(SortedSet<>) ||
                            genericType == typeof(Stack<>) || genericType == typeof(Queue<>)) {
                            Encode(value as IEnumerable);
                            break;
                        }
                    }
                    
                    Encode(ConvertToJsonNode(value));
                    break;
                }
            }
        }
        
        private void Encode(ITuple tuple) {
            JSONArray array = _currentNode as JSONArray;
            JSONNode previousNode = _currentNode;
            _currentNode = new JSONArray();
            array?.Add(_currentNode);

            for (int i = 0; i < tuple.Length; i++) {
                Add(tuple[i]);
            }
            _currentNode = previousNode;
        }

        private void Encode(ISerializable serializable) {
            JSONArray array = _currentNode as JSONArray;
            JSONNode previousNode = _currentNode;
            _currentNode = new JSONObject();
            array?.Add(_currentNode);

            serializable.Encode(this);
            _currentNode = previousNode;
        }

        private void Encode(IEnumerable enumerable) {
            JSONArray array = _currentNode as JSONArray;
            JSONNode previousNode = _currentNode;
            _currentNode = new JSONArray();
            array?.Add(_currentNode);

            foreach (object item in enumerable) {
                Add(item);
            }

            _currentNode = previousNode;
        }

        private void Encode(IDictionary dictionary) {
            JSONArray array = _currentNode as JSONArray;
            JSONNode previousNode = _currentNode;
            _currentNode = new JSONObject();
            array?.Add(_currentNode);

            foreach (object dictKey in dictionary.Keys) {
                Add(dictKey.ToString(), dictionary[dictKey]);
            }

            _currentNode = previousNode;
        }

        private void Encode(JSONNode value) {
            JSONArray array = _currentNode as JSONArray;
            array?.Add(value);
        }
        
        #endregion
        
        #region Encoding as Dictionary

        /// <summary>
        /// Used when the encoder has been initialized as a dictionary.
        /// </summary>
        public void Add<TK, TV>(TK key, TV value) {
            if (value is null) {
                return;
            }

            if (_currentNode is not JSONObject) {
                return;
            }
            
            string stringKey = GetKeyAsString(key);
            if (stringKey is null) {
                return;
            }
            
            switch (value) {
                case ITuple tuple:
                    Encode(stringKey, tuple);
                    break;
                case ISerializable serializable:
                    Encode(stringKey, serializable);
                    break;
                case IList list:
                    Encode(stringKey, list);
                    break;
                case IDictionary dictionary:
                    Encode(stringKey, dictionary);
                    break;
                
                default: {
                    Type valueType = typeof(TV);
                    if (valueType.IsGenericType) {
                        Type genericType = valueType.GetGenericTypeDefinition();
                        if (genericType == typeof(HashSet<>) || genericType == typeof(SortedSet<>) ||
                            genericType == typeof(Stack<>) || genericType == typeof(Queue<>)) {
                            Encode(stringKey, value as IEnumerable);
                            break;
                        }
                    }
                    Encode(stringKey, ConvertToJsonNode(value));
                    break;
                }
            }
        }
        
        private void Encode(string key, ITuple tuple) {
            JSONObject obj = _currentNode as JSONObject;
            JSONNode previousNode = _currentNode;
            _currentNode = new JSONArray();
            obj?.Add(key, _currentNode);

            for (int i = 0; i < tuple.Length; i++) {
                Add(tuple[i]);
            }
            _currentNode = previousNode;
        }
        
        private void Encode(string key, ISerializable serializable) {
            JSONObject obj = _currentNode as JSONObject;
            JSONNode previousNode = _currentNode;
            _currentNode = new JSONObject();
            obj?.Add(key, _currentNode);

            serializable.Encode(this);
            _currentNode = previousNode;
        }

        private void Encode(string key, IEnumerable enumerable) {
            JSONObject obj = _currentNode as JSONObject;
            JSONNode previousNode = _currentNode;
            _currentNode = new JSONArray();
            obj?.Add(key, _currentNode);
            foreach (var item in enumerable) {
                Add(item);
            }
            
            _currentNode = previousNode;
        }
        
        private void Encode(string key, IDictionary dictionary) {
            JSONObject obj = _currentNode as JSONObject;
            JSONNode previousNode = _currentNode;
            _currentNode = new JSONObject();
            obj?.Add(key, _currentNode);

            foreach (object dictKey in dictionary.Keys) {
                Add(dictKey.ToString(), dictionary[dictKey]);
            }

            _currentNode = previousNode;
        }
        
        private void Encode(string key, JSONNode value) {
            JSONObject obj = _currentNode as JSONObject;
            obj?.Add(key, value);
        }
        
        #endregion
        
        #region JsonNode Conversion
        
        private static JSONNode ConvertToJsonNode<T>(T value) {
            if (value is null) {
                return null;
            }
            
            if (value.GetType().IsEnum) {
                return value.ToString();
            }

            switch (value) {
                case string s: return s;
                case int i: return i;
                case double d: return d;
                case float f: return f;
                case long l: return l;
                case ulong ul: return ul;
                case bool b: return b;
                case decimal m: return m;
                case char c: return c;
                case uint ui: return ui;
                case byte bt: return bt;
                case sbyte sb: return sb;
                case short sh: return sh;
                case ushort ush: return ush;
                case System.DateTime dt: return dt;
                case System.TimeSpan ts: return ts;
                case System.Guid guid: return guid;
                case Vector2 v2: return v2;
                case Vector3 v3: return v3;
                case Vector4 v4: return v4;
                case Color col: return col;
                case Color32 col32: return col32;
                case Quaternion q: return q;
                case Rect rect: return rect;
                case RectOffset offset: return offset;
                default:
                    Debug.Log($"JSONEncoder - Failed to convert {value.GetType().FullName} to JSONNode.");
                    return null;
            }
        }
        
        #endregion
        
        #region Helpers
        
        private static string GetKeyAsString<TK>(TK key) {
            string stringKey = null;
            switch (key) {
                case string str:
                    stringKey = str;
                    break;
                case int i:
                    stringKey = i.ToString();
                    break;
                default: {
                    if (typeof(TK).IsEnum) {
                        stringKey = key.ToString();
                    }
                    else {
                        Debug.Log($"JSONEncoder - Failed to convert {key.GetType().FullName} to string.");
                    }

                    break;
                }
            }
            return stringKey;
        }
        
        #endregion
    }
}