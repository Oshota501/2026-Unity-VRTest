using System.Collections.Generic;
using TMPro;
using TownReview.Core.City;
using UnityEngine;

namespace TownReview.UI
{
    // 都市APIの住民（CityResident）の頭の上に、voice（短い一言）を吹き出しとして表示する。
    // CityBuilder.Built イベントを購読し、街が生成（再生成）されるたびに吹き出しを作り直す。
    // 吹き出しは住民の子オブジェクトとして作るので、住民が削除されると一緒に消える。
    //
    // 日本語を表示するには、日本語の文字を含む TMP のフォントアセットを Font に設定すること
    // （口コミパネルで使っているものと同じでよい）。
    public class ResidentVoiceBubbles : MonoBehaviour
    {
        [Tooltip("日本語を含むTMPフォントアセット。未設定なら TMP Settings の既定フォント（日本語は表示されない）。")]
        [SerializeField] private TMP_FontAsset font;

        [Tooltip("足元からの高さ（m）")]
        [SerializeField] private float heightOffset = 2.4f;

        [SerializeField] private float fontSize = 2f;

        [Tooltip("吹き出しの横幅（m）。これを超えると折り返す。")]
        [SerializeField] private float width = 3f;

        [SerializeField] private Color textColor = Color.white;

        [Tooltip("吹き出しを向ける先のカメラ。未設定なら Main Camera を使う。")]
        [SerializeField] private Camera targetCamera;

        private readonly List<Transform> bubbles = new();

        private void OnEnable()
        {
            CityBuilder.Built += OnCityBuilt;
        }

        private void OnDisable()
        {
            CityBuilder.Built -= OnCityBuilt;
        }

        private void OnCityBuilt(CitySnapshot snapshot, IReadOnlyList<CityResident> residents)
        {
            bubbles.Clear();
            foreach (CityResident resident in residents)
            {
                if (resident == null || resident.Data == null || resident.Data.Voice == "")
                {
                    continue;
                }

                bubbles.Add(CreateBubble(resident));
            }
        }

        private Transform CreateBubble(CityResident resident)
        {
            // TextMeshPro を付けると Transform が RectTransform に置き換わるため、位置の設定より先に付ける
            var bubble = new GameObject("VoiceBubble");
            var text = bubble.AddComponent<TextMeshPro>();
            bubble.transform.SetParent(resident.transform, false);
            bubble.transform.localPosition = new Vector3(0f, heightOffset, 0f);

            if (font != null)
            {
                text.font = font;
            }
            text.text = resident.Data.Voice;
            text.fontSize = fontSize;
            text.color = textColor;
            text.alignment = TextAlignmentOptions.Bottom;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.rectTransform.sizeDelta = new Vector2(width, 1f);

            return bubble.transform;
        }

        // 吹き出しを常にカメラの方へ向ける（数が少ないので、ここでまとめて処理する）
        private void LateUpdate()
        {
            if (bubbles.Count == 0)
            {
                return;
            }

            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null)
            {
                return;
            }

            Quaternion rotation = cam.transform.rotation;
            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                if (bubbles[i] == null)
                {
                    bubbles.RemoveAt(i);
                    continue;
                }

                bubbles[i].rotation = rotation;
            }
        }
    }
}
