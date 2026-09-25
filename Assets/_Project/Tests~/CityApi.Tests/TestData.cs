using System.IO;
using NUnit.Framework;

namespace TownReview.Tests
{
    internal static class TestData
    {
        public static string Read(string fileName)
        {
            return File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", fileName));
        }

        public static string NowJson => Read("SampleCitySnapshot_Now.json");
        public static string IdealJson => Read("SampleCitySnapshot_Ideal.json");
    }
}
