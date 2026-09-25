using NUnit.Framework;
using TownReview.Core.City;

namespace TownReview.Tests
{
    public class CitySnapshotValidatorTests
    {
        [Test]
        public void サンプルJSONには警告がない()
        {
            Assert.That(CitySnapshotValidator.Validate(CitySnapshotParser.Parse(TestData.NowJson)), Is.Empty);
            Assert.That(CitySnapshotValidator.Validate(CitySnapshotParser.Parse(TestData.IdealJson)), Is.Empty);
        }

        [Test]
        public void 仕様違反を警告する()
        {
            const string json = @"{
                ""cityId"": ""sample"", ""mode"": ""past"",
                ""bounds"": { ""min"": { ""x"": 10, ""y"": 0, ""z"": 0 }, ""max"": { ""x"": 0, ""y"": 0, ""z"": 0 } },
                ""buildings"": [
                    { ""id"": ""b1"", ""category"": ""castle"", ""size"": { ""x"": 1, ""y"": 1, ""z"": 1 } },
                    { ""id"": ""b1"", ""category"": ""house"",  ""size"": { ""x"": 1, ""y"": 0, ""z"": 1 } }
                ],
                ""avatars"": [
                    { ""id"": ""a1"", ""buildingId"": ""b999"", ""emotion"": ""angry"",
                      ""voice"": ""あいうえおかきくけこさしすせそたちつてとなにぬねのはひふへほまみむめもやゆよらりるれろ"" },
                    { ""id"": ""a1"", ""buildingId"": """", ""emotion"": ""happy"" }
                ]
            }";

            var warnings = CitySnapshotValidator.Validate(CitySnapshotParser.Parse(json));

            Assert.That(warnings, Has.Some.Contains("mode"));
            Assert.That(warnings, Has.Some.Contains("bounds"));
            Assert.That(warnings, Has.Some.Contains("建物ID \"b1\" が重複"));
            Assert.That(warnings, Has.Some.Contains("castle"));
            Assert.That(warnings, Has.Some.Contains("size"));
            Assert.That(warnings, Has.Some.Contains("b999"));
            Assert.That(warnings, Has.Some.Contains("angry"));
            Assert.That(warnings, Has.Some.Contains("40文字"));
            Assert.That(warnings, Has.Some.Contains("アバターID \"a1\" が重複"));
            Assert.That(warnings, Has.Count.EqualTo(9));
        }

        [Test]
        public void 空のbuildingIdは警告しない()
        {
            const string json = @"{ ""cityId"": ""s"", ""mode"": ""now"",
                ""avatars"": [ { ""id"": ""a1"", ""buildingId"": """", ""emotion"": ""happy"" } ] }";

            Assert.That(CitySnapshotValidator.Validate(CitySnapshotParser.Parse(json)), Is.Empty);
        }
    }
}
