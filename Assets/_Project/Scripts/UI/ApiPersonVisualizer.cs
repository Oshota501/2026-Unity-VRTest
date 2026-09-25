using System.Collections.Generic;
using TownReview.Core.City;
using TownReview.Infrastructure;
using UnityEngine;

namespace TownReview.UI
{
    // SampleScene の既存の仕組み（Person プレハブ ＋ プレイヤーが近づくとコメントを表示）を使って、
    // 都市API（CitySnapshotLoader）から取得した住民（avatars）を配置する。建物（buildings）は使わない。
    //
    // ・CitySnapshotLoader の Loaded イベントを受け取るたびに、前回の住民を消して作り直す（now / ideal の切り替えにも対応）。
    // ・APIの座標はワールド座標としてそのまま使う（City Origin を設定すると、そのオブジェクトを原点にする）。
    // ・プレイヤーとの距離の判定は、既存の PersonVisualizer と同じく Person.ClosedPlayer() / FarPlayer() を呼ぶ。
    //   高さの差で判定が変わらないよう、水平方向（x と z）の距離で判定する。
    public class ApiPersonVisualizer : MonoBehaviour
    {
        [Tooltip("住民データを取得する CitySnapshotLoader。未設定なら同じオブジェクトから探す。")]
        [SerializeField] private CitySnapshotLoader loader;

        [Tooltip("住民として置くプレハブ（Person コンポーネント付き。例：Assets/Script/View/Person/People.prefab）")]
        [SerializeField] private Person personPrefab;

        [Tooltip("近づいたかどうかを判定するプレイヤー。コメントはこのプレイヤーの Eye カメラの方を向く。")]
        [SerializeField] private PlayerController player;

        [Tooltip("この距離（m）より近づくとコメントを表示する")]
        [SerializeField] private float detectionPlayerDistance = 6f;

        [Header("配置")]
        [Tooltip("APIの原点 (0,0,0) にするオブジェクト。未設定ならワールドの原点。")]
        [SerializeField] private Transform cityOrigin;

        [Tooltip("APIの position（足元）から上にずらす量。People.prefab（1mのCube・中心が原点）なら 0.5。")]
        [SerializeField] private float heightOffset = 0.5f;

        [Header("コメント")]
        [SerializeField] private PersonCommentFormatter.TextMode textMode = PersonCommentFormatter.TextMode.VoiceAndComment;

        [Tooltip("1行の文字数。これを超えると改行する（0で改行しない）。")]
        [SerializeField] private int maxCharsPerLine = 20;

        private readonly List<Person> people = new();

        // 生成したフレーム。Person は Start() で元の高さを覚えるため、Start() より前に距離判定をしないようにする
        private int spawnedFrame = -1;

        private void Awake()
        {
            if (loader == null)
            {
                loader = GetComponent<CitySnapshotLoader>();
            }
        }

        private void OnEnable()
        {
            if (loader != null)
            {
                loader.Loaded += Show;
            }
        }

        private void OnDisable()
        {
            if (loader != null)
            {
                loader.Loaded -= Show;
            }
        }

        private void Start()
        {
            if (loader == null)
            {
                Debug.LogError($"{nameof(ApiPersonVisualizer)}: CitySnapshotLoader が設定されていません。", this);
            }

            if (personPrefab == null)
            {
                Debug.LogError($"{nameof(ApiPersonVisualizer)}: Person Prefab が設定されていません。", this);
            }

            if (player == null || player.eye == null)
            {
                Debug.LogError($"{nameof(ApiPersonVisualizer)}: Player（と Player の Eye カメラ）が設定されていません。コメントを表示できません。", this);
            }
        }

        public void Show(CitySnapshot snapshot)
        {
            Clear();
            if (snapshot == null || personPrefab == null)
            {
                return;
            }

            foreach (AvatarData avatar in snapshot.Avatars)
            {
                people.Add(Spawn(avatar));
            }
            spawnedFrame = Time.frameCount;
        }

        public void Clear()
        {
            foreach (Person person in people)
            {
                if (person != null)
                {
                    Destroy(person.gameObject);
                }
            }
            people.Clear();
        }

        private Person Spawn(AvatarData avatar)
        {
            var localPosition = new Vector3(avatar.Position.X, avatar.Position.Y + heightOffset, avatar.Position.Z);
            Quaternion localRotation = Quaternion.Euler(0f, avatar.RotationY, 0f);

            Vector3 position = cityOrigin != null ? cityOrigin.TransformPoint(localPosition) : localPosition;
            Quaternion rotation = cityOrigin != null ? cityOrigin.rotation * localRotation : localRotation;

            // 見やすいよう、このオブジェクトの子にまとめる（ワールド上の位置は変わらない）
            Person person = Instantiate(personPrefab, position, rotation, transform);
            person.name = $"Person_{avatar.Id}";
            person.player = player;
            person.SetComment(PersonCommentFormatter.Build(avatar, textMode, maxCharsPerLine));
            return person;
        }

        private void Update()
        {
            if (player == null || player.eye == null || people.Count == 0 || Time.frameCount == spawnedFrame)
            {
                return;
            }

            Vector3 playerPosition = player.transform.position;
            float sqrDistanceLimit = detectionPlayerDistance * detectionPlayerDistance;

            foreach (Person person in people)
            {
                if (person == null)
                {
                    continue;
                }

                Vector3 offset = person.transform.position - playerPosition;
                offset.y = 0f;
                if (offset.sqrMagnitude < sqrDistanceLimit)
                {
                    person.ClosedPlayer();
                }
                else
                {
                    person.FarPlayer();
                }
            }
        }
    }
}
