using TownReview.Core.City;
using TownReview.Infrastructure;
using UnityEngine;

// 都市API（GET /v1/cities/{cityId}/snapshot）の内容を SampleScene に反映する。
// CitySnapshotLoader が読み込んだ建物を BuildingManager（BuildingVisualizer）に、
// 住民を PersonManager（PersonVisualizer）に渡して並べてもらう。
// このオブジェクトの位置がAPIの原点 (0,0,0) になる（街との位置合わせは、このオブジェクトを動かす）。
public class CitySnapshotVisualizer : MonoBehaviour
{
    [Tooltip("都市データを取得する CitySnapshotLoader。未設定なら同じオブジェクトから探す。")]
    [SerializeField] private CitySnapshotLoader loader;

    [Tooltip("建物を並べる BuildingManager")]
    [SerializeField] private BuildingVisualizer buildingManager;

    [Tooltip("住民を並べる PersonManager")]
    [SerializeField] private PersonVisualizer personManager;

    void Awake()
    {
        if (loader == null)
        {
            loader = GetComponent<CitySnapshotLoader>();
        }
        if (loader == null)
        {
            Debug.LogError($"{nameof(CitySnapshotVisualizer)}: CitySnapshotLoader が見つかりません。同じオブジェクトに追加してください。", this);
        }
    }

    // CitySnapshotLoader は Start で読み込むので、それより前（OnEnable）に購読しておく
    void OnEnable()
    {
        if (loader != null) loader.Loaded += OnLoaded;
    }

    void OnDisable()
    {
        if (loader != null) loader.Loaded -= OnLoaded;
    }

    private void OnLoaded(CitySnapshot snapshot)
    {
        if (buildingManager != null)
        {
            buildingManager.ShowBuildings(snapshot.Buildings, transform);
        }
        else
        {
            Debug.LogWarning($"{nameof(CitySnapshotVisualizer)}: Building Manager が設定されていないため、建物を表示しません。", this);
        }

        if (personManager != null)
        {
            personManager.ShowAvatars(snapshot.Avatars, transform);
        }
        else
        {
            Debug.LogWarning($"{nameof(CitySnapshotVisualizer)}: Person Manager が設定されていないため、住民を表示しません。", this);
        }
    }
}
