using NUnit.Framework;
using TownReview.Core.City;
using TownReview.UI;
using TextMode = TownReview.UI.PersonCommentFormatter.TextMode;

namespace TownReview.Tests
{
    public class PersonCommentFormatterTests
    {
        private static AvatarData Avatar(string voice, string comment)
        {
            return new AvatarData { Voice = voice, Comment = comment };
        }

        [Test]
        public void Voiceモードは一言だけ()
        {
            Assert.That(PersonCommentFormatter.Build(Avatar("短い一言", "全文"), TextMode.Voice, 0), Is.EqualTo("短い一言"));
        }

        [Test]
        public void Commentモードは全文だけ()
        {
            Assert.That(PersonCommentFormatter.Build(Avatar("短い一言", "全文"), TextMode.Comment, 0), Is.EqualTo("全文"));
        }

        [Test]
        public void 両方のモードは一言の下に全文()
        {
            Assert.That(
                PersonCommentFormatter.Build(Avatar("短い一言", "全文"), TextMode.VoiceAndComment, 0),
                Is.EqualTo("「短い一言」\n全文"));
        }

        [Test]
        public void 片方が空ならもう片方で代用する()
        {
            Assert.That(PersonCommentFormatter.Build(Avatar("", "全文"), TextMode.Voice, 0), Is.EqualTo("全文"));
            Assert.That(PersonCommentFormatter.Build(Avatar("一言", ""), TextMode.Comment, 0), Is.EqualTo("一言"));
            Assert.That(PersonCommentFormatter.Build(Avatar("", "全文"), TextMode.VoiceAndComment, 0), Is.EqualTo("全文"));
            Assert.That(PersonCommentFormatter.Build(Avatar("", ""), TextMode.VoiceAndComment, 0), Is.EqualTo(""));
        }

        [Test]
        public void nullのアバターは空文字()
        {
            Assert.That(PersonCommentFormatter.Build(null, TextMode.Voice, 10), Is.EqualTo(""));
        }

        [Test]
        public void 指定文字数ごとに改行する()
        {
            Assert.That(PersonCommentFormatter.Wrap("あいうえおかきくけこさ", 5), Is.EqualTo("あいうえお\nかきくけこ\nさ"));
        }

        [Test]
        public void ちょうど割り切れるときは末尾に改行を付けない()
        {
            Assert.That(PersonCommentFormatter.Wrap("あいうえお", 5), Is.EqualTo("あいうえお"));
        }

        [Test]
        public void 元からある改行で行の文字数を数え直す()
        {
            Assert.That(PersonCommentFormatter.Wrap("あい\nうえおかきく", 4), Is.EqualTo("あい\nうえおか\nきく"));
        }

        [Test]
        public void 文字数が0以下なら改行しない()
        {
            Assert.That(PersonCommentFormatter.Wrap("あいうえお", 0), Is.EqualTo("あいうえお"));
            Assert.That(PersonCommentFormatter.Wrap("あいうえお", -1), Is.EqualTo("あいうえお"));
        }

        [Test]
        public void 絵文字を途中で切らない()
        {
            // 😀 は2つのcharでできている（サロゲートペア）
            Assert.That(PersonCommentFormatter.Wrap("あ😀い", 1), Is.EqualTo("あ\n😀\nい"));
        }

        [Test]
        public void サンプルの住民のコメントを組み立てられる()
        {
            AvatarData a001 = CitySnapshotParser.Parse(TestData.NowJson).Avatars[0];

            string text = PersonCommentFormatter.Build(a001, TextMode.VoiceAndComment, 20);

            Assert.That(text, Does.StartWith("「夕方はレジが混む。朝は空いてる」\n夕方17〜19時はレジが混んでいて10分"));
            foreach (string line in text.Split('\n'))
            {
                Assert.That(line.Length, Is.LessThanOrEqualTo(20), line);
            }
        }
    }
}
