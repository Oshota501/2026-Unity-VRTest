using System.Collections.Generic;
using System.Text;
using TMPro;
using TownReview.Core.Avatars;
using TownReview.Core.City;
using TownReview.Core.Reviews;
using UnityEngine;

namespace TownReview.UI
{
    // 口コミを画面に重ねる2DのCanvas UI（掲示板のような一覧パネル）として表示する。
    // 3D空間内に文字を浮かべる形にはしない。
    // ResidentAvatarのReviewsRequestedイベントを購読し、Interact()が呼ばれたら表示する。
    // 都市API（CitySnapshot）の住民 CityResident に触れたときも、同じパネルにコメント全文を表示する。
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
            CityResident.CommentRequested += ShowResident;
        }

        private void OnDisable()
        {
            ResidentAvatar.ReviewsRequested -= Show;
            CityResident.CommentRequested -= ShowResident;
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

        public void ShowResident(CityResident resident)
        {
            if (resident == null || resident.Data == null)
            {
                return;
            }

            if (contentText != null)
            {
                contentText.text = BuildResidentText(resident);
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

        private static string BuildResidentText(CityResident resident)
        {
            AvatarData data = resident.Data;
            var builder = new StringBuilder();

            if (data.Voice != "")
            {
                builder.AppendLine($"■ {data.Voice}");
            }

            BuildingData building = resident.RelatedBuilding;
            if (building != null)
            {
                string place = building.Name != "" ? $"{building.Name}（{ToLabel(building.CategoryType)}）" : ToLabel(building.CategoryType);
                builder.AppendLine($"場所：{place}");
            }

            builder.AppendLine($"気分：{ToLabel(resident.Emotion)}");
            builder.AppendLine();
            builder.AppendLine(data.Comment != "" ? data.Comment : "（コメントはありません）");
            return builder.ToString();
        }

        private static string ToLabel(BuildingCategory category)
        {
            switch (category)
            {
                case BuildingCategory.House: return "戸建て";
                case BuildingCategory.Apartment: return "集合住宅";
                case BuildingCategory.Supermarket: return "スーパー";
                case BuildingCategory.ConvenienceStore: return "コンビニ";
                case BuildingCategory.Restaurant: return "飲食店";
                case BuildingCategory.Station: return "駅";
                case BuildingCategory.Park: return "公園";
                case BuildingCategory.School: return "学校";
                case BuildingCategory.Hospital: return "病院";
                default: return "その他";
            }
        }

        private static string ToLabel(ResidentEmotion emotion)
        {
            switch (emotion)
            {
                case ResidentEmotion.Happy: return "満足";
                case ResidentEmotion.Annoyed: return "不満";
                case ResidentEmotion.Worried: return "不安";
                default: return "ふつう";
            }
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
