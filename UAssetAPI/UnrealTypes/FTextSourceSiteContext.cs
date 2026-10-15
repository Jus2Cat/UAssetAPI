using Newtonsoft.Json;

namespace UAssetAPI.UnrealTypes
{
    public struct FTextSourceSiteContext
    {
        [JsonProperty] public FString KeyName { get; set; }
        [JsonProperty] public FString SiteDescription { get; set; }
        [JsonProperty] public bool IsEditorOnly { get; set; }
        [JsonProperty] public bool IsOptional { get; set; }
        /// <summary>
        /// Translator notes of the text at this site (UE 5.8+): FTextSourceSiteContext::StreamStructuredArchive writes them
        /// after IsOptional when FFortniteMainBranchObjectVersion >= AddDevNotesToFText and the archive keeps editor data
        /// (GatherableTextData.cpp). Null for older packages.
        /// </summary>
        [JsonProperty] public FString DevNotes { get; set; }
        [JsonProperty] public FLocMetadataObject InfoMetaData { get; set; }
        [JsonProperty] public FLocMetadataObject KeyMetaData { get; set; }
    }
}
