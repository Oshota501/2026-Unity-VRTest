using System.Text;
using TownReview.Core.City;

namespace TownReview.UI
{
    // 住民の上に出す文字（voice / comment）を組み立てる。
    // SampleScene の Person はコメントを折り返さずに1行で表示するため、ここで一定の文字数ごとに改行を入れる。
    // UnityEngine を参照しない（Tests~ のテストから確認するため）。
    public static class PersonCommentFormatter
    {
        public enum TextMode
        {
            Voice,           // 短い一言だけ
            Comment,         // コメント全文だけ
            VoiceAndComment  // 1行目に一言、その下にコメント全文
        }

        public static string Build(AvatarData avatar, TextMode mode, int maxCharsPerLine)
        {
            if (avatar == null)
            {
                return "";
            }

            string voice = avatar.Voice ?? "";
            string comment = avatar.Comment ?? "";

            string text;
            switch (mode)
            {
                case TextMode.Voice:
                    // voice が空ならコメントで代用する
                    text = voice != "" ? voice : comment;
                    break;
                case TextMode.Comment:
                    text = comment != "" ? comment : voice;
                    break;
                default:
                    if (voice == "" || comment == "")
                    {
                        text = voice + comment;
                    }
                    else
                    {
                        text = $"「{voice}」\n{comment}";
                    }
                    break;
            }

            return Wrap(text, maxCharsPerLine);
        }

        // maxCharsPerLine 文字ごとに改行を入れる。元からある改行はそのまま使う。
        // 0以下なら改行を入れない。絵文字など2文字で1つの文字（サロゲートペア）は途中で切らない。
        public static string Wrap(string text, int maxCharsPerLine)
        {
            if (string.IsNullOrEmpty(text))
            {
                return "";
            }

            if (maxCharsPerLine <= 0)
            {
                return text;
            }

            var builder = new StringBuilder(text.Length + text.Length / maxCharsPerLine);
            int lineLength = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '\n')
                {
                    builder.Append(c);
                    lineLength = 0;
                    continue;
                }

                if (lineLength >= maxCharsPerLine)
                {
                    builder.Append('\n');
                    lineLength = 0;
                }

                builder.Append(c);
                if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    builder.Append(text[i + 1]);
                    i++;
                }
                lineLength++;
            }

            return builder.ToString();
        }
    }
}
