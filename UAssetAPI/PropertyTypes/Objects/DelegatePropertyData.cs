using System;
using Newtonsoft.Json;
using UAssetAPI.CustomVersions;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.PropertyTypes.Objects;

/// <summary>
/// Describes a function bound to an Object.
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class FDelegate
{
    /// <summary>
    /// References the main actor export
    /// </summary>
    [JsonProperty]
    public FPackageIndex Object;
    /// <summary>
    /// The name of the delegate
    /// </summary>
    [JsonProperty]
    public FName Delegate;

    public FDelegate(FPackageIndex _object, FName @delegate)
    {
        Object = _object;
        Delegate = @delegate;
    }

    public FDelegate() { }

    /// <summary>
    /// UE 5.8 (ScriptDelegates.h, TScriptDelegate operator&lt;&lt;): at DynamicDelegatePayloads (262) a bool after the name
    /// says whether a payload follows; from CompactDynamicDelegatePayloads (263) the bool is gone and a payload is marked
    /// by this suffix on the function name instead — without one the layout is 5.7's.
    /// </summary>
    public const string PayloadSuffix = "[bHasPayload]";

    public FDelegate(AssetBinaryReader reader)
    {
        Object = new FPackageIndex(reader);
        Delegate = reader.ReadFName();
        var version = reader.Asset.GetCustomVersion<FFortniteMainBranchObjectVersion>();
        var hasPayload = false;
        if (version >= FFortniteMainBranchObjectVersion.DynamicDelegatePayloads && version < FFortniteMainBranchObjectVersion.CompactDynamicDelegatePayloads)
            hasPayload = reader.ReadBooleanInt(); // FArchive writes a bool as 4 bytes
        else if (version >= FFortniteMainBranchObjectVersion.CompactDynamicDelegatePayloads)
            hasPayload = Delegate?.ToString().EndsWith(PayloadSuffix) == true;
        // the payload's layout is the engine's FDynamicDelegatePropertyPayload, not in 5.8.3's sources (payloads are off
        // there, UE_USE_DYNAMIC_DELEGATE_PAYLOADS 0): a delegate with one is not readable — said, not skipped
        if (hasPayload) throw new FormatException($"Delegate {Delegate} carries a payload (UE 5.8 dynamic delegate payloads), whose layout is not known");
    }

    public int Write(AssetBinaryWriter writer)
    {
        writer.XFERPTR(Object);
        int size = sizeof(int);
        writer.Write(Delegate);
        size += 8;
        var version = writer.Asset.GetCustomVersion<FFortniteMainBranchObjectVersion>();
        if (version >= FFortniteMainBranchObjectVersion.DynamicDelegatePayloads && version < FFortniteMainBranchObjectVersion.CompactDynamicDelegatePayloads)
        {
            writer.Write(0); // "no payload": FArchive writes a bool as 4 bytes
            size += sizeof(int);
        }
        return size;
    }
}

/// <summary>
/// Describes a function bound to an Object.
/// </summary>
public class DelegatePropertyData : PropertyData<FDelegate>
{
    public DelegatePropertyData(FName name) : base(name) { }

    public DelegatePropertyData() { }

    private static readonly FString CurrentPropertyType = new FString("DelegateProperty");
    public override FString PropertyType => CurrentPropertyType;

    public override void Read(AssetBinaryReader reader, bool includeHeader, long leng1, long leng2 = 0, PropertySerializationContext serializationContext = PropertySerializationContext.Normal)
    {
        if (includeHeader)
        {
            this.ReadEndPropertyTag(reader);
        }

        Value = new FDelegate(reader);
    }

    public override int Write(AssetBinaryWriter writer, bool includeHeader, PropertySerializationContext serializationContext = PropertySerializationContext.Normal)
    {
        if (includeHeader)
        {
            this.WriteEndPropertyTag(writer);
        }

        Value ??= new FDelegate(FPackageIndex.FromRawIndex(0), null);
        return Value.Write(writer);
    }

    public override string ToString()
    {
        return null;
    }

    public override void FromString(string[] d, UAsset asset)
    {

    }

    protected override void HandleCloned(PropertyData res)
    {
        DelegatePropertyData cloningProperty = (DelegatePropertyData)res;

        cloningProperty.Value = new FDelegate(this.Value.Object, this.Value.Delegate);
    }
}
