using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TownReview.Core.City
{
    // レスポンスのJSONが読めなかったときに投げる例外。
    public class CitySnapshotFormatException : Exception
    {
        public CitySnapshotFormatException(string message, Exception inner = null)
            : base(message, inner)
        {
        }
    }

    // APIレスポンス（JSON文字列）を CitySnapshot に変換する。
    // 仕様では null を使わないことになっているが、サーバー側の不具合で null が来ても
    // Unity側が落ちないよう、読み込み後に既定値（"" や空リスト）へ置き換える。
    public static class CitySnapshotParser
    {
        private static readonly JsonSerializerSettings Settings = new()
        {
            // generatedAt（"2026-09-25T09:00:00Z"）を日付型に解釈させず、文字列のまま扱うための念のための設定。
            DateParseHandling = DateParseHandling.None,
            // 将来 effects などのフィールドが追加されても読めるよう、知らないフィールドは無視する。
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public static CitySnapshot Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new CitySnapshotFormatException("レスポンスが空です。");
            }

            CitySnapshot snapshot;
            try
            {
                snapshot = JsonConvert.DeserializeObject<CitySnapshot>(json, Settings);
            }
            catch (JsonException e)
            {
                throw new CitySnapshotFormatException($"JSONの形式が正しくありません: {e.Message}", e);
            }

            if (snapshot == null)
            {
                throw new CitySnapshotFormatException("JSONの中身が null です。");
            }

            Normalize(snapshot);
            return snapshot;
        }

        public static bool TryParse(string json, out CitySnapshot snapshot, out string errorMessage)
        {
            try
            {
                snapshot = Parse(json);
                errorMessage = "";
                return true;
            }
            catch (CitySnapshotFormatException e)
            {
                snapshot = null;
                errorMessage = e.Message;
                return false;
            }
        }

        // 400 / 404 のときのエラーレスポンスを読む。形式が違えば false。
        public static bool TryParseError(string json, out CityApiError error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            try
            {
                CityApiErrorResponse response = JsonConvert.DeserializeObject<CityApiErrorResponse>(json, Settings);
                if (response?.Error == null)
                {
                    return false;
                }

                error = response.Error;
                error.Code ??= "";
                error.Message ??= "";
                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static void Normalize(CitySnapshot snapshot)
        {
            snapshot.CityId ??= "";
            snapshot.CityName ??= "";
            snapshot.Mode ??= "";
            snapshot.GeneratedAt ??= "";

            snapshot.Bounds ??= new CityBounds();
            snapshot.Bounds.Min ??= new CityVector3();
            snapshot.Bounds.Max ??= new CityVector3();

            snapshot.Buildings = RemoveNulls(snapshot.Buildings);
            foreach (BuildingData building in snapshot.Buildings)
            {
                building.Id ??= "";
                building.Name ??= "";
                building.Category ??= "";
                building.Position ??= new CityVector3();
                building.Size ??= new CityVector3();
            }

            snapshot.Avatars = RemoveNulls(snapshot.Avatars);
            foreach (AvatarData avatar in snapshot.Avatars)
            {
                avatar.Id ??= "";
                avatar.BuildingId ??= "";
                avatar.Position ??= new CityVector3();
                avatar.Emotion ??= "";
                avatar.Voice ??= "";
                avatar.Comment ??= "";

                avatar.Appearance ??= new AvatarAppearance();
                avatar.Appearance.BodyType ??= "";
                avatar.Appearance.HairStyle ??= "";
                avatar.Appearance.HairColor ??= "";
                avatar.Appearance.Outfit ??= "";
            }
        }

        private static List<T> RemoveNulls<T>(List<T> list) where T : class
        {
            if (list == null)
            {
                return new List<T>();
            }

            list.RemoveAll(item => item == null);
            return list;
        }
    }
}
