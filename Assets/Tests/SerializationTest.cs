using System.Collections.Generic;
using System;
using NUnit.Framework;
using GimGim.Serialization;
using UnityEngine;

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
        bool success = true;

        success &= decoder.Get("intValue", ref _intValue);
        success &= decoder.Get("stringValue", ref _stringValue);

        return success;
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

    public override bool Decode(IDecoder decoder) {
        bool success = true;

        success &= base.Decode(decoder);
        success &= decoder.Get("extraIntValue", ref _extraIntValue);

        return success;
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

        Assert.AreEqual(
            "{\"string\":\"Hello, World!\",\"int\":1,\"double\":2,\"float\":3,\"long\":4,\"ulong\":5,\"bool\":true,\"decimal\":\"7.0\",\"char\":\"A\",\"uint\":9,\"byte\":10,\"sbyte\":11,\"short\":11,\"ushort\":12}",
            encoder.GetString());
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
    public void TestHashSetEncode() {
        HashSet<int> intSet = new() { 1, 2, 3 };
        HashSet<string> stringSet = new() { "one", "two", "three" };
        JsonEncoder encoder = new();
        encoder.Add("intSet", intSet);
        encoder.Add("stringSet", stringSet);
        Assert.AreEqual("{\"intSet\":[1,2,3],\"stringSet\":[\"one\",\"two\",\"three\"]}", encoder.GetString());
    }

    [Test]
    public void TestQueueEncode() {
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
    public void TestStackEncode() {
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
    public void TestArrayEncode() {
        int[] array = { 1, 2, 3 };
        string[] array2 = { "test", "test2" };
        JsonEncoder encoder = new();
        encoder.Add("array", array);
        encoder.Add("array2", array2);
        Assert.AreEqual("{\"array\":[1,2,3],\"array2\":[\"test\",\"test2\"]}", encoder.GetString());
    }

    [Test]
    public void TestNullableEncode() {
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
        Assert.AreEqual("{\"nestedList\":[{\"key1\":[1,2,3],\"key2\":[4,5,6]},{\"key3\":[7,8,9]}]}",
            encoder.GetString());
    }

    #endregion

    #region Decoding

    [Test]
    public void TestISerializableDecode() {
        string json = "{\"intValue\":42,\"stringValue\":\"Hello, World!\"}";
        JsonDecoder decoder = new(json);
        TestData data = new();
        data.Decode(decoder);
        Assert.AreEqual(42, data.IntValue);
        Assert.AreEqual("Hello, World!", data.StringValue);
    }

    [Test]
    public void TestChildClassDecode() {
        string json = "{\"intValue\":42,\"stringValue\":\"Hello, World!\",\"extraIntValue\":3}";
        JsonDecoder decoder = new(json);
        ChildTestData data = new();
        data.Decode(decoder);
        Assert.AreEqual(42, data.IntValue);
        Assert.AreEqual("Hello, World!", data.StringValue);
        Assert.AreEqual(3, data.ExtraIntValue);
    }

    [Test]
    public void TestTupleDecode() {
        string json = "{\"tuple\":[3,\"test\"]}";
        Tuple<int, string> tuple = new(0, "");
        JsonDecoder decoder = new(json);
        decoder.Get("tuple", ref tuple);
        Assert.AreEqual(3, tuple.Item1);
        Assert.AreEqual("test", tuple.Item2);
    }

    [Test]
    public void TestPrimitiveDecode() {
        string json =
            "{\"string\":\"Hello, World!\",\"int\":1,\"double\":2,\"float\":3,\"long\":4,\"ulong\":5,\"bool\":true,\"decimal\":\"7.0\",\"char\":\"A\",\"uint\":9,\"byte\":10,\"sbyte\":11,\"short\":11,\"ushort\":12}";
        JsonDecoder decoder = new(json);
        string str = null;
        int i = 0;
        float f = 0;
        double d = 0;
        long l = 0;
        ulong ul = 0;
        bool b = false;
        decimal dec = 0;
        char c = ' ';
        uint ui = 0;
        byte by = 0;
        sbyte sb = 0;
        short sh = 0;
        ushort ush = 0;

        decoder.Get("string", ref str);
        decoder.Get("int", ref i);
        decoder.Get("double", ref d);
        decoder.Get("float", ref f);
        decoder.Get("long", ref l);
        decoder.Get("ulong", ref ul);
        decoder.Get("bool", ref b);
        decoder.Get("decimal", ref dec);
        decoder.Get("char", ref c);
        decoder.Get("uint", ref ui);
        decoder.Get("byte", ref by);
        decoder.Get("sbyte", ref sb);
        decoder.Get("short", ref sh);
        decoder.Get("ushort", ref ush);

        Assert.AreEqual("Hello, World!", str);
        Assert.AreEqual(1, i);
        Assert.AreEqual(2.0, d);
        Assert.AreEqual(3.0f, f);
        Assert.AreEqual(4L, l);
        Assert.AreEqual(5UL, ul);
        Assert.AreEqual(true, b);
        Assert.AreEqual(7.0m, dec);
        Assert.AreEqual('A', c);
        Assert.AreEqual(9u, ui);
        Assert.AreEqual(10, by);
        Assert.AreEqual(11, sb);
        Assert.AreEqual(11, sh);
        Assert.AreEqual(12, ush);
    }

    [Test]
    public void TestUnityDataTypesDecode()
    {
        Vector2 vector2 = Vector2.zero;
        Vector3 vector3 = Vector3.zero;
        Vector4 vector4 = Vector4.zero;
        Quaternion quaternion = Quaternion.identity;
        Color color = Color.black;
        Color32 color32 = new(0, 0, 0, 0);
        Rect rect = new(0, 0, 0, 0);
        RectOffset rectOffset = new(0, 0, 0, 0);

        JsonDecoder decoder = new(
            "{\"vector2\":[1,2],\"vector3\":[1,2,3],\"vector4\":[1,2,3,4],\"quaternion\":[1,2,3,4],\"color\":[1,2,3,4],\"color32\":[1,2,3,4],\"rect\":[1,2,3,4],\"rectOffset\":[1,2,3,4]}");
        decoder.Get("vector2", ref vector2);
        decoder.Get("vector3", ref vector3);
        decoder.Get("vector4", ref vector4);
        decoder.Get("quaternion", ref quaternion);
        decoder.Get("color", ref color);
        decoder.Get("color32", ref color32);
        decoder.Get("rect", ref rect);
        decoder.Get("rectOffset", ref rectOffset);

        Assert.IsTrue(vector2 is { x: 1, y: 2 });
        Assert.IsTrue(vector3 is { x: 1, y: 2, z: 3 });
        Assert.IsTrue(vector4 is { x: 1, y: 2, z: 3, w: 4 });
        Assert.IsTrue(quaternion is { x: 1, y: 2, z: 3, w: 4 });
        Assert.IsTrue(color is { r: 1, g: 2, b: 3, a: 4 });
        Assert.IsTrue(color32 is { r: 1, g: 2, b: 3, a: 4 });
        Assert.IsTrue(rect is { x: 1, y: 2, width: 3, height: 4 });
        Assert.IsTrue(rectOffset is { left: 1, right: 2, top: 3, bottom: 4 });
    }

    [Test]
    public void TestNullDecode() {
        string json = "{}";
        JsonDecoder decoder = new(json);
        List<int> list = null;
        decoder.Get("list", ref list);
        Assert.IsNull(list);
    }

    [Test]
    public void TestListDecode() {
        string json = "{\"list\":[1,2,3]}";
        JsonDecoder decoder = new(json);
        List<int> list = new();
        decoder.Get("list", ref list);
        Assert.AreEqual(3, list.Count);
        Assert.AreEqual(1, list[0]);
        Assert.AreEqual(2, list[1]);
        Assert.AreEqual(3, list[2]);
    }

    [Test]
    public void TestDictionaryDecode() {
        string json = "{\"dict\":{\"key1\":1,\"key2\":2}}";
        JsonDecoder decoder = new(json);
        Dictionary<string, int> dict = new();
        decoder.Get("dict", ref dict);
        Assert.AreEqual(2, dict.Count);
        Assert.AreEqual(1, dict["key1"]);
        Assert.AreEqual(2, dict["key2"]);
    }

    [Test]
    public void TestEnumDecode() {
        string json = "{\"list\":[\"Earth\",\"Fire\",\"Water\"]}";
        JsonDecoder decoder = new(json);
        List<TestEnum> list = new();
        decoder.Get("list", ref list);
        Assert.AreEqual(3, list.Count);
        Assert.AreEqual(TestEnum.Earth, list[0]);
        Assert.AreEqual(TestEnum.Fire, list[1]);
        Assert.AreEqual(TestEnum.Water, list[2]);
    }

    [Test]
    public void TestHashSetDecode() {
        string json = "{\"intSet\":[1,2,3],\"stringSet\":[\"one\",\"two\",\"three\"]}";
        JsonDecoder decoder = new(json);
        HashSet<int> intSet = new();
        HashSet<string> stringSet = new();
        decoder.Get("intSet", ref intSet);
        decoder.Get("stringSet", ref stringSet);
        Assert.AreEqual(3, intSet.Count);
        Assert.IsTrue(intSet.Contains(1));
        Assert.IsTrue(intSet.Contains(2));
        Assert.IsTrue(intSet.Contains(3));
        Assert.AreEqual(3, stringSet.Count);
        Assert.IsTrue(stringSet.Contains("one"));
        Assert.IsTrue(stringSet.Contains("two"));
        Assert.IsTrue(stringSet.Contains("three"));
    }

    [Test]
    public void TestQueueDecode() {
        string json = "{\"queue\":[\"first\",\"second\",\"third\"]}";
        JsonDecoder decoder = new(json);
        Queue<string> queue = new();
        decoder.Get("queue", ref queue);
        Assert.AreEqual(3, queue.Count);
        Assert.AreEqual("first", queue.Dequeue());
        Assert.AreEqual("second", queue.Dequeue());
        Assert.AreEqual("third", queue.Dequeue());
    }

    [Test]
    public void TestStackDecode() {
        string json = "{\"stack\":[\"first\",\"second\",\"third\"]}";
        JsonDecoder decoder = new(json);
        Stack<string> stack = new();
        decoder.Get("stack", ref stack);
        Assert.AreEqual(3, stack.Count);
        Assert.AreEqual("third", stack.Pop());
        Assert.AreEqual("second", stack.Pop());
        Assert.AreEqual("first", stack.Pop());
    }

    [Test]
    public void TestArrayDecode() {
        string json = "{\"array\":[1,2,3],\"array2\":[\"test\",\"test2\"]}";
        JsonDecoder decoder = new(json);
        List<int> array = new();
        List<string> array2 = new();
        decoder.Get("array", ref array);
        decoder.Get("array2", ref array2);
        Assert.AreEqual(3, array.Count);
        Assert.AreEqual(1, array[0]);
        Assert.AreEqual(2, array[1]);
        Assert.AreEqual(3, array[2]);
        Assert.AreEqual(2, array2.Count);
        Assert.AreEqual("test", array2[0]);
        Assert.AreEqual("test2", array2[1]);
    }

    [Test]
    public void TestNullableDecode() {
        string json = "{\"hasValue\":7}";
        JsonDecoder decoder = new(json);
        int? hasValue = null;
        float? noValue = null;
        decoder.Get("hasValue", ref hasValue);
        decoder.Get("noValue", ref noValue);
        Assert.AreEqual(7, hasValue);
        Assert.IsNull(noValue);
    }

    [Test]
    public void TestNestedStructuresDecode() {
        string json = "{\"nestedList\":[{\"key1\":[1,2,3],\"key2\":[4,5,6]},{\"key3\":[7,8,9]}]}";
        JsonDecoder decoder = new(json);
        List<Dictionary<string, List<int>>> nestedList = new();
        decoder.Get("nestedList", ref nestedList);
        Assert.AreEqual(2, nestedList.Count);
        Assert.AreEqual(2, nestedList[0].Count);
        Assert.AreEqual(3, nestedList[0]["key1"].Count);
        Assert.AreEqual(3, nestedList[1]["key3"].Count);
    }

    #endregion
}