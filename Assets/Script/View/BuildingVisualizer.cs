using System.Collections.Generic;
using TownReview.Core.City;
using UnityEngine;

// 建物のアセット（プレハブ）を「建物ID → プレハブ」の辞書で持ち、APIの建物を配置する。
// 建物IDはプレハブ名と同じにする（例：APIの id が "Tohu" なら、Tohu.prefab を使う）。
// 辞書にないIDの建物は、APIの size の大きさの Cube で代用する。
public class BuildingVisualizer : MonoBehaviour
{
    [SerializeField] private List<BuildingRegistry> buildings = new List<BuildingRegistry>();

    [Tooltip("オンにすると、再生開始時に登録されている建物をすべてプレハブの位置のまま表示する（PLATEAUの街など）。APIの建物IDと同じ名前のプレハブがあると二重に表示されるので、そのときはオフにする。")]
    [SerializeField] private bool spawnAllOnStart = true;

    // 大きさ0の建物は見えず当たり判定も作れないため、Cube は最低この大きさにする
    private const float MinCubeSize = 0.1f;

    // 建物のアセット。キーはプレハブ名（＝APIの建物ID）
    private readonly Dictionary<string, GameObject> prefabs = new();

    // APIの建物IDごとに生成した建物
    private readonly Dictionary<string, GameObject> view = new();

    private GameObject apiRoot;

    // APIの読み込みが Start より先に来ても使えるよう、辞書は Awake で作る
    void Awake()
    {
        foreach (var registry in buildings)
        {
            if (registry == null) continue;

            foreach (var prefab in registry.GetView())
            {
                if (prefab == null) continue;

                if (!prefabs.TryAdd(prefab.name, prefab))
                {
                    Debug.LogWarning($"{nameof(BuildingVisualizer)}: 建物ID \"{prefab.name}\" のプレハブが複数登録されています。最初のものを使います。", this);
                }
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!spawnAllOnStart) return;

        // foreach (var prefab in prefabs.Values)
        // {
        //     Instantiate(prefab);
        // }
    }

    // APIの建物を並べる。origin の位置がAPIの原点 (0,0,0) になる。
    // 呼ぶたびに前回並べた建物は削除する（now / ideal の切り替えに使う）。
    public void ShowBuildings(IReadOnlyList<BuildingData> buildingData, Transform origin)
    {
        ClearBuildings();

        apiRoot = new GameObject("ApiBuildings");
        apiRoot.transform.SetParent(origin, false);

        foreach (BuildingData data in buildingData)
        {
            GameObject building = CreateBuilding(data);
            if (data.Id != "")
            {
                view.TryAdd(data.Id, building);
            }
        }
    }

    public void ClearBuildings()
    {
        view.Clear();

        if (apiRoot != null)
        {
            Destroy(apiRoot);
            apiRoot = null;
        }
    }

    private GameObject CreateBuilding(BuildingData data)
    {
        Vector3 position = new Vector3(data.Position.X, data.Position.Y, data.Position.Z);
        Quaternion rotation = Quaternion.Euler(0f, data.RotationY, 0f);
        string objectName = data.Name == "" ? $"Building_{data.Id}_{data.Category}" : $"Building_{data.Id}_{data.Name}";

        if (prefabs.TryGetValue(data.Id, out GameObject prefab))
        {
            GameObject instance = Instantiate(prefab, apiRoot.transform);
            instance.name = objectName;
            instance.transform.SetLocalPositionAndRotation(position, rotation);
            return instance;
        }

        // 辞書にないIDは Cube で代用する。Cubeの原点は中心なので、底面が position に来るよう高さの半分だけ上げる。
        Vector3 size = new Vector3(
            Mathf.Max(data.Size.X, MinCubeSize),
            Mathf.Max(data.Size.Y, MinCubeSize),
            Mathf.Max(data.Size.Z, MinCubeSize));

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName + "(Cube)";
        cube.transform.SetParent(apiRoot.transform, false);
        cube.transform.SetLocalPositionAndRotation(position + Vector3.up * (size.y * 0.5f), rotation);
        cube.transform.localScale = size;
        return cube;
    }
}
