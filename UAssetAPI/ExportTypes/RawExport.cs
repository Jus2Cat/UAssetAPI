using System.Collections.Generic;
using System.IO;
using UAssetAPI.UnrealTypes;
using UAssetAPI.ExportTypes;

namespace UAssetAPI.ExportTypes
{
    /// <summary>
    /// An export that could not be properly parsed by UAssetAPI, and is instead represented as an array of bytes as a fallback.
    /// </summary>
    public class RawExport : Export
    {
        public byte[] Data;
        /// <summary>
        /// Why this export is raw: the message of the exception that stopped parsing it, when parsing was
        /// attempted and failed (null when the export was raw by request — SkipParsingExports, manual skips).
        /// A consumer can tell "not decoded" from "chose not to decode" and report the former.
        /// </summary>
        public string ParseError;
        /// <summary>
        /// The tagged properties that were parsed before the failure, as "Name : Type" — so a consumer can
        /// say where in the export parsing stopped, not only that it did. Null when nothing was parsed.
        /// </summary>
        public List<string> ParsedBeforeError;
        /// <summary>Byte offset of the failure inside this export's serialized data (position - SerialOffset).</summary>
        public long? ParseErrorOffset;

        public RawExport(Export super)
        {
            Asset = super.Asset;
            Extras = super.Extras;
        }

        public RawExport(byte[] data, UAsset asset, byte[] extras) : base(asset, extras)
        {
            Data = data;
        }

        public RawExport()
        {

        }

        public override void Write(AssetBinaryWriter writer)
        {
            writer.Write(Data);
        }
    }
}
