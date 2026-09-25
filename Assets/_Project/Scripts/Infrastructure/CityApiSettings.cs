using UnityEngine;

namespace TownReview.Infrastructure
{
    // 都市APIの接続先の設定。コードに直接書かず、Data/ に置いた設定ファイル（ScriptableObject）で持つ。
    // 作り方：Projectウィンドウで Assets/_Project/Data を右クリック → Create → TownReview → City API Settings
    //
    // ベースURLはGitに入れないため、この設定ファイルではなくプロジェクト直下の .env に書く。
    //   CITY_API_BASE_URL=https://api.example.com
    [CreateAssetMenu(fileName = "CityApiSettings", menuName = "TownReview/City API Settings")]
    public class CityApiSettings : ScriptableObject
    {
        [Tooltip("都市ID。MVPでは sample 固定でよい。")]
        [SerializeField] private string cityId = "sample";

        [Tooltip("通信のタイムアウト（秒）")]
        [SerializeField] private int timeoutSeconds = 10;

        // .env の CITY_API_BASE_URL（末尾の / は不要）。書かれていなければ ""
        public string BaseUrl => EnvSettings.Get(EnvSettings.CityApiBaseUrlKey);
        public string CityId => cityId;
        public int TimeoutSeconds => timeoutSeconds;
    }
}
