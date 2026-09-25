using NUnit.Framework;
using TownReview.Core.City;

namespace TownReview.Tests
{
    public class CityEnumParserTests
    {
        [TestCase("house", BuildingCategory.House)]
        [TestCase("apartment", BuildingCategory.Apartment)]
        [TestCase("supermarket", BuildingCategory.Supermarket)]
        [TestCase("convenience_store", BuildingCategory.ConvenienceStore)]
        [TestCase("restaurant", BuildingCategory.Restaurant)]
        [TestCase("station", BuildingCategory.Station)]
        [TestCase("park", BuildingCategory.Park)]
        [TestCase("school", BuildingCategory.School)]
        [TestCase("hospital", BuildingCategory.Hospital)]
        [TestCase("other", BuildingCategory.Other)]
        public void 仕様書のcategoryをすべて変換できる(string value, BuildingCategory expected)
        {
            Assert.That(CityEnumParser.ParseCategory(value), Is.EqualTo(expected));
            Assert.That(CityEnumParser.IsKnownCategory(value), Is.True);
        }

        [TestCase("castle")]
        [TestCase("")]
        [TestCase(null)]
        public void 未知のcategoryはOtherになる(string value)
        {
            Assert.That(CityEnumParser.ParseCategory(value), Is.EqualTo(BuildingCategory.Other));
            Assert.That(CityEnumParser.IsKnownCategory(value), Is.False);
        }

        [Test]
        public void categoryは大文字や前後の空白を許容する()
        {
            Assert.That(CityEnumParser.ParseCategory(" Convenience_Store "), Is.EqualTo(BuildingCategory.ConvenienceStore));
        }

        [TestCase("happy", ResidentEmotion.Happy)]
        [TestCase("neutral", ResidentEmotion.Neutral)]
        [TestCase("annoyed", ResidentEmotion.Annoyed)]
        [TestCase("worried", ResidentEmotion.Worried)]
        [TestCase("angry", ResidentEmotion.Neutral)]
        [TestCase(null, ResidentEmotion.Neutral)]
        public void emotionを変換できる_未知はNeutral(string value, ResidentEmotion expected)
        {
            Assert.That(CityEnumParser.ParseEmotion(value), Is.EqualTo(expected));
        }

        [Test]
        public void modeとAPIの文字列を相互変換できる()
        {
            Assert.That(CityEnumParser.ToApiValue(CityMode.Now), Is.EqualTo("now"));
            Assert.That(CityEnumParser.ToApiValue(CityMode.Ideal), Is.EqualTo("ideal"));
            Assert.That(CityEnumParser.ParseMode("ideal"), Is.EqualTo(CityMode.Ideal));
            Assert.That(CityEnumParser.ParseMode("now"), Is.EqualTo(CityMode.Now));
            Assert.That(CityEnumParser.IsValidMode("past"), Is.False);
        }
    }
}
