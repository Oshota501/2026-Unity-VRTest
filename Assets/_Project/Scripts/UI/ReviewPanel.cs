using System.Collections.Generic;
using System.Text;
using TMPro;
using TownReview.Core.Avatars;
using TownReview.Core.Reviews;
using UnityEngine;

namespace TownReview.UI
{
    // 口コミを画面に重ねる2DのCanvas UI（掲示板のような一覧パネル）として表示する。
    // 3D空間内に文字を浮かべる形にはしない。
    // ResidentAvatarのReviewsRequestedイベントを購読し、Interact()が呼ばれたら表示する。
    public class ReviewPanel : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI contentText;

        private void Awake()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            ResidentAvatar.ReviewsRequested += Show;
        }

        private void OnDisable()
        {
            ResidentAvatar.ReviewsRequested -= Show;
        }

        public void Show(IReadOnlyList<Review> reviews)
        {
            if (contentText != null)
            {
                contentText.text = BuildDisplayText(reviews);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }
        }

        // 閉じるボタンのOnClickから呼ぶ想定。
        public void Close()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private static string BuildDisplayText(IReadOnlyList<Review> reviews)
        {
            if (reviews == null || reviews.Count == 0)
            {
                return "この地域の口コミはまだありません。";
            }

            var builder = new StringBuilder();
            foreach (Review review in reviews)
            {
                builder.AppendLine($"■ {ToLabel(review.TimeOfDay)}の口コミ");
                builder.AppendLine($"夜の治安 {review.NightSafety} / 夜の騒音 {review.NightNoise} / 休日の騒音 {review.HolidayNoise} / 日中の交通量 {review.DaytimeTraffic} / 生活の便利さ {review.Convenience}");
                builder.AppendLine(review.Comment);
                builder.AppendLine();
            }

            return builder.ToString();
        }

        private static string ToLabel(TimeOfDay timeOfDay)
        {
            switch (timeOfDay)
            {
                case TimeOfDay.Day:
                    return "昼";
                case TimeOfDay.Night:
                    return "夜";
                case TimeOfDay.Holiday:
                    return "休日";
                default:
                    return timeOfDay.ToString();
            }
        }
    }
}
