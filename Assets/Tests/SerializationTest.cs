using System.Collections.Generic;
using System;
using GimGim.Data;
using GimGim.Enums;
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
    #region Encoding Tests

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
        Tuple<int, string> tuple = new Tuple<int, string>(42, "Hello, World!");

        JsonEncoder encoder = new();
        encoder.Add("tuple", tuple);
        Assert.AreEqual("{\"tuple\":[42,\"Hello, World!\"]}", encoder.GetString());
    }

    [Test]
    public void TestValueTupleEncode() {
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

    #region Decoding Tests

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
    public void TestValueTupleDecode() {
        string json = "{\"valueTuple\":[3,\"test\"]}";
        (int num, string str) valueTuple = new(0, "");
        JsonDecoder decoder = new(json);
        decoder.Get("valueTuple", ref valueTuple);
        Assert.AreEqual(3, valueTuple.num);
        Assert.AreEqual("test", valueTuple.str);
    }

    [Test]
    public void TestListOfValueTuplesDecode() {
        string json =
            "{\"cards\":[[1044, 4],[1030, 2],[1069, 4],[1033, 2],[1017, 1],[1065, 4],[1064, 3],[1035, 2],[1006, 1],[1091, 2],[1093, 2],[1094, 1],[1090, 2],[1095, 2],[1099, 16],[1102, 12]]}";
        List<(int id, int count)> cards = new();
        JsonDecoder decoder = new(json);
        decoder.Get("cards", ref cards);
        Assert.AreEqual(16, cards.Count);
        Assert.AreEqual(1044, cards[0].id);
        Assert.AreEqual(4, cards[0].count);
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
    
    #region Card Decoding Tests
    
    [Test]
    public void TestPokemonCardDecode() {
        string json = "{\"id\":\"15149\",\"name\":\"Ho-oh\",\"supertype\":\"Pokemon\",\"subtypes\":[\"Basic\"],\"hp\":\"80\",\"types\":[\"Colorless\"],\"abilities\":[{\"name\":\"Crystal Type\",\"text\":\"Whenever you attach a Fire, Water, or Lightning basic Energy card from your hand to Ho-oh, Ho-oh's type (color) becomes the same as that type of Energy until the end of the turn.\",\"type\":\"Poke-Body\"}],\"attacks\":[{\"name\":\"Holy Flame\",\"cost\":[\"Fire\",\"Lightning\"],\"convertedEnergyCost\":2,\"damage\":\"20\",\"text\":\"\"},{\"name\":\"Scalding Steam\",\"cost\":[\"Fire\",\"Water\",\"Water\",\"Colorless\"],\"convertedEnergyCost\":4,\"damage\":\"40\",\"text\":\"Discard an Energy card attached to Ho-oh and flip a coin. If heads, the Defending Pokemon is now Burned.\"}],\"weaknesses\":[{\"type\":\"Water\",\"value\":\"x2\"}],\"retreatCost\":[\"Colorless\",\"Colorless\",\"Colorless\"],\"convertedRetreatCost\":3,\"number\":\"149\",\"rarity\":\"RareSecret\",\"images\":{\"small\":\"https://images.pokemontcg.io/ecard3/149.png\",\"large\":\"https://images.pokemontcg.io/ecard3/149_hires.png\"},\"setCode\":\"ecard3\"}";
        JsonDecoder decoder = new(json);
        PokemonProfile pokemonProfile = new();
        pokemonProfile.Decode(decoder);
        
        Assert.AreEqual(15149, pokemonProfile.Id);
        Assert.AreEqual("Ho-oh", pokemonProfile.Name);
        Assert.AreEqual("ecard3", pokemonProfile.SetCode);
        Assert.AreEqual(SuperType.Pokemon, pokemonProfile.SuperType);
        Assert.AreEqual(1, pokemonProfile.SubTypes.Count);
        Assert.AreEqual(SubType.Basic, pokemonProfile.SubTypes[0]);
        Assert.AreEqual(80, pokemonProfile.Hp);
        Assert.AreEqual(1, pokemonProfile.Types.Count);
        Assert.AreEqual(EnergyType.Colorless, pokemonProfile.Types[0]);
        Assert.AreEqual(1, pokemonProfile.Abilities.Count);
        Assert.AreEqual("Crystal Type", pokemonProfile.Abilities[0]["name"]);
        Assert.AreEqual("Whenever you attach a Fire, Water, or Lightning basic Energy card from your hand to Ho-oh, Ho-oh's type (color) becomes the same as that type of Energy until the end of the turn.", pokemonProfile.Abilities[0]["text"]);
        Assert.AreEqual(2, pokemonProfile.Attacks.Count);
        Assert.AreEqual("Holy Flame", pokemonProfile.Attacks[0]["name"]);
        Assert.AreEqual("2", pokemonProfile.Attacks[0]["convertedEnergyCost"]);
        Assert.AreEqual("20", pokemonProfile.Attacks[0]["damage"]);
        Assert.AreEqual("Scalding Steam", pokemonProfile.Attacks[1]["name"]);
        Assert.AreEqual("4", pokemonProfile.Attacks[1]["convertedEnergyCost"]);
        Assert.AreEqual("40", pokemonProfile.Attacks[1]["damage"]);
        Assert.AreEqual(1, pokemonProfile.Weaknesses.Count);
        Assert.AreEqual("Water", pokemonProfile.Weaknesses[0]["type"]);
        Assert.AreEqual("x2", pokemonProfile.Weaknesses[0]["value"]);
        Assert.AreEqual(3, pokemonProfile.RetreatCost.Count);
        Assert.AreEqual(EnergyType.Colorless, pokemonProfile.RetreatCost[0]);
        Assert.AreEqual(EnergyType.Colorless, pokemonProfile.RetreatCost[1]);
        Assert.AreEqual(EnergyType.Colorless, pokemonProfile.RetreatCost[2]);
        Assert.AreEqual(3, pokemonProfile.ConvertedRetreatCost);
        Assert.AreEqual(Rarity.RareSecret, pokemonProfile.Rarity);
        Assert.AreEqual("https://images.pokemontcg.io/ecard3/149.png", pokemonProfile.Images["small"]);
        Assert.AreEqual("https://images.pokemontcg.io/ecard3/149_hires.png", pokemonProfile.Images["large"]);
    }
    
    [Test]
    public void TestTrainerCardDecode() {
        string json = "{\"id\":\"15120\",\"name\":\"Relic Hunter\",\"supertype\":\"Trainer\",\"subtypes\":[\"Supporter\"],\"rules\":\"Search your deck for up to 2 Supporter and/or Stadium cards, show them to your opponent, and put them into your hand. Shuffle your deck afterward.\",\"number\":\"120\",\"rarity\":\"Uncommon\",\"images\":{\"small\":\"https://images.pokemontcg.io/ecard3/120.png\",\"large\":\"https://images.pokemontcg.io/ecard3/120_hires.png\"},\"setCode\":\"ecard3\"}";
        JsonDecoder decoder = new(json);
        TrainerProfile trainerProfile = new();
        trainerProfile.Decode(decoder);
        
        Assert.AreEqual(15120, trainerProfile.Id);
        Assert.AreEqual("Relic Hunter", trainerProfile.Name);
        Assert.AreEqual("ecard3", trainerProfile.SetCode);
        Assert.AreEqual(SuperType.Trainer, trainerProfile.SuperType);
        Assert.AreEqual(1, trainerProfile.SubTypes.Count);
        Assert.AreEqual(SubType.Supporter, trainerProfile.SubTypes[0]);
        Assert.AreEqual("Search your deck for up to 2 Supporter and/or Stadium cards, show them to your opponent, and put them into your hand. Shuffle your deck afterward.", trainerProfile.Rules);
        Assert.AreEqual(Rarity.Uncommon, trainerProfile.Rarity);
        Assert.AreEqual("https://images.pokemontcg.io/ecard3/120.png", trainerProfile.Images["small"]);
        Assert.AreEqual("https://images.pokemontcg.io/ecard3/120_hires.png", trainerProfile.Images["large"]);
        // TODO: Check after abilities have been implemented
    }

    [Test]
    public void TestSetJsonDecode() {
        string json = "{\"id\":\"1000\",\"name\":\"Base\",\"setCode\":\"base1\",\"series\":\"Base\",\"totalCards\":102,\"totalDecks\":5,\"images\":{\"symbol\":\"https://images.pokemontcg.io/base1/symbol.png\",\"logo\":\"https://images.pokemontcg.io/base1/logo.png\"}}";
        JsonDecoder decoder = new(json);
        SetProfile setProfile = new();
        setProfile.Decode(decoder);
        
        Assert.AreEqual(1000, setProfile.Id);
        Assert.AreEqual("Base", setProfile.Name);
        Assert.AreEqual("base1", setProfile.SetCode);
        Assert.AreEqual(SetSeries.Base, setProfile.Series);
        Assert.AreEqual(102, setProfile.TotalCards);
        Assert.AreEqual(5, setProfile.TotalDecks);
        Assert.AreEqual("https://images.pokemontcg.io/base1/symbol.png", setProfile.Images["symbol"]);
        Assert.AreEqual("https://images.pokemontcg.io/base1/logo.png", setProfile.Images["logo"]);
    }

    #endregion
}