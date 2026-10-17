using UAssetAPI.PropertyTypes.Objects;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.PropertyTypes.Structs;

/// <summary>
/// FGameplayEffectVersion (GameplayEffect.h): UGameplayEffect::DataVersion. Its one field is not a UPROPERTY, and
/// FGameplayEffectVersion::Serialize writes it alone (GameplayEffect.cpp: Slot &lt;&lt; CurrentVersion) — one byte of
/// EGameplayEffectVersion (uint8: Monolithic, Modular53, AbilitiesComponent53), no property tags. Read as tagged it took
/// that byte for a name index and every Gameplay Effect Blueprint reported a misread DataVersion.
/// </summary>
public struct FGameplayEffectVersion : IStruct<FGameplayEffectVersion>
{
    public byte CurrentVersion;

    public FGameplayEffectVersion(byte currentVersion)
    {
        CurrentVersion = currentVersion;
    }

    public FGameplayEffectVersion(AssetBinaryReader reader)
    {
        CurrentVersion = reader.ReadByte();
    }

    public static FGameplayEffectVersion Read(AssetBinaryReader reader) => new FGameplayEffectVersion(reader);

    public int Write(AssetBinaryWriter writer)
    {
        writer.Write(CurrentVersion);
        return sizeof(byte);
    }

    public override string ToString()
    {
        return CurrentVersion.ToString();
    }

    public static FGameplayEffectVersion FromString(string[] d, UAsset asset)
    {
        byte.TryParse(d[0], out byte res);
        return new FGameplayEffectVersion(res);
    }
}

public class GameplayEffectVersionPropertyData : BasePropertyData<FGameplayEffectVersion>
{
    public GameplayEffectVersionPropertyData(FName name) : base(name) { }

    public GameplayEffectVersionPropertyData() { }

    private static readonly FString CurrentPropertyType = new FString("GameplayEffectVersion");
    public override FString PropertyType => CurrentPropertyType;
}
