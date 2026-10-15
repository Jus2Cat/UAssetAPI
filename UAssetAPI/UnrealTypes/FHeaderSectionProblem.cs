using Newtonsoft.Json;

namespace UAssetAPI.UnrealTypes
{
    /// <summary>
    /// A section of the package summary that could not be read: which, where it starts, why. Optional sections (the
    /// localization texts, the asset registry data, thumbnails, ...) are read each within its own bounds, and one that
    /// fails is said here instead of losing the whole package; the rest of the package is read as usual.
    /// </summary>
    public class FHeaderSectionProblem
    {
        [JsonProperty] public string Section { get; set; }
        [JsonProperty] public long Offset { get; set; }
        [JsonProperty] public string Reason { get; set; }
    }
}
