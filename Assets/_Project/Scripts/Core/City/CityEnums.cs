namespace TownReview.Core.City
{
    // 取得する都市の種類（クエリパラメータ mode）
    public enum CityMode
    {
        Now,   // 現在の都市
        Ideal  // 理想の都市
    }

    // 建物の種類（レスポンスの category）
    public enum BuildingCategory
    {
        House,            // 戸建て
        Apartment,        // 集合住宅
        Supermarket,      // スーパー
        ConvenienceStore, // コンビニ
        Restaurant,       // 飲食店
        Station,          // 駅
        Park,             // 公園
        School,           // 学校
        Hospital,         // 病院
        Other             // その他（未知の値もここに入る）
    }

    // 住民の表情（レスポンスの emotion）
    public enum ResidentEmotion
    {
        Neutral,
        Happy,
        Annoyed,
        Worried
    }

    // APIの文字列と列挙型の相互変換。未知の値は既定値（Other / Neutral / Now）にする。
    public static class CityEnumParser
    {
        public static string ToApiValue(CityMode mode)
        {
            return mode == CityMode.Ideal ? "ideal" : "now";
        }

        public static bool IsValidMode(string value)
        {
            string key = Normalize(value);
            return key == "now" || key == "ideal";
        }

        public static CityMode ParseMode(string value)
        {
            return Normalize(value) == "ideal" ? CityMode.Ideal : CityMode.Now;
        }

        public static bool IsKnownCategory(string value)
        {
            return ParseCategory(value) != BuildingCategory.Other || Normalize(value) == "other";
        }

        public static BuildingCategory ParseCategory(string value)
        {
            switch (Normalize(value))
            {
                case "house": return BuildingCategory.House;
                case "apartment": return BuildingCategory.Apartment;
                case "supermarket": return BuildingCategory.Supermarket;
                case "convenience_store": return BuildingCategory.ConvenienceStore;
                case "restaurant": return BuildingCategory.Restaurant;
                case "station": return BuildingCategory.Station;
                case "park": return BuildingCategory.Park;
                case "school": return BuildingCategory.School;
                case "hospital": return BuildingCategory.Hospital;
                default: return BuildingCategory.Other;
            }
        }

        public static bool IsKnownEmotion(string value)
        {
            string key = Normalize(value);
            return key == "neutral" || key == "happy" || key == "annoyed" || key == "worried";
        }

        public static ResidentEmotion ParseEmotion(string value)
        {
            switch (Normalize(value))
            {
                case "happy": return ResidentEmotion.Happy;
                case "annoyed": return ResidentEmotion.Annoyed;
                case "worried": return ResidentEmotion.Worried;
                default: return ResidentEmotion.Neutral;
            }
        }

        private static string Normalize(string value)
        {
            return value == null ? "" : value.Trim().ToLowerInvariant();
        }
    }
}
