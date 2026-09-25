using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TownReview.Core.Reviews
{
    // 口コミの時間帯（昼・夜・休日）。
    // 将来Core/TimeOfDayに作るTimeOfDayControllerでも同じ区分を使う想定。
    public enum TimeOfDay
    {
        Day,
        Night,
        Holiday
    }

    // 口コミの審査状態。承認済み（Approved）のものだけを画面に表示する。
    public enum ReviewStatus
    {
        Pending,  // 審査中
        Approved, // 承認
        Rejected  // 却下
    }

    // 口コミ1件分のデータ。評価項目は5段階（1〜5）で固定。
    public class Review
    {
        [JsonProperty("id")]
        public string Id;

        // PLATEAUと同じ地域メッシュコード。表示中エリアの口コミだけを取得するために使う。
        [JsonProperty("mesh_code")]
        public string MeshCode;

        [JsonProperty("time_of_day")]
        [JsonConverter(typeof(StringEnumConverter))]
        public TimeOfDay TimeOfDay;

        [JsonProperty("status")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ReviewStatus Status;

        [JsonProperty("night_safety")]
        public int NightSafety;      // 夜の治安（1〜5）

        [JsonProperty("night_noise")]
        public int NightNoise;       // 夜の騒音（1〜5）

        [JsonProperty("holiday_noise")]
        public int HolidayNoise;     // 休日の騒音（1〜5）

        [JsonProperty("daytime_traffic")]
        public int DaytimeTraffic;   // 日中の交通量（1〜5）

        [JsonProperty("convenience")]
        public int Convenience;      // 生活の便利さ（1〜5）

        [JsonProperty("comment")]
        public string Comment;       // 自由記述
    }
}
