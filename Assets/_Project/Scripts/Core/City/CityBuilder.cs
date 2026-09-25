using System;
using System.Collections.Generic;
using UnityEngine;

namespace TownReview.Core.City
{
    // CitySnapshot（APIのレスポンス）をもとに、建物と住民アバターをシーンに並べる。
    //
    // ・APIの原点 (0,0,0) はこのオブジェクトの位置になる（PLATEAUの街と位置を合わせるときは、このオブジェクトを動かす）。
    // ・category ごとのプレハブが未設定なら、size の大きさの Cube を仮表示する（色は種類ごとに変える）。
    // ・住民プレハブが未設定なら、Capsule（体）＋Sphere（髪の色）を仮表示する。
    // ・Build() を呼ぶたびに前回生成したものは削除される（now / ideal の切り替えに使う）。
    public class CityBuilder : MonoBehaviour
    {
        [Serializable]
        public class CategoryPrefab
        {
            public BuildingCategory category;
            public GameObject prefab;
        }

        [Header("建物")]
        [Tooltip("category ごとのプレハブ。未設定の category は Other のプレハブ、それもなければ仮のCubeで表示する。")]
        [SerializeField] private List<CategoryPrefab> buildingPrefabs = new();

        [Tooltip("オンにすると、プレハブの Scale を API の size に合わせる（1m四方・底面が原点のプレハブを想定）。")]
        [SerializeField] private bool scalePrefabsToSize;

        [Header("住民")]
        [Tooltip("住民アバターのプレハブ（CityResident と Collider が付いたもの）。未設定なら仮のCapsuleで表示する。")]
        [SerializeField] private CityResident residentPrefab;

        [Header("地面")]
        [Tooltip("オンにすると bounds の範囲に仮の地面を作る。PLATEAUの街を表示している場合はオフのままにする。")]
        [SerializeField] private bool generateGround;

        // 建物・住民の生成が終わったときに呼ばれる。UI層（吹き出し表示など）が購読する。
        public static event Action<CitySnapshot, IReadOnlyList<CityResident>> Built;

        public CitySnapshot Current { get; private set; }
        public IReadOnlyList<CityResident> Residents => residents;

        private readonly List<CityResident> residents = new();
        private readonly Dictionary<Color, Material> materialCache = new();
        private GameObject cityRoot;

        public void Build(CitySnapshot snapshot)
        {
            if (snapshot == null)
            {
                Debug.LogWarning($"{nameof(CityBuilder)}: snapshot が null のため何もしません。", this);
                return;
            }

            Clear();
            Current = snapshot;

            cityRoot = new GameObject($"City_{snapshot.CityId}_{snapshot.Mode}");
            cityRoot.transform.SetParent(transform, false);

            if (generateGround)
            {
                CreateGround(snapshot.Bounds);
            }

            var buildingsById = new Dictionary<string, BuildingData>();
            foreach (BuildingData building in snapshot.Buildings)
            {
                CreateBuilding(building);
                if (building.Id != "" && !buildingsById.ContainsKey(building.Id))
                {
                    buildingsById.Add(building.Id, building);
                }
            }

            foreach (AvatarData avatar in snapshot.Avatars)
            {
                buildingsById.TryGetValue(avatar.BuildingId, out BuildingData relatedBuilding);
                residents.Add(CreateResident(avatar, relatedBuilding));
            }

            Built?.Invoke(snapshot, residents);
        }

        public void Clear()
        {
            residents.Clear();
            Current = null;

            if (cityRoot != null)
            {
                Destroy(cityRoot);
                cityRoot = null;
            }
        }

        private void OnDestroy()
        {
            foreach (Material material in materialCache.Values)
            {
                Destroy(material);
            }
            materialCache.Clear();
        }

        private void CreateBuilding(BuildingData data)
        {
            Vector3 position = ToVector3(data.Position);
            Vector3 size = ToSafeSize(data.Size);
            Quaternion rotation = Quaternion.Euler(0f, data.RotationY, 0f);
            string objectName = data.Name == "" ? $"Building_{data.Id}_{data.Category}" : $"Building_{data.Id}_{data.Name}";

            GameObject prefab = FindBuildingPrefab(data.CategoryType);
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, cityRoot.transform);
                instance.name = objectName;
                instance.transform.SetLocalPositionAndRotation(position, rotation);
                if (scalePrefabsToSize)
                {
                    instance.transform.localScale = size;
                }
                return;
            }

            // プレハブがない場合は仮のCube。Cubeの原点は中心なので、底面が position に来るよう高さの半分だけ上げる。
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(cityRoot.transform, false);
            cube.transform.SetLocalPositionAndRotation(position + Vector3.up * (size.y * 0.5f), rotation);
            cube.transform.localScale = size;
            ApplyColor(cube, GetCategoryColor(data.CategoryType));
        }

        private CityResident CreateResident(AvatarData data, BuildingData relatedBuilding)
        {
            Vector3 position = ToVector3(data.Position);
            Quaternion rotation = Quaternion.Euler(0f, data.RotationY, 0f);

            CityResident resident;
            if (residentPrefab != null)
            {
                resident = Instantiate(residentPrefab, cityRoot.transform);
                resident.transform.SetLocalPositionAndRotation(position, rotation);
            }
            else
            {
                resident = CreatePlaceholderResident(data, position, rotation);
            }

            resident.name = $"Resident_{data.Id}";
            resident.Initialize(data, relatedBuilding);
            return resident;
        }

        // 仮の住民：足元を原点にした空のオブジェクトの下に、体（Capsule）と頭（Sphere・髪の色）を置く。
        private CityResident CreatePlaceholderResident(AvatarData data, Vector3 position, Quaternion rotation)
        {
            var root = new GameObject();
            root.transform.SetParent(cityRoot.transform, false);
            root.transform.SetLocalPositionAndRotation(position, rotation);

            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0f, 0.8f, 0f);
            body.transform.localScale = new Vector3(0.5f, 0.8f, 0.5f);
            ApplyColor(body, GetEmotionColor(data.EmotionType));

            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            // 当たり判定は体のCapsuleだけで十分なので、頭のColliderは外す
            Destroy(head.GetComponent<Collider>());
            ApplyColor(head, ParseHairColor(data.Appearance.HairColor));

            return root.AddComponent<CityResident>();
        }

        private void CreateGround(CityBounds bounds)
        {
            Vector3 min = ToVector3(bounds.Min);
            Vector3 max = ToVector3(bounds.Max);
            Vector3 size = max - min;
            if (size.x <= 0f || size.z <= 0f)
            {
                return;
            }

            // Plane は 10m四方なので、10で割った値をスケールにする
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(cityRoot.transform, false);
            ground.transform.localPosition = new Vector3((min.x + max.x) * 0.5f, min.y, (min.z + max.z) * 0.5f);
            ground.transform.localScale = new Vector3(size.x / 10f, 1f, size.z / 10f);
            ApplyColor(ground, new Color(0.45f, 0.45f, 0.45f));
        }

        private GameObject FindBuildingPrefab(BuildingCategory category)
        {
            GameObject otherPrefab = null;
            foreach (CategoryPrefab entry in buildingPrefabs)
            {
                if (entry == null || entry.prefab == null)
                {
                    continue;
                }

                if (entry.category == category)
                {
                    return entry.prefab;
                }

                if (entry.category == BuildingCategory.Other)
                {
                    otherPrefab = entry.prefab;
                }
            }

            return otherPrefab;
        }

        // 同じ色のマテリアルは使い回す（生成数が増えてもマテリアルが増えすぎないように）
        private void ApplyColor(GameObject target, Color color)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null || renderer.sharedMaterial == null)
            {
                return;
            }

            if (!materialCache.TryGetValue(color, out Material material))
            {
                material = new Material(renderer.sharedMaterial) { color = color };
                materialCache.Add(color, material);
            }

            renderer.sharedMaterial = material;
        }

        private static Vector3 ToVector3(CityVector3 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        // 大きさ0の建物は見えず当たり判定も作れないため、最低0.1mにする
        private static Vector3 ToSafeSize(CityVector3 value)
        {
            const float minSize = 0.1f;
            return new Vector3(Mathf.Max(value.X, minSize), Mathf.Max(value.Y, minSize), Mathf.Max(value.Z, minSize));
        }

        private static Color ParseHairColor(string htmlColor)
        {
            return ColorUtility.TryParseHtmlString(htmlColor, out Color color) ? color : new Color(0.15f, 0.1f, 0.05f);
        }

        private static Color GetCategoryColor(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.House: return new Color(0.95f, 0.85f, 0.7f);
                case BuildingCategory.Apartment: return new Color(0.8f, 0.8f, 0.85f);
                case BuildingCategory.Supermarket: return new Color(0.95f, 0.6f, 0.3f);
                case BuildingCategory.ConvenienceStore: return new Color(0.3f, 0.7f, 0.95f);
                case BuildingCategory.Restaurant: return new Color(0.9f, 0.4f, 0.4f);
                case BuildingCategory.Station: return new Color(0.5f, 0.5f, 0.6f);
                case BuildingCategory.Park: return new Color(0.4f, 0.75f, 0.4f);
                case BuildingCategory.School: return new Color(0.95f, 0.9f, 0.5f);
                case BuildingCategory.Hospital: return new Color(0.95f, 0.95f, 0.95f);
                default: return new Color(0.6f, 0.6f, 0.6f);
            }
        }

        private static Color GetEmotionColor(ResidentEmotion emotion)
        {
            switch (emotion)
            {
                case ResidentEmotion.Happy: return new Color(1f, 0.85f, 0.3f);
                case ResidentEmotion.Annoyed: return new Color(0.9f, 0.35f, 0.3f);
                case ResidentEmotion.Worried: return new Color(0.45f, 0.6f, 0.95f);
                default: return new Color(0.8f, 0.8f, 0.8f);
            }
        }
    }
}
