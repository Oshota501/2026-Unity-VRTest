using System;
using System.Collections.Generic;
using TownReview.Core.Interaction;
using TownReview.Core.Reviews;
using UnityEngine;

namespace TownReview.Core.Avatars
{
    // 住民アバター本体。IInteractableを実装し、Platform層から呼ばれるInteract()で
    // 自分のいるエリア（メッシュコード）の承認済み口コミを取得する。
    //
    // 取得結果はUI層に直接渡さず、静的イベントで通知する。
    // Core層はUI層を参照しない方針のため、UI側（ReviewPanelなど）がこのイベントを購読する形にしている。
    [RequireComponent(typeof(Collider))]
    public class ResidentAvatar : MonoBehaviour, IInteractable
    {
        [SerializeField] private string meshCode;

        private ReviewService reviewService;

        // 口コミの取得が完了したときに呼ばれる。UI層のReviewPanelが購読する。
        public static event Action<IReadOnlyList<Review>> ReviewsRequested;

        // AvatarSpawnerが生成直後に呼び出し、必要な依存関係を渡す。
        public void Initialize(ReviewService service, string areaMeshCode)
        {
            reviewService = service;
            meshCode = areaMeshCode;
        }

        [ContextMenu("Test: Interact")]
        public void Interact()
        {
            if (reviewService == null)
            {
                Debug.LogWarning($"{nameof(ResidentAvatar)}: ReviewServiceが初期化されていません。Initialize()を呼んでください。", this);
                return;
            }

            reviewService.GetApprovedReviews(meshCode, null, OnReviewsLoaded, OnLoadFailed);
        }

        private void OnReviewsLoaded(IReadOnlyList<Review> reviews)
        {
            ReviewsRequested?.Invoke(reviews);
        }

        private void OnLoadFailed(string errorMessage)
        {
            Debug.LogWarning($"{nameof(ResidentAvatar)}: 口コミの取得に失敗しました: {errorMessage}", this);
        }
    }
}
