using System.Collections.Generic;
using TownReview.Core.City;
using UnityEngine;
using UnityEngine.Serialization;

// 街にいる人（Person）を並べ、プレイヤーとの距離に応じてコメントの表示を切り替える。
// ・再生開始時：PersonRegistry に登録したプレハブを、プレハブの位置のまま表示する（Spawn All On Start がオンのとき）
// ・APIの住民：CitySnapshotVisualizer から ShowAvatars() が呼ばれ、住民ごとに Avatar Prefab を並べる
public class PersonVisualizer : MonoBehaviour
{
    [Tooltip("人のプレハブを登録した PersonRegistry")]
    [FormerlySerializedAs("buildings")]
    [SerializeField] private List<PersonRegistry> registries = new();

    [SerializeField] private PlayerController player;

    [Tooltip("この距離（m）より近づくとコメントを表示する")]
    [SerializeField] private float detectionPlayerDistance = 6f;

    [Tooltip("APIの住民に使う Person のプレハブ。未設定なら PersonRegistry に登録されている最初のプレハブを使う。")]
    [SerializeField] private Person avatarPrefab;

    [Tooltip("オンにすると、再生開始時に PersonRegistry の人をすべてプレハブの位置のまま表示する。APIの住民だけを出したいときはオフにする。")]
    [SerializeField] private bool spawnAllOnStart = true;

    // 表示中のすべての人（距離の判定に使う）
    private readonly List<Person> persons = new();

    // APIの住民。キーは住民ID
    private readonly Dictionary<string, Person> residents = new();
    private GameObject apiRoot;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError($"{nameof(PersonVisualizer)}: Player が設定されていません。近づいてもコメントは表示されません。", this);
        }

        if (!spawnAllOnStart) return;

        foreach (Person prefab in EnumeratePersonPrefabs())
        {
            AddPerson(Instantiate(prefab));
        }
    }

    void Update()
    {
        if (player == null) return;

        // 地面に沿った距離（x・z）で判定する。高さ（y）は無視する。
        Vector3 playerPosition = player.transform.position;
        float sqrDetectionDistance = detectionPlayerDistance * detectionPlayerDistance;
        for (int i = persons.Count - 1; i >= 0; i--)
        {
            Person person = persons[i];
            if (person == null)
            {
                // 削除された人はリストから外す
                persons.RemoveAt(i);
                continue;
            }

            Vector3 offset = person.transform.position - playerPosition;
            offset.y = 0f;
            if (offset.sqrMagnitude < sqrDetectionDistance)
            {
                person.ClosedPlayer();
            }
            else
            {
                person.FarPlayer();
            }
        }
    }

    // APIの住民を並べる。origin の位置がAPIの原点 (0,0,0) になる。
    // 近づいたときに voice（短い一言）を表示する。呼ぶたびに前回並べた住民は削除する。
    public void ShowAvatars(IReadOnlyList<AvatarData> avatars, Transform origin)
    {
        ClearAvatars();

        Person prefab = FindAvatarPrefab();
        if (prefab == null)
        {
            Debug.LogError($"{nameof(PersonVisualizer)}: 住民に使うプレハブがありません。PersonManager の Avatar Prefab に、Person が付いたプレハブを設定してください。", this);
            return;
        }

        apiRoot = new GameObject("ApiResidents");
        apiRoot.transform.SetParent(origin, false);

        foreach (AvatarData data in avatars)
        {
            if (residents.ContainsKey(data.Id))
            {
                Debug.LogWarning($"{nameof(PersonVisualizer)}: 住民ID \"{data.Id}\" が重複しているため、2人目以降は配置しませんでした。", this);
                continue;
            }

            Person person = Instantiate(prefab, apiRoot.transform);
            person.name = $"Resident_{data.Id}";
            person.transform.SetLocalPositionAndRotation(
                new Vector3(data.Position.X, data.Position.Y, data.Position.Z),
                Quaternion.Euler(0f, data.RotationY, 0f));
            person.SetComment(data.Voice != "" ? data.Voice : data.Comment);

            AddPerson(person);
            residents.Add(data.Id, person);
        }

        Debug.Log($"{nameof(PersonVisualizer)}: 住民を {residents.Count} 人表示しました（プレハブ：{prefab.name}）。", this);
    }

    public void ClearAvatars()
    {
        foreach (Person person in residents.Values)
        {
            persons.Remove(person);
        }
        residents.Clear();

        if (apiRoot != null)
        {
            Destroy(apiRoot);
            apiRoot = null;
        }
    }

    private void AddPerson(Person person)
    {
        person.player = player;
        persons.Add(person);
    }

    private Person FindAvatarPrefab()
    {
        if (avatarPrefab != null) return avatarPrefab;

        foreach (Person prefab in EnumeratePersonPrefabs())
        {
            return prefab;
        }

        return null;
    }

    // PersonRegistry に登録されたプレハブのうち、Person が付いているものを返す。
    // 削除されたプレハブ（Missing）や Person が付いていないプレハブは、警告を出して飛ばす。
    private IEnumerable<Person> EnumeratePersonPrefabs()
    {
        foreach (PersonRegistry registry in registries)
        {
            if (registry == null) continue;

            foreach (GameObject prefab in registry.GetView())
            {
                if (prefab == null)
                {
                    Debug.LogWarning($"{nameof(PersonVisualizer)}: {registry.name} に、削除されたプレハブ（Missing）が登録されています。", registry);
                    continue;
                }

                if (!prefab.TryGetComponent(out Person person))
                {
                    Debug.LogWarning($"{nameof(PersonVisualizer)}: プレハブ「{prefab.name}」の一番上のオブジェクトに Person が付いていません。", prefab);
                    continue;
                }

                yield return person;
            }
        }
    }
}
