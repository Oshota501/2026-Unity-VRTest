using System;
using TownReview.Core.Interaction;
using UnityEngine;

namespace TownReview.Core.City
{
    // APIから受け取った住民アバター1人分。CityBuilder が生成時に Initialize() を呼ぶ。
    // IInteractable を実装しており、Platform層（VRコントローラー・クリックなど）が Interact() を呼ぶと
    // CommentRequested イベントでコメント全文の表示を依頼する（UI層の ReviewPanel が受け取る）。
    //
    // 当たり判定（Collider）はこのオブジェクトか子オブジェクトに付けておくこと。
    public class CityResident : MonoBehaviour, IInteractable
    {
        // 触れられたときに呼ばれる。UI層の ReviewPanel が購読する。
        public static event Action<CityResident> CommentRequested;

        public AvatarData Data { get; private set; }

        // 関連する建物（buildingId が "" または見つからない場合は null）
        public BuildingData RelatedBuilding { get; private set; }

        public ResidentEmotion Emotion => Data != null ? Data.EmotionType : ResidentEmotion.Neutral;

        public void Initialize(AvatarData data, BuildingData relatedBuilding)
        {
            Data = data;
            RelatedBuilding = relatedBuilding;

            if (GetComponentInChildren<Collider>() == null)
            {
                Debug.LogWarning($"{nameof(CityResident)}: Collider がないため、触れて反応させることができません。", this);
            }
        }

        [ContextMenu("Test: Interact")]
        public void Interact()
        {
            if (Data == null)
            {
                Debug.LogWarning($"{nameof(CityResident)}: データが設定されていません。Initialize() を呼んでください。", this);
                return;
            }

            CommentRequested?.Invoke(this);
        }
    }
}
