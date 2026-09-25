using System;
using System.Collections.Generic;
using TownReview.Core.Reviews;
using TownReview.Infrastructure;
using UnityEngine;

namespace TownReview.Core.Avatars
{
    // 住民アバターをシーン上の指定位置に配置する。
    //
    // 注意：段階2の仮実装として、ここでLocalReviewRepositoryを直接生成している。
    // 本来Core層はInfrastructure層の具体的な実装（LocalReviewRepository/SupabaseReviewRepository）を
    // 知るべきではなく、IReviewRepositoryだけに依存するのが理想。
    // 段階3でSupabaseReviewRepositoryに差し替える際は、この生成部分だけを
    // 差し替え可能な仕組み（外部から注入する等）に整理する想定。
    public class AvatarSpawner : MonoBehaviour
    {
        [Serializable]
        public class SpawnPoint
        {
            public Transform point;
            public string meshCode;
        }

        [SerializeField] private ResidentAvatar avatarPrefab;
        [SerializeField] private List<SpawnPoint> spawnPoints = new();
        [SerializeField] private TextAsset reviewsJson;

        private void Start()
        {
            if (avatarPrefab == null || reviewsJson == null)
            {
                Debug.LogWarning($"{nameof(AvatarSpawner)}: avatarPrefab または reviewsJson が未設定です。", this);
                return;
            }

            IReviewRepository repository = new LocalReviewRepository(reviewsJson);
            var reviewService = new ReviewService(repository);

            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint.point == null)
                {
                    continue;
                }

                ResidentAvatar avatar = Instantiate(avatarPrefab, spawnPoint.point.position, spawnPoint.point.rotation);
                avatar.Initialize(reviewService, spawnPoint.meshCode);
            }
        }
    }
}
