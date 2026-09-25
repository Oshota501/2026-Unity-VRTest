using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace TownReview.Infrastructure
{
    // ビルドの直前に自動で呼ばれ、.env から EnvSettings.BuildKeys の値だけを
    // Assets/_Project/Data/Generated/Resources/BakedEnv.txt に書き出す（このファイルがビルドに含まれる）。
    // 実機（Quest・スマホ・WebGL）では .env を読めないため。
    // 書き出したファイルは .gitignore で除外している（URLをGitに入れないため）。手で編集しないこと。
    public class EnvBuildProcessor : IPreprocessBuildWithReport
    {
        private const string OutputFolder = "Assets/_Project/Data/Generated/Resources";

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!File.Exists(EnvSettings.EnvFilePath))
            {
                Debug.LogWarning($"{nameof(EnvBuildProcessor)}: .env が見つかりません（{EnvSettings.EnvFilePath}）。ビルドしたアプリはAPIに接続できません。");
            }

            Dictionary<string, string> env = EnvSettings.ReadEnvFile();
            var content = new StringBuilder();
            content.Append("# ビルド時に .env から自動生成（EnvBuildProcessor）。手で編集しない。\n");
            foreach (string key in EnvSettings.BuildKeys)
            {
                if (env.TryGetValue(key, out string value) && value != "")
                {
                    content.Append(key).Append("=\"").Append(value).Append("\"\n");
                }
                else
                {
                    Debug.LogWarning($"{nameof(EnvBuildProcessor)}: .env に {key} がありません。ビルドしたアプリではこの値が空になります。");
                }
            }

            string outputPath = $"{OutputFolder}/{EnvSettings.BakedResourceName}.txt";
            string text = content.ToString();

            // 内容が変わらないときは書き込まない（不要な再インポートを避ける）
            if (File.Exists(outputPath) && File.ReadAllText(outputPath) == text)
            {
                return;
            }

            Directory.CreateDirectory(OutputFolder);
            File.WriteAllText(outputPath, text);
            AssetDatabase.Refresh();
        }
    }
}
