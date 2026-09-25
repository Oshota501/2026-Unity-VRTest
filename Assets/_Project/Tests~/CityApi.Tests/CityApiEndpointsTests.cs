using System;
using NUnit.Framework;
using TownReview.Core.City;
using TownReview.Infrastructure;

namespace TownReview.Tests
{
    public class CityApiEndpointsTests
    {
        [Test]
        public void 仕様書の例と同じURLになる()
        {
            Assert.That(
                CityApiEndpoints.BuildSnapshotUrl("https://api.example.com", "sample", CityMode.Now),
                Is.EqualTo("https://api.example.com/v1/cities/sample/snapshot?mode=now"));
        }

        [Test]
        public void idealモードのURL()
        {
            Assert.That(
                CityApiEndpoints.BuildSnapshotUrl("https://api.example.com", "sample", CityMode.Ideal),
                Is.EqualTo("https://api.example.com/v1/cities/sample/snapshot?mode=ideal"));
        }

        [Test]
        public void ベースURL末尾のスラッシュと空白は取り除く()
        {
            Assert.That(
                CityApiEndpoints.BuildSnapshotUrl(" http://localhost:8080/ ", "sample", CityMode.Now),
                Is.EqualTo("http://localhost:8080/v1/cities/sample/snapshot?mode=now"));
        }

        [Test]
        public void 都市IDに記号や日本語があってもエスケープする()
        {
            Assert.That(
                CityApiEndpoints.BuildSnapshotUrl("https://api.example.com", "a b/c", CityMode.Now),
                Is.EqualTo("https://api.example.com/v1/cities/a%20b%2Fc/snapshot?mode=now"));
            Assert.That(
                CityApiEndpoints.BuildSnapshotUrl("https://api.example.com", "大阪", CityMode.Now),
                Is.EqualTo("https://api.example.com/v1/cities/%E5%A4%A7%E9%98%AA/snapshot?mode=now"));
        }

        [TestCase(null, "sample")]
        [TestCase("", "sample")]
        [TestCase("https://api.example.com", "")]
        [TestCase("https://api.example.com", "  ")]
        public void 空の値は例外になる(string baseUrl, string cityId)
        {
            Assert.Throws<ArgumentException>(() => CityApiEndpoints.BuildSnapshotUrl(baseUrl, cityId, CityMode.Now));
        }
    }
}
