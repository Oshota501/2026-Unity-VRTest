using System;
using System.Collections.Generic;
using TownReview.Core.City;
using UnityEngine;

namespace TownReview.Infrastructure
{
    // 都市データ（建物・住民）を取得して CityBuilder に渡す。
    // 「どこから取得するか（仮のJSON / API）」はこのコンポーネントで切り替える。
    // Core層の CityBuilder は ICitySnapshotRepository の具体的な実装を知らなくてよいようにしている。
    //
    // CityBuilder がない場合は、Loaded イベントを購読している側が表示する
    // （SampleScene では CitySnapshotVisualizer が BuildingManager / PersonManager に渡す）。
    //
    // UIのボタンから LoadNow() / LoadIdeal() を呼ぶと、現在の都市と理想の都市を切り替えられる。
    public class CitySnapshotLoader : MonoBehaviour
    {
        public enum Source
        {
            LocalJson, // Data/ に置いたJSON（APIが完成するまでの仮データ）
            Api        // CityApiSettings の接続先に GET /v1/cities/{cityId}/snapshot を送る
        }

        [Tooltip("取得したデータを表示する CityBuilder。未設定なら同じオブジェクトから探す。CitySnapshotVisualizer で表示する場合は空のままでよい。")]
        [SerializeField] private CityBuilder cityBuilder;

        [SerializeField] private Source source = Source.LocalJson;
        [SerializeField] private CityMode mode = CityMode.Now;
        [SerializeField] private bool loadOnStart = true;

        [Header("API（Source = Api のとき）")]
        [SerializeField] private CityApiSettings apiSettings;

        [Header("仮データ（Source = LocalJson のとき）")]
        [SerializeField] private TextAsset nowJson;
        [SerializeField] private TextAsset idealJson;

        public event Action<CitySnapshot> Loaded;
        public event Action<string> LoadFailed;

        public CityMode Mode => mode;
        public bool IsLoading { get; private set; }

        // 連続で切り替えたとき、古いリクエストの結果で上書きしないための番号
        private int requestVersion;

        private void Awake()
        {
            if (cityBuilder == null)
            {
                cityBuilder = GetComponent<CityBuilder>();
            }
        }

        private void Start()
        {
            if (loadOnStart)
            {
                Load(mode);
            }
        }

        [ContextMenu("Reload")]
        public void Reload()
        {
            Load(mode);
        }

        public void LoadNow()
        {
            Load(CityMode.Now);
        }

        public void LoadIdeal()
        {
            Load(CityMode.Ideal);
        }

        public void Load(CityMode newMode)
        {
            // 読み込んでも表示する先がない場合は、設定漏れとして知らせる
            if (cityBuilder == null && Loaded == null)
            {
                Debug.LogError($"{nameof(CitySnapshotLoader)}: 表示先がありません。CityBuilder を設定するか、CitySnapshotVisualizer に登録してください。", this);
                return;
            }

            if (!TryCreateRepository(out ICitySnapshotRepository repository, out string cityId))
            {
                return;
            }

            mode = newMode;
            int version = ++requestVersion;
            IsLoading = true;

            repository.GetSnapshot(
                cityId,
                newMode,
                snapshot => OnLoaded(version, snapshot),
                error => OnFailed(version, error));
        }

        private bool TryCreateRepository(out ICitySnapshotRepository repository, out string cityId)
        {
            repository = null;
            cityId = "";

            switch (source)
            {
                case Source.Api:
                    if (apiSettings == null)
                    {
                        Debug.LogError($"{nameof(CitySnapshotLoader)}: Source が Api ですが、API Settings が設定されていません。", this);
                        return false;
                    }

                    string baseUrl = apiSettings.BaseUrl;
                    if (baseUrl == "")
                    {
                        Debug.LogError($"{nameof(CitySnapshotLoader)}: APIのURLがありません。プロジェクト直下の .env に {EnvSettings.CityApiBaseUrlKey}=https://... を書いてください（ビルドしたアプリの場合は、書いたあとビルドし直す）。", this);
                        return false;
                    }

                    repository = new HttpCitySnapshotRepository(baseUrl, apiSettings.TimeoutSeconds);
                    cityId = apiSettings.CityId;
                    return true;

                default:
                    if (nowJson == null && idealJson == null)
                    {
                        Debug.LogError($"{nameof(CitySnapshotLoader)}: Source が LocalJson ですが、JSON（Now Json / Ideal Json）が設定されていません。", this);
                        return false;
                    }

                    repository = new LocalCitySnapshotRepository(
                        nowJson != null ? nowJson.text : null,
                        idealJson != null ? idealJson.text : null);
                    cityId = "local";
                    return true;
            }
        }

        private void OnLoaded(int version, CitySnapshot snapshot)
        {
            // 通信中にこのオブジェクトが削除された、またはより新しい読み込みが始まった場合は無視する
            if (this == null || version != requestVersion)
            {
                return;
            }

            IsLoading = false;

            List<string> warnings = CitySnapshotValidator.Validate(snapshot);
            foreach (string warning in warnings)
            {
                Debug.LogWarning($"{nameof(CitySnapshotLoader)}: {warning}", this);
            }

            if (snapshot.Mode != "" && snapshot.ModeType != mode)
            {
                Debug.LogWarning($"{nameof(CitySnapshotLoader)}: mode={CityEnumParser.ToApiValue(mode)} を要求しましたが、レスポンスの mode は \"{snapshot.Mode}\" でした。", this);
            }

            if (cityBuilder != null)
            {
                cityBuilder.Build(snapshot);
            }
            Debug.Log($"{nameof(CitySnapshotLoader)}: 「{snapshot.CityName}」（{snapshot.Mode}）を読み込みました。建物 {snapshot.Buildings.Count} 件 / 住民 {snapshot.Avatars.Count} 人", this);
            Loaded?.Invoke(snapshot);
        }

        private void OnFailed(int version, string error)
        {
            if (this == null || version != requestVersion)
            {
                return;
            }

            IsLoading = false;
            Debug.LogError($"{nameof(CitySnapshotLoader)}: 都市データの取得に失敗しました: {error}", this);
            LoadFailed?.Invoke(error);
        }
    }
}
