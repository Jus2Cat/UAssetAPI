using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.PropertyTypes.Structs;

/// <summary>
/// A value with per-platform overrides (TPerPlatformProperty), as the engine writes it
/// (Engine/Source/Runtime/CoreUObject/Public/UObject/PerPlatformPropertiesImpl.inl, operator&lt;&lt;; the same layout in
/// 5.7, where the flag is called bCooked, and in 5.8, bStripSerializeAllProperties):
///   bool (4 bytes) — the overrides were stripped (a cooked package);
///   the default value;
///   unless stripped: TMap&lt;FName, value&gt; — the overrides an editor package keeps (count, then name and value pairs).
/// Value[0] is the default. Upstream read the flag as an element count and stopped there: an editor package's
/// default and overrides were never read, and the property ended 8 bytes short of its tag (the reader's size check
/// found 80 of them in r7game, all ScreenSize and PlatformTargetFrameRate).
/// </summary>
public abstract class TPerPlatformPropertyData<T> : PropertyData<T[]>
{
    /// <summary>The overrides were stripped (a cooked package): only the value for the target platform is in the file.</summary>
    [JsonProperty]
    public bool Stripped;

    /// <summary>The per-platform overrides of an editor package: platform (or platform group) name → value.</summary>
    [JsonProperty]
    public List<KeyValuePair<FName, T>> PerPlatform = new List<KeyValuePair<FName, T>>();

    public TPerPlatformPropertyData(FName name) : base(name)
    {
        Value = [];
    }

    public TPerPlatformPropertyData()
    {
        Value = [];
    }

    public override bool HasCustomStructSerialization => true;

    protected abstract T ReadValue(AssetBinaryReader reader);

    /// <returns>The value written as text, or default when it does not parse.</returns>
    protected abstract T ParseValue(string text);

    /// <summary>The default from the first field; overrides are not given as text.</summary>
    public override void FromString(string[] d, UAsset asset)
    {
        Value = [ParseValue(d.Length > 0 ? d[0] : null)];
        PerPlatform = new List<KeyValuePair<FName, T>>();
    }

    /// <returns>Bytes written.</returns>
    protected abstract int WriteValue(AssetBinaryWriter writer, T value);

    public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0, PropertySerializationContext serializationContext = PropertySerializationContext.Normal)
    {
        if (includeHeader)
        {
            this.ReadEndPropertyTag(reader);
        }

        Stripped = reader.ReadInt32() != 0;
        Value = [ReadValue(reader)];
        PerPlatform = new List<KeyValuePair<FName, T>>();
        if (!Stripped)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                FName platform = reader.ReadFName();
                PerPlatform.Add(new KeyValuePair<FName, T>(platform, ReadValue(reader)));
            }
        }
    }

    public override int Write(AssetBinaryWriter writer, bool includeHeader, PropertySerializationContext serializationContext = PropertySerializationContext.Normal)
    {
        if (includeHeader)
        {
            this.WriteEndPropertyTag(writer);
        }

        writer.Write(Stripped ? 1 : 0);
        int size = sizeof(int);
        size += WriteValue(writer, Value.Length > 0 ? Value[0] : default);
        if (!Stripped)
        {
            writer.Write(PerPlatform.Count);
            size += sizeof(int);
            foreach (var entry in PerPlatform)
            {
                writer.Write(entry.Key);
                size += sizeof(int) * 2;
                size += WriteValue(writer, entry.Value);
            }
        }
        return size;
    }

    public override string ToString()
    {
        if (Value.Length == 0) return "()";
        string oup = "(" + Convert.ToString(Value[0]);
        foreach (var entry in PerPlatform) oup += ", " + entry.Key + "=" + Convert.ToString(entry.Value);
        return oup + ")";
    }
}

/// <summary>
/// <see cref="BoolPropertyData"/> (<see cref="bool"/>) property with per-platform overrides.
/// </summary>
public class PerPlatformBoolPropertyData : TPerPlatformPropertyData<bool>
{
    public PerPlatformBoolPropertyData(FName name) : base(name) { }

    public PerPlatformBoolPropertyData() { }

    private static readonly FString CurrentPropertyType = new FString("PerPlatformBool");
    public override FString PropertyType => CurrentPropertyType;

    // FArchive writes a bool as 4 bytes
    protected override bool ReadValue(AssetBinaryReader reader) => reader.ReadInt32() != 0;

    protected override int WriteValue(AssetBinaryWriter writer, bool value)
    {
        writer.WriteBooleanInt(value);
        return sizeof(int);
    }

    protected override bool ParseValue(string text) => bool.TryParse(text, out bool value) && value;
}

/// <summary>
/// <see cref="FloatPropertyData"/> (<see cref="float"/>) property with per-platform overrides.
/// </summary>
public class PerPlatformFloatPropertyData : TPerPlatformPropertyData<float>
{
    public PerPlatformFloatPropertyData(FName name) : base(name) { }

    public PerPlatformFloatPropertyData() { }

    private static readonly FString CurrentPropertyType = new FString("PerPlatformFloat");
    public override FString PropertyType => CurrentPropertyType;

    protected override float ReadValue(AssetBinaryReader reader) => reader.ReadSingle();

    protected override int WriteValue(AssetBinaryWriter writer, float value)
    {
        writer.Write(value);
        return sizeof(float);
    }

    protected override float ParseValue(string text) => float.TryParse(text, out float value) ? value : 0;
}

/// <summary>
/// <see cref="IntPropertyData"/> (<see cref="int"/>) property with per-platform overrides.
/// </summary>
public class PerPlatformIntPropertyData : TPerPlatformPropertyData<int>
{
    public PerPlatformIntPropertyData(FName name) : base(name) { }

    public PerPlatformIntPropertyData() { }

    private static readonly FString CurrentPropertyType = new FString("PerPlatformInt");
    public override FString PropertyType => CurrentPropertyType;

    protected override int ReadValue(AssetBinaryReader reader) => reader.ReadInt32();

    protected override int WriteValue(AssetBinaryWriter writer, int value)
    {
        writer.Write(value);
        return sizeof(int);
    }

    protected override int ParseValue(string text) => int.TryParse(text, out int value) ? value : 0;
}

/// <summary>
/// <see cref="PerPlatformFrameRatePropertyData"/> (<see cref="FFrameRate"/>) property with per-platform overrides.
/// </summary>
public class PerPlatformFrameRatePropertyData : TPerPlatformPropertyData<FFrameRate>
{
    public PerPlatformFrameRatePropertyData(FName name) : base(name) { }

    public PerPlatformFrameRatePropertyData() { }

    private static readonly FString CurrentPropertyType = new FString("PerPlatformFrameRate");
    public override FString PropertyType => CurrentPropertyType;

    protected override FFrameRate ReadValue(AssetBinaryReader reader) => new FFrameRate(reader.ReadInt32(), reader.ReadInt32());

    protected override int WriteValue(AssetBinaryWriter writer, FFrameRate value)
    {
        writer.Write(value.Numerator);
        writer.Write(value.Denominator);
        return sizeof(int) * 2;
    }

    protected override FFrameRate ParseValue(string text) => FFrameRate.TryParse(text, out FFrameRate value) ? value : default;
}
