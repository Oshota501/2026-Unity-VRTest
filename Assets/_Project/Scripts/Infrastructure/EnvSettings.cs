using System.Collections.Generic;
using UnityEngine;

namespace TownReview.Infrastructure
{
    // プロジェクト直下（Assets フォルダと同じ階層）の .env から設定値を読む。
    // APIのURLなど、Gitに入れたくない値をここから取り出す。
    //
    // ・エディタ：.env を毎回読み直す。.env を書き換えたら、再生し直す（または Reload）だけで反映される。
    // ・ビルドしたアプリ（Quest・スマホ・WebGL）：実機では .env を読めないため、ビルド直前に
    //   EnvBuildProcessor が BuildKeys の値だけを Resources に書き出し、それを読む。
    public static class EnvSettings
    {
        // 都市APIのベースURL（例：CITY_API_BASE_URL=https://api.example.com）
        public const string CityApiBaseUrlKey = "CITY_API_BASE_URL";

        // ビルドに含めるキー。.env のほかの値（秘密鍵など）はビルドに入れない。
        public static readonly string[] BuildKeys = { CityApiBaseUrlKey };

        // ビルド用に書き出すファイルの名前（Resources.Load で読む）
        public const string BakedResourceName = "BakedEnv";

#if !UNITY_EDITOR
        private static Dictionary<string, string> bakedValues;
#endif

        // 値がない、または空のときは "" を返す
        public static string Get(string key)
        {
            return GetValues().TryGetValue(key, out string value) ? value : "";
        }

        private static Dictionary<string, string> GetValues()
        {
#if UNITY_EDITOR
            return ReadEnvFile();
#else
            if (bakedValues == null)
            {
                var asset = Resources.Load<TextAsset>(BakedResourceName);
                bakedValues = asset != null ? DotEnvParser.Parse(asset.text) : new Dictionary<string, string>();
            }
            return bakedValues;
#endif
        }

#if UNITY_EDITOR
        // System.IO によるファイル読み込みはエディタでだけ使う（WebGL などでは使えないため）
        public static string EnvFilePath =>
            System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), ".env");

        public static Dictionary<string, string> ReadEnvFile()
        {
            string path = EnvFilePath;
            if (!System.IO.File.Exists(path))
            {
                return new Dictionary<string, string>();
            }

            return DotEnvParser.Parse(System.IO.File.ReadAllText(path));
        }
#endif
    }
}
