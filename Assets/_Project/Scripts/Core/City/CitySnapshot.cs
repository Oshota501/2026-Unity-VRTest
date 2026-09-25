using System.Collections.Generic;
using Newtonsoft.Json;

namespace TownReview.Core.City
{
    // GET /v1/cities/{cityId}/snapshot のレスポンス（都市の建物と住民のリスト）。
    //
    // このファイルは UnityEngine を参照しない純粋なC#にしている。
    // Unityの外（Assets/_Project/Tests~ のテストプロジェクト）からもそのままテストするため。
    // Unityの Vector3 への変換は CityBuilder 側で行う。
    //
    // 仕様書ではJsonUtility向けに書かれているが、このプロジェクトの方針どおり Newtonsoft Json で読み込む。
    // フィールドの初期値は「値がないとき」の既定値。null が来た場合も CitySnapshotParser が既定値に直す。
    public class CitySnapshot
    {
        [JsonProperty("cityId")]
        public string CityId = "";

        [JsonProperty("cityName")]
        public string CityName = "";

        // "now"（現在の都市）/ "ideal"（理想の都市）
        [JsonProperty("mode")]
        public string Mode = "";

        // データ生成日時（ISO 8601）。日付型に変換せず文字列のまま保持する。
        [JsonProperty("generatedAt")]
        public string GeneratedAt = "";

        // 都市の範囲（カメラの移動制限や地面の生成に使う）
        [JsonProperty("bounds")]
        public CityBounds Bounds = new();

        [JsonProperty("buildings")]
        public List<BuildingData> Buildings = new();

        [JsonProperty("avatars")]
        public List<AvatarData> Avatars = new();

        [JsonIgnore]
        public CityMode ModeType => CityEnumParser.ParseMode(Mode);
    }

    // APIの座標（メートル。x：東が正、y：上が正、z：北が正。Unityと同じ向き）。
    public class CityVector3
    {
        [JsonProperty("x")]
        public float X;

        [JsonProperty("y")]
        public float Y;

        [JsonProperty("z")]
        public float Z;

        public CityVector3()
        {
        }

        public CityVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString() => $"({X}, {Y}, {Z})";
    }

    public class CityBounds
    {
        [JsonProperty("min")]
        public CityVector3 Min = new();

        [JsonProperty("max")]
        public CityVector3 Max = new();
    }

    // 建物1件分。position は建物の「底面の中心」。
    public class BuildingData
    {
        [JsonProperty("id")]
        public string Id = "";

        // 住宅など名前を出さないものは ""
        [JsonProperty("name")]
        public string Name = "";

        // 建物の種類（house, apartment, supermarket ...）。未知の値は other として扱う。
        [JsonProperty("category")]
        public string Category = "";

        [JsonProperty("position")]
        public CityVector3 Position = new();

        // y軸まわりの向き（度）
        [JsonProperty("rotationY")]
        public float RotationY;

        // 幅(x)・高さ(y)・奥行き(z)
        [JsonProperty("size")]
        public CityVector3 Size = new();

        [JsonIgnore]
        public BuildingCategory CategoryType => CityEnumParser.ParseCategory(Category);
    }

    // 住民アバター1人分。position は足元の位置。
    public class AvatarData
    {
        [JsonProperty("id")]
        public string Id = "";

        // 関連する建物のID。建物に紐づかない場合は ""
        [JsonProperty("buildingId")]
        public string BuildingId = "";

        [JsonProperty("position")]
        public CityVector3 Position = new();

        [JsonProperty("rotationY")]
        public float RotationY;

        [JsonProperty("appearance")]
        public AvatarAppearance Appearance = new();

        // happy / neutral / annoyed / worried
        [JsonProperty("emotion")]
        public string Emotion = "";

        // 吹き出し用の短い一言（40文字以内）
        [JsonProperty("voice")]
        public string Voice = "";

        // コメント全文（触れたときの詳細表示用）
        [JsonProperty("comment")]
        public string Comment = "";

        [JsonIgnore]
        public ResidentEmotion EmotionType => CityEnumParser.ParseEmotion(Emotion);
    }

    // アバターの見た目。値とアセットの対応はUnity側で決める（未知の値は既定の見た目）。
    public class AvatarAppearance
    {
        [JsonProperty("bodyType")]
        public string BodyType = "";

        [JsonProperty("hairStyle")]
        public string HairStyle = "";

        // "#3B2A1A" 形式。Unity側で ColorUtility.TryParseHtmlString で変換する。
        [JsonProperty("hairColor")]
        public string HairColor = "";

        [JsonProperty("outfit")]
        public string Outfit = "";
    }

    // エラーレスポンス { "error": { "code": "NOT_FOUND", "message": "city not found" } }
    public class CityApiErrorResponse
    {
        [JsonProperty("error")]
        public CityApiError Error;
    }

    public class CityApiError
    {
        [JsonProperty("code")]
        public string Code = "";

        [JsonProperty("message")]
        public string Message = "";
    }
}
