using NUnit.Framework;
using TownReview.Core.City;
using TownReview.Infrastructure;

namespace TownReview.Tests
{
    public class LocalCitySnapshotRepositoryTests
    {
        [Test]
        public void nowとidealでそれぞれのJSONを返す()
        {
            var repository = new LocalCitySnapshotRepository(TestData.NowJson, TestData.IdealJson);

            CitySnapshot now = Get(repository, CityMode.Now, out string nowError);
            CitySnapshot ideal = Get(repository, CityMode.Ideal, out string idealError);

            Assert.That(nowError, Is.Null);
            Assert.That(idealError, Is.Null);
            Assert.That(now.ModeType, Is.EqualTo(CityMode.Now));
            Assert.That(ideal.ModeType, Is.EqualTo(CityMode.Ideal));
        }

        [Test]
        public void JSONが設定されていないmodeはエラーを返す()
        {
            var repository = new LocalCitySnapshotRepository(TestData.NowJson);

            CitySnapshot ideal = Get(repository, CityMode.Ideal, out string error);

            Assert.That(ideal, Is.Null);
            Assert.That(error, Does.Contain("ideal"));
        }

        [Test]
        public void 壊れたJSONはエラーを返す()
        {
            var repository = new LocalCitySnapshotRepository("{ broken");

            CitySnapshot now = Get(repository, CityMode.Now, out string error);

            Assert.That(now, Is.Null);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void 呼び出すたびに別のオブジェクトを返す()
        {
            var repository = new LocalCitySnapshotRepository(TestData.NowJson);

            CitySnapshot first = Get(repository, CityMode.Now, out _);
            first.Buildings.Clear();
            CitySnapshot second = Get(repository, CityMode.Now, out _);

            Assert.That(second.Buildings, Has.Count.EqualTo(6));
        }

        private static CitySnapshot Get(ICitySnapshotRepository repository, CityMode mode, out string error)
        {
            CitySnapshot result = null;
            string errorMessage = null;
            repository.GetSnapshot("sample", mode, s => result = s, e => errorMessage = e);
            error = errorMessage;
            return result;
        }
    }
}
