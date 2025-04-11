using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using GimGim.Serialization;
using UnityEngine;

namespace Tests {
    internal class TestData : ISerializable {
        private int _intValue;
        private string _stringValue;
        
        public int IntValue {
            get => _intValue;
            set => _intValue = value;
        }
        
        public string StringValue {
            get => _stringValue;
            set => _stringValue = value;
        }
        
        public virtual bool Decode(IDecoder decoder) {
            return true;
        }

        public virtual void Encode(IEncoder encoder) {
            encoder.Add("intValue", _intValue);
            encoder.Add("stringValue", _stringValue);
        }
    }

    internal class ChildTestData : TestData {
        private int _extraIntValue;
        
        public int ExtraIntValue {
            get => _extraIntValue;
            set => _extraIntValue = value;
        }

        public override void Encode(IEncoder encoder) {
            base.Encode(encoder);
            encoder.Add("extraIntValue", _extraIntValue);
        }
    }
    
    public enum TestEnum {
        Earth,
        Fire,
        Water,
        Wind
    }
    
    public class SerializationTest {
        #region Encoding
        
        [Test]
        public void TestISerializableEncode() {
            TestData data = new() {
                IntValue = 42,
                StringValue = "Hello, World!"
            };
            
            JsonEncoder encoder = new();
            data.Encode(encoder);
            Assert.AreEqual("{\"intValue\":42,\"stringValue\":\"Hello, World!\"}", encoder.GetString());
        }

        [Test]
        public void TestChildClassEncode() {
            ChildTestData data = new() {
                IntValue = 42,
                StringValue = "Hello, World!",
                ExtraIntValue = 3
            };
            
            JsonEncoder encoder = new();
            data.Encode(encoder);
            Assert.AreEqual("{\"intValue\":42,\"stringValue\":\"Hello, World!\",\"extraIntValue\":3}", encoder.GetString());
        }

        [Test]
        public void TestTupleEncode() {
            (int a, string b) tuple = (42, "Hello, World!");
            
            JsonEncoder encoder = new();
            encoder.Add("tuple", tuple);
            Assert.AreEqual("{\"tuple\":[42,\"Hello, World!\"]}", encoder.GetString());
        }

        [Test]
        public void TestPrimitiveEncode() {
            JsonEncoder encoder = new();
            encoder.Add("string", "Hello, World!");
            encoder.Add("int", 1);
            encoder.Add("double", 2.0f);
            encoder.Add("float", 3.0f);
            encoder.Add("long", 4L);
            encoder.Add("ulong", 5UL);
            encoder.Add("bool", true);
            encoder.Add("decimal", 7.0m);
            encoder.Add("char", 'A');
            encoder.Add("uint", 9u);
            encoder.Add("byte", (byte)10);
            encoder.Add("sbyte", (sbyte)11);
            encoder.Add("short", (short)11);
            encoder.Add("ushort", (ushort)12);
            
            Assert.AreEqual("{\"string\":\"Hello, World!\",\"int\":1,\"double\":2,\"float\":3,\"long\":4,\"ulong\":5,\"bool\":true,\"decimal\":\"7.0\",\"char\":\"A\",\"uint\":9,\"byte\":10,\"sbyte\":11,\"short\":11,\"ushort\":12}", encoder.GetString());
        }

        [Test]
        public void TestUnityDataTypesEncode() {
            JsonEncoder encoder = new();
            encoder.Add("vector2", new Vector2(1.0f, 2.0f));
            encoder.Add("vector3", new Vector3(1.0f, 2.0f, 3.0f));
            encoder.Add("vector4", new Vector4(1.0f, 2.0f, 3.0f, 4.0f));
            encoder.Add("color", new Color(1.0f, 2.0f, 3.0f, 4.0f));
            encoder.Add("color32", new Color32(1, 2, 3, 4));
            encoder.Add("quaternion", new Quaternion(1.0f, 2.0f, 3.0f, 4.0f));
            encoder.Add("rect", new Rect(1.0f, 2.0f, 3.0f, 4.0f));
            encoder.Add("rectOffset", new RectOffset(1, 2, 3, 4));
            
        }

        [Test]
        public void TestNullEncode() {
            JsonEncoder encoder = new();
            encoder.Add("list", (List<int>)null);
            Assert.AreEqual("{}", encoder.GetString());
        }

        [Test]
        public void TestListEncode() {
            List<int> list = new List<int> { 1, 2, 3 };
            JsonEncoder encoder = new();
            encoder.Add("list", list);
            Assert.AreEqual("{\"list\":[1,2,3]}", encoder.GetString());
        }
        
        [Test]
        public void TestDictionaryEncode() {
            Dictionary<string, int> dict = new Dictionary<string, int> {
                { "key1", 1 },
                { "key2", 2 }
            };
            JsonEncoder encoder = new();
            encoder.Add("dict", dict);
            Assert.AreEqual("{\"dict\":{\"key1\":1,\"key2\":2}}", encoder.GetString());
        }
        
        [Test]
        public void TestEnumEncode() {
            List<TestEnum> list = new() {
                TestEnum.Earth,
                TestEnum.Fire,
                TestEnum.Water
            };
            
            JsonEncoder encoder = new();
            encoder.Add("list", list);
            Assert.AreEqual("{\"list\":[\"Earth\",\"Fire\",\"Water\"]}", encoder.GetString());
        }
        
        [Test]
        public void TestHashSetEncode()
        {
            HashSet<int> intSet = new() { 1, 2, 3 };
            HashSet<string> stringSet = new() { "one", "two", "three" };
            JsonEncoder encoder = new();
            encoder.Add("intSet", intSet);
            encoder.Add("stringSet", stringSet);
            Assert.AreEqual("{\"intSet\":[1,2,3],\"stringSet\":[\"one\",\"two\",\"three\"]}", encoder.GetString());
        }
        
        [Test]
        public void TestQueueEncode()
        {
            Queue<string> queue = new();
            queue.Enqueue("first");
            queue.Enqueue("second");
            queue.Enqueue("third");

            JsonEncoder encoder = new();
            encoder.Add("queue", queue);
            Assert.AreEqual("{\"queue\":[\"first\",\"second\",\"third\"]}", encoder.GetString());

            string[] queueArray = queue.ToArray();
            Assert.AreEqual("first", queueArray[0]);
            Assert.AreEqual("second", queueArray[1]);
            Assert.AreEqual("third", queueArray[2]);
        }
        
        [Test]
        public void TestStackEncode()
        {
            Stack<string> stack = new();
            stack.Push("first");
            stack.Push("second");
            stack.Push("third");

            JsonEncoder encoder = new();
            encoder.Add("stack", stack);
            Assert.AreEqual("{\"stack\":[\"third\",\"second\",\"first\"]}", encoder.GetString());

            string[] stackArray = stack.ToArray();
            Assert.AreEqual("third", stackArray[0]);
            Assert.AreEqual("second", stackArray[1]);
            Assert.AreEqual("first", stackArray[2]);
        }
        
        [Test]
        public void TestArrayEncode()
        {
            int[] array = { 1, 2, 3 };
            string[] array2 = { "test", "test2" };
            JsonEncoder encoder = new();
            encoder.Add("array", array);
            encoder.Add("array2", array2);
            Assert.AreEqual("{\"array\":[1,2,3],\"array2\":[\"test\",\"test2\"]}", encoder.GetString());
        }
        
        [Test]
        public void TestNullableEncode()
        {
            int? hasValue = 7;
            JsonEncoder encoder = new();
            encoder.Add("hasValue", hasValue);
            encoder.Add("noValue", (float?)null);
            Assert.AreEqual("{\"hasValue\":7}", encoder.GetString());
        }

        [Test]
        public void TestNestedStructuresEncode() {
            List<Dictionary<string, List<int>>> nestedList = new() {
                new Dictionary<string, List<int>> {
                    { "key1", new List<int> { 1, 2, 3 } },
                    { "key2", new List<int> { 4, 5, 6 } }
                },
                new Dictionary<string, List<int>> {
                    { "key3", new List<int> { 7, 8, 9 } }
                }
            };
            
            JsonEncoder encoder = new();
            encoder.Add("nestedList", nestedList);
            Assert.AreEqual("{\"nestedList\":[{\"key1\":[1,2,3],\"key2\":[4,5,6]},{\"key3\":[7,8,9]}]}", encoder.GetString());
        }
        
        #endregion
    }
}