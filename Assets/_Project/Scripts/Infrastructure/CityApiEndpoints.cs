using System;
using TownReview.Core.City;

namespace TownReview.Infrastructure
{
    // 都市APIのURLを組み立てる。UnityEngine を参照しない（Tests~ のテストから確認するため）。
    public static class CityApiEndpoints
    {
        // 例：BuildSnapshotUrl("https://api.example.com", "sample", CityMode.Now)
        //   → https://api.example.com/v1/cities/sample/snapshot?mode=now
        public static string BuildSnapshotUrl(string baseUrl, string cityId, CityMode mode)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new ArgumentException("ベースURLが空です。", nameof(baseUrl));
            }

            if (string.IsNullOrWhiteSpace(cityId))
            {
                throw new ArgumentException("都市IDが空です。", nameof(cityId));
            }

            string root = baseUrl.Trim().TrimEnd('/');
            string escapedCityId = Uri.EscapeDataString(cityId.Trim());
            return $"{root}/v1/cities/{escapedCityId}/snapshot?mode={CityEnumParser.ToApiValue(mode)}";
        }
    }
}
