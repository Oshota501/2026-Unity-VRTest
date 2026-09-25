using UnityEngine;

namespace TownReview.Infrastructure
{
    // 都市APIの接続先の設定。コードに直接書かず、Data/ に置いた設定ファイル（ScriptableObject）で持つ。
    // 作り方：Projectウィンドウで Assets/_Project/Data を右クリック → Create → TownReview → City API Settings
    [CreateAssetMenu(fileName = "CityApiSettings", menuName = "TownReview/City API Settings")]
    public class CityApiSettings : ScriptableObject
    {
        [Tooltip("APIのベースURL（末尾の / は不要）。例：https://api.example.com")]
        [SerializeField] private string baseUrl = "https://api.example.com";

        [Tooltip("都市ID。MVPでは sample 固定でよい。")]
        [SerializeField] private string cityId = "sample";

        [Tooltip("通信のタイムアウト（秒）")]
        [SerializeField] private int timeoutSeconds = 10;

        public string BaseUrl => baseUrl;
        public string CityId => cityId;
        public int TimeoutSeconds => timeoutSeconds;
    }
}
