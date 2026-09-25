using System;
using TownReview.Core.City;

namespace TownReview.Infrastructure
{
    // 仮データ用の ICitySnapshotRepository 実装。APIが完成する前の動作確認に使う。
    // JSONの文字列を受け取って読み込む（Unityでは TextAsset.text を渡す）。
    // WebGL/Android では System.IO でファイルを直接読めないため、ファイルパスではなく文字列で受け取る。
    // cityId は無視し、mode に応じて now 用 / ideal 用のJSONを返す。
    public class LocalCitySnapshotRepository : ICitySnapshotRepository
    {
        private readonly string nowJson;
        private readonly string idealJson;

        public LocalCitySnapshotRepository(string nowJson, string idealJson = null)
        {
            this.nowJson = nowJson;
            this.idealJson = idealJson;
        }

        public void GetSnapshot(string cityId, CityMode mode, Action<CitySnapshot> onSuccess, Action<string> onError)
        {
            string json = mode == CityMode.Ideal ? idealJson : nowJson;
            if (string.IsNullOrEmpty(json))
            {
                onError($"mode={CityEnumParser.ToApiValue(mode)} 用のJSONが設定されていません。");
                return;
            }

            if (CitySnapshotParser.TryParse(json, out CitySnapshot snapshot, out string errorMessage))
            {
                onSuccess(snapshot);
            }
            else
            {
                onError(errorMessage);
            }
        }
    }
}
