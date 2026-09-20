using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using UAssetAPI.UnrealTypes;

namespace UAssetAPI.Tests
{
    [TestClass]
    public class TMapTests
    {
        [TestMethod]
        public void EnumerableConstructorUsesLastDuplicateValue()
        {
            var entries = new[]
            {
                new KeyValuePair<string, int>("duplicate", 1),
                new KeyValuePair<string, int>("other", 2),
                new KeyValuePair<string, int>("duplicate", 3),
            };

            var map = new TMap<string, int>(entries);

            Assert.AreEqual(2, map.Count);
            Assert.AreEqual(3, map["duplicate"]);
            Assert.AreEqual(2, map["other"]);
        }
    }
}
