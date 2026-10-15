using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.JSON
{
    /// <summary>
    /// Writes an FPackageIndex as the full path of the object it points at ("import:/Script/Engine.Default__Actor",
    /// "export:PersistentLevel.Actor_1.Root"), not as its index. An index is a position in this save's import or export
    /// table: one added name, import or export shifts it, so the same object reads as another number in the next save.
    /// Write-only: what it writes is for comparing two saves, not for reading back.
    /// </summary>
    internal class FPackageIndexPathJsonConverter : JsonConverter
    {
        private readonly UAsset asset;
        private readonly Dictionary<int, string> paths = new Dictionary<int, string>();

        public FPackageIndexPathJsonConverter(UAsset asset)
        {
            this.asset = asset;
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(FPackageIndex);

        public override bool CanRead => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var index = value as FPackageIndex;
            if (index == null || index.Index == 0) writer.WriteNull();
            else writer.WriteValue(PathOf(index));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException("An object path is written for comparison and is not read back");
        }

        private string PathOf(FPackageIndex index)
        {
            lock (paths)
                if (paths.TryGetValue(index.Index, out var known)) return known;
            var parts = new List<string>();
            var current = index;
            // an outer chain longer than the table is a loop in a damaged file: the index itself names it then
            for (var guard = 0; current != null && current.Index != 0 && guard <= asset.Imports.Count + asset.Exports.Count; guard++)
            {
                if (current.IsImport())
                {
                    if (-current.Index - 1 >= asset.Imports.Count) { parts.Insert(0, $"#{current.Index}"); break; }
                    var import = current.ToImport(asset);
                    parts.Insert(0, import.ObjectName?.ToString());
                    current = import.OuterIndex;
                }
                else
                {
                    if (current.Index - 1 >= asset.Exports.Count) { parts.Insert(0, $"#{current.Index}"); break; }
                    var export = current.ToExport(asset);
                    parts.Insert(0, export.ObjectName?.ToString());
                    current = export.OuterIndex;
                }
            }
            var path = (index.IsImport() ? "import:" : "export:") + string.Join(".", parts);
            lock (paths) paths[index.Index] = path;
            return path;
        }
    }
}

namespace UAssetAPI.JSON
{
    /// <summary>The contract resolver of UAsset's JSON with FPackageIndex written as a path (the type's own attribute writes the index).</summary>
    internal class PathContractResolver : UAssetContractResolver
    {
        private readonly FPackageIndexPathJsonConverter paths;

        public PathContractResolver(FPackageIndexPathJsonConverter paths) : base(null)
        {
            this.paths = paths;
        }

        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType == typeof(FPackageIndex)) return paths;
            return base.ResolveContractConverter(objectType);
        }
    }
}
