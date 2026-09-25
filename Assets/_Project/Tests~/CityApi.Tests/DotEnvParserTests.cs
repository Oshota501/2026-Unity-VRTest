using NUnit.Framework;
using TownReview.Infrastructure;

namespace TownReview.Tests
{
    public class DotEnvParserTests
    {
        [Test]
        public void KEYとVALUEを読む()
        {
            var values = DotEnvParser.Parse("CITY_API_BASE_URL=https://api.example.com");

            Assert.That(values["CITY_API_BASE_URL"], Is.EqualTo("https://api.example.com"));
        }

        [Test]
        public void ポートやクエリ付きのURLもそのまま読む()
        {
            var values = DotEnvParser.Parse("A=http://localhost:8080/v1?x=1&y=2");

            Assert.That(values["A"], Is.EqualTo("http://localhost:8080/v1?x=1&y=2"));
        }

        [Test]
        public void コメント行と空行は無視する()
        {
            var values = DotEnvParser.Parse("# コメント\n\n  # 字下げしたコメント\nA=1\n");

            Assert.That(values.Count, Is.EqualTo(1));
            Assert.That(values["A"], Is.EqualTo("1"));
        }

        [Test]
        public void Windowsの改行でも読める()
        {
            var values = DotEnvParser.Parse("A=1\r\nB=2\r\n");

            Assert.That(values["A"], Is.EqualTo("1"));
            Assert.That(values["B"], Is.EqualTo("2"));
        }

        [Test]
        public void キーと値の前後の空白は取り除く()
        {
            var values = DotEnvParser.Parse("  A  =  https://api.example.com  ");

            Assert.That(values["A"], Is.EqualTo("https://api.example.com"));
        }

        [Test]
        public void 引用符で囲んだ値は引用符を外す()
        {
            var values = DotEnvParser.Parse("A=\"https://a.example.com\"\nB='https://b.example.com'");

            Assert.That(values["A"], Is.EqualTo("https://a.example.com"));
            Assert.That(values["B"], Is.EqualTo("https://b.example.com"));
        }

        [Test]
        public void 引用符の中のシャープは値の一部()
        {
            var values = DotEnvParser.Parse("A=\"abc # def\" # コメント");

            Assert.That(values["A"], Is.EqualTo("abc # def"));
        }

        [Test]
        public void 引用符なしの値は空白の後のシャープ以降をコメントとして捨てる()
        {
            var values = DotEnvParser.Parse("A=https://api.example.com # 本番\nB= # 空");

            Assert.That(values["A"], Is.EqualTo("https://api.example.com"));
            Assert.That(values["B"], Is.EqualTo(""));
        }

        [Test]
        public void 空白の後でないシャープは値の一部()
        {
            var values = DotEnvParser.Parse("A=https://example.com/#top");

            Assert.That(values["A"], Is.EqualTo("https://example.com/#top"));
        }

        [Test]
        public void 先頭のexportは無視する()
        {
            var values = DotEnvParser.Parse("export A=1");

            Assert.That(values["A"], Is.EqualTo("1"));
        }

        [Test]
        public void 同じキーは後の行が優先()
        {
            var values = DotEnvParser.Parse("A=1\nA=2");

            Assert.That(values["A"], Is.EqualTo("2"));
        }

        [Test]
        public void 値が空のキーは空文字になる()
        {
            var values = DotEnvParser.Parse("A=");

            Assert.That(values["A"], Is.EqualTo(""));
        }

        [Test]
        public void イコールがない行とキーが空の行は無視する()
        {
            var values = DotEnvParser.Parse("NOVALUE\n=abc\nA=1");

            Assert.That(values.Count, Is.EqualTo(1));
            Assert.That(values["A"], Is.EqualTo("1"));
        }

        [Test]
        public void 閉じる引用符がない値はそのまま()
        {
            var values = DotEnvParser.Parse("A=\"abc");

            Assert.That(values["A"], Is.EqualTo("\"abc"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void 空の入力は空の辞書(string text)
        {
            Assert.That(DotEnvParser.Parse(text), Is.Empty);
        }
    }
}
