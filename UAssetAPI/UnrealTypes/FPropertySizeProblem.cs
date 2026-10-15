using Newtonsoft.Json;

namespace UAssetAPI.UnrealTypes
{
    /// <summary>
    /// A tagged property whose value this reader did not read as the engine wrote it: the tag declares how many bytes the
    /// value has, and the reader took another number (an engine added a field inside a native struct), or could not
    /// decode the struct at all and kept its bytes raw. The reader then goes on from the declared end, so the rest of the
    /// export is read right; this property's value may be wrong. Without this the next tag was read from the wrong
    /// place: an exception far from the cause, or a plausible wrong value with no word said.
    /// </summary>
    public class FPropertySizeProblem
    {
        /// <summary>The property, under the struct or class that holds it: "R7LootItemData.CountInterval".</summary>
        [JsonProperty] public string Property { get; set; }
        [JsonProperty] public string Type { get; set; }
        /// <summary>Bytes the tag declares.</summary>
        [JsonProperty] public long Declared { get; set; }
        /// <summary>Bytes the reader took (Declared when the struct was kept raw).</summary>
        [JsonProperty] public long Read { get; set; }
        /// <summary>Where the value starts, from the start of the export.</summary>
        [JsonProperty] public long Offset { get; set; }
        /// <summary>Why the value was kept raw, when it was; null for a size mismatch.</summary>
        [JsonProperty] public string Reason { get; set; }
    }
}
