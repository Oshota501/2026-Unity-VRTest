using NUnit.Framework;
using TownReview.Core.City;

namespace TownReview.Tests
{
    public class CitySnapshotParserTests
    {
        [Test]
        public void サンプルJSONのルート項目を読み込める()
        {
            CitySnapshot city = CitySnapshotParser.Parse(TestData.NowJson);

            Assert.That(city.CityId, Is.EqualTo("sample"));
            Assert.That(city.CityName, Is.EqualTo("サンプル市"));
            Assert.That(city.Mode, Is.EqualTo("now"));
            Assert.That(city.ModeType, Is.EqualTo(CityMode.Now));
            Assert.That(city.Buildings, Has.Count.EqualTo(6));
            Assert.That(city.Avatars, Has.Count.EqualTo(5));
        }

        [Test]
        public void generatedAtは日付に変換されず元の文字列のまま()
        {
            CitySnapshot city = CitySnapshotParser.Parse(TestData.NowJson);

            Assert.That(city.GeneratedAt, Is.EqualTo("2026-09-25T09:00:00Z"));
        }

        [Test]
        public void boundsを読み込める()
        {
            CitySnapshot city = CitySnapshotParser.Parse(TestData.NowJson);

            AssertVector(city.Bounds.Min, -200f, 0f, -200f);
            AssertVector(city.Bounds.Max, 200f, 0f, 200f);
        }

        [Test]
        public void 建物の全項目を読み込める()
        {
            BuildingData b = CitySnapshotParser.Parse(TestData.NowJson).Buildings[0];

            Assert.That(b.Id, Is.EqualTo("b001"));
            Assert.That(b.Name, Is.EqualTo("サンプルマート"));
            Assert.That(b.Category, Is.EqualTo("supermarket"));
            Assert.That(b.CategoryType, Is.EqualTo(BuildingCategory.Supermarket));
            AssertVector(b.Position, 20f, 0f, 40f);
            Assert.That(b.RotationY, Is.EqualTo(90f));
            AssertVector(b.Size, 20f, 6f, 15f);
        }

        [Test]
        public void 名前のない建物はnameが空文字()
        {
            BuildingData apartment = CitySnapshotParser.Parse(TestData.NowJson).Buildings[3];

            Assert.That(apartment.Name, Is.EqualTo(""));
            Assert.That(apartment.CategoryType, Is.EqualTo(BuildingCategory.Apartment));
        }

        [Test]
        public void アバターの全項目を読み込める()
        {
            AvatarData a = CitySnapshotParser.Parse(TestData.NowJson).Avatars[0];

            Assert.That(a.Id, Is.EqualTo("a001"));
            Assert.That(a.BuildingId, Is.EqualTo("b001"));
            AssertVector(a.Position, 22f, 0f, 30f);
            Assert.That(a.RotationY, Is.EqualTo(180f));
            Assert.That(a.Appearance.BodyType, Is.EqualTo("type_b"));
            Assert.That(a.Appearance.HairStyle, Is.EqualTo("short_02"));
            Assert.That(a.Appearance.HairColor, Is.EqualTo("#3B2A1A"));
            Assert.That(a.Appearance.Outfit, Is.EqualTo("casual_05"));
            Assert.That(a.EmotionType, Is.EqualTo(ResidentEmotion.Neutral));
            Assert.That(a.Voice, Is.EqualTo("夕方はレジが混む。朝は空いてる"));
            Assert.That(a.Comment, Does.StartWith("夕方17〜19時はレジが混んでいて"));
        }

        [Test]
        public void 建物に紐づかないアバターはbuildingIdが空文字()
        {
            AvatarData a = CitySnapshotParser.Parse(TestData.NowJson).Avatars[4];

            Assert.That(a.Id, Is.EqualTo("a005"));
            Assert.That(a.BuildingId, Is.EqualTo(""));
            Assert.That(a.EmotionType, Is.EqualTo(ResidentEmotion.Worried));
        }

        [Test]
        public void idealモードのサンプルを読み込める()
        {
            CitySnapshot city = CitySnapshotParser.Parse(TestData.IdealJson);

            Assert.That(city.ModeType, Is.EqualTo(CityMode.Ideal));
            Assert.That(city.Buildings, Has.Count.EqualTo(7));
            Assert.That(city.Avatars, Has.Count.EqualTo(6));
        }

        [Test]
        public void nullが来ても既定値に置き換わる()
        {
            const string json = @"{
                ""cityId"": null, ""cityName"": null, ""mode"": null, ""generatedAt"": null,
                ""bounds"": { ""min"": null },
                ""buildings"": [ null, { ""id"": ""b1"", ""name"": null, ""category"": null, ""position"": null, ""size"": null } ],
                ""avatars"": [ { ""id"": ""a1"", ""buildingId"": null, ""position"": null, ""appearance"": null,
                                 ""emotion"": null, ""voice"": null, ""comment"": null } ]
            }";

            CitySnapshot city = CitySnapshotParser.Parse(json);

            Assert.That(city.CityId, Is.EqualTo(""));
            Assert.That(city.GeneratedAt, Is.EqualTo(""));
            Assert.That(city.Bounds.Min, Is.Not.Null);
            Assert.That(city.Bounds.Max, Is.Not.Null);

            Assert.That(city.Buildings, Has.Count.EqualTo(1), "リスト内の null は取り除かれる");
            BuildingData b = city.Buildings[0];
            Assert.That(b.Name, Is.EqualTo(""));
            Assert.That(b.CategoryType, Is.EqualTo(BuildingCategory.Other));
            AssertVector(b.Position, 0f, 0f, 0f);
            AssertVector(b.Size, 0f, 0f, 0f);

            AvatarData a = city.Avatars[0];
            Assert.That(a.BuildingId, Is.EqualTo(""));
            AssertVector(a.Position, 0f, 0f, 0f);
            Assert.That(a.Appearance, Is.Not.Null);
            Assert.That(a.Appearance.HairColor, Is.EqualTo(""));
            Assert.That(a.EmotionType, Is.EqualTo(ResidentEmotion.Neutral));
            Assert.That(a.Voice, Is.EqualTo(""));
            Assert.That(a.Comment, Is.EqualTo(""));
        }

        [Test]
        public void 項目がない場合も既定値になる()
        {
            CitySnapshot city = CitySnapshotParser.Parse(@"{ ""cityId"": ""x"" }");

            Assert.That(city.Buildings, Is.Empty);
            Assert.That(city.Avatars, Is.Empty);
            Assert.That(city.Mode, Is.EqualTo(""));
        }

        [Test]
        public void buildingsやavatarsがnullなら空リストになる()
        {
            CitySnapshot city = CitySnapshotParser.Parse(@"{ ""buildings"": null, ""avatars"": null }");

            Assert.That(city.Buildings, Is.Not.Null.And.Empty);
            Assert.That(city.Avatars, Is.Not.Null.And.Empty);
        }

        [Test]
        public void 将来追加されるフィールドがあっても読み込める()
        {
            const string json = @"{
                ""cityId"": ""sample"", ""mode"": ""now"",
                ""effects"": [ { ""type"": ""noise"", ""level"": 3 } ],
                ""buildings"": [ { ""id"": ""b1"", ""category"": ""house"", ""floors"": 2 } ],
                ""avatars"": []
            }";

            CitySnapshot city = CitySnapshotParser.Parse(json);

            Assert.That(city.Buildings[0].CategoryType, Is.EqualTo(BuildingCategory.House));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        [TestCase("null")]
        [TestCase("{ this is not json")]
        [TestCase("[1, 2, 3]")]
        [TestCase("<html>502 Bad Gateway</html>")]
        public void 読めないJSONは例外になる(string json)
        {
            Assert.Throws<CitySnapshotFormatException>(() => CitySnapshotParser.Parse(json));
        }

        [Test]
        public void TryParseは失敗時にfalseとメッセージを返す()
        {
            bool ok = CitySnapshotParser.TryParse("{ broken", out CitySnapshot city, out string error);

            Assert.That(ok, Is.False);
            Assert.That(city, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void 数値の型が違うと例外になる()
        {
            Assert.Throws<CitySnapshotFormatException>(
                () => CitySnapshotParser.Parse(@"{ ""buildings"": [ { ""rotationY"": ""ninety"" } ] }"));
        }

        [Test]
        public void エラーレスポンス404を読み込める()
        {
            bool ok = CitySnapshotParser.TryParseError(
                @"{ ""error"": { ""code"": ""NOT_FOUND"", ""message"": ""city not found"" } }",
                out CityApiError error);

            Assert.That(ok, Is.True);
            Assert.That(error.Code, Is.EqualTo("NOT_FOUND"));
            Assert.That(error.Message, Is.EqualTo("city not found"));
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("{ broken")]
        [TestCase(@"{ ""cityId"": ""sample"" }")]
        [TestCase("Bad Request")]
        public void エラー形式でないものはTryParseErrorがfalse(string json)
        {
            Assert.That(CitySnapshotParser.TryParseError(json, out _), Is.False);
        }

        private static void AssertVector(CityVector3 v, float x, float y, float z)
        {
            Assert.That(v, Is.Not.Null);
            Assert.That(v.X, Is.EqualTo(x), "x");
            Assert.That(v.Y, Is.EqualTo(y), "y");
            Assert.That(v.Z, Is.EqualTo(z), "z");
        }
    }
}
