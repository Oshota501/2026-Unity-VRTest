using System;
using System.Collections.Generic;

namespace TownReview.Infrastructure
{
    // .env 形式（1行に KEY=VALUE）の文字列を読み、キーと値の辞書にする。
    // UnityEngine を参照しない（Tests~ のテストから確認するため）。
    //
    // 対応している書き方：
    //   # コメント行、空行
    //   KEY=value              前後の空白は取り除く
    //   KEY="value" / 'value'  引用符で囲んだ値（中の # もそのまま値になる）
    //   KEY=value # コメント    引用符なしの値は「空白＋#」以降をコメントとして捨てる
    //   export KEY=value       先頭の export は無視する
    // 同じキーが複数あるときは後の行が優先。= がない行は無視する。
    public static class DotEnvParser
    {
        private const string ExportPrefix = "export ";

        public static Dictionary<string, string> Parse(string text)
        {
            var values = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrEmpty(text))
            {
                return values;
            }

            foreach (string rawLine in text.Split('\n'))
            {
                // Trim で Windows の改行（\r）も取り除かれる
                string line = rawLine.Trim();
                if (line == "" || line[0] == '#')
                {
                    continue;
                }

                if (line.StartsWith(ExportPrefix, StringComparison.Ordinal))
                {
                    line = line.Substring(ExportPrefix.Length).TrimStart();
                }

                int separator = line.IndexOf('=');
                if (separator <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, separator).Trim();
                values[key] = ParseValue(line.Substring(separator + 1));
            }

            return values;
        }

        private static string ParseValue(string rawValue)
        {
            string value = rawValue.Trim();
            if (value.Length > 0 && (value[0] == '"' || value[0] == '\''))
            {
                int end = value.IndexOf(value[0], 1);
                // 閉じる引用符がないときは、引用符も含めてそのまま値にする
                return end > 0 ? value.Substring(1, end - 1) : value;
            }

            // 「https://example.com/#top」のように空白の後でない # は値の一部として残す
            for (int i = 1; i < rawValue.Length; i++)
            {
                if (rawValue[i] == '#' && char.IsWhiteSpace(rawValue[i - 1]))
                {
                    return rawValue.Substring(0, i).Trim();
                }
            }

            return value;
        }
    }
}
