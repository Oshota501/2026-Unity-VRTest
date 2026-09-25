using System.Collections.Generic;

namespace TownReview.Core.City
{
    // 読み込んだ CitySnapshot が仕様どおりかを確認し、気になる点を警告文のリストで返す。
    // 警告があっても表示はできる（未知の category は other として表示するなど）。
    // サーバー側の実装確認やデバッグ用に、CitySnapshotLoader がコンソールに出力する。
    public static class CitySnapshotValidator
    {
        public const int MaxVoiceLength = 40;

        public static List<string> Validate(CitySnapshot snapshot)
        {
            var warnings = new List<string>();
            if (snapshot == null)
            {
                warnings.Add("snapshot が null です。");
                return warnings;
            }

            if (snapshot.CityId == "")
            {
                warnings.Add("cityId が空です。");
            }

            if (!CityEnumParser.IsValidMode(snapshot.Mode))
            {
                warnings.Add($"mode \"{snapshot.Mode}\" は now / ideal のどちらでもありません。");
            }

            CityVector3 min = snapshot.Bounds.Min;
            CityVector3 max = snapshot.Bounds.Max;
            if (min.X > max.X || min.Y > max.Y || min.Z > max.Z)
            {
                warnings.Add($"bounds の min {min} が max {max} より大きい成分を持っています。");
            }

            var buildingIds = new HashSet<string>();
            foreach (BuildingData building in snapshot.Buildings)
            {
                if (building.Id == "")
                {
                    warnings.Add("id が空の建物があります。");
                }
                else if (!buildingIds.Add(building.Id))
                {
                    warnings.Add($"建物ID \"{building.Id}\" が重複しています。");
                }

                if (!CityEnumParser.IsKnownCategory(building.Category))
                {
                    warnings.Add($"建物 \"{building.Id}\" の category \"{building.Category}\" は未知の値のため other として扱います。");
                }

                if (building.Size.X <= 0f || building.Size.Y <= 0f || building.Size.Z <= 0f)
                {
                    warnings.Add($"建物 \"{building.Id}\" の size {building.Size} に0以下の値があります。");
                }
            }

            var avatarIds = new HashSet<string>();
            foreach (AvatarData avatar in snapshot.Avatars)
            {
                if (avatar.Id == "")
                {
                    warnings.Add("id が空のアバターがあります。");
                }
                else if (!avatarIds.Add(avatar.Id))
                {
                    warnings.Add($"アバターID \"{avatar.Id}\" が重複しています。");
                }

                if (avatar.BuildingId != "" && !buildingIds.Contains(avatar.BuildingId))
                {
                    warnings.Add($"アバター \"{avatar.Id}\" の buildingId \"{avatar.BuildingId}\" に対応する建物がありません。");
                }

                if (!CityEnumParser.IsKnownEmotion(avatar.Emotion))
                {
                    warnings.Add($"アバター \"{avatar.Id}\" の emotion \"{avatar.Emotion}\" は未知の値のため neutral として扱います。");
                }

                if (avatar.Voice.Length > MaxVoiceLength)
                {
                    warnings.Add($"アバター \"{avatar.Id}\" の voice が{MaxVoiceLength}文字を超えています（{avatar.Voice.Length}文字）。");
                }
            }

            return warnings;
        }
    }
}
