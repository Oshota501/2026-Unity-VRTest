using System;

namespace TownReview.Core.City
{
    // 都市データ（建物・住民）の取得方法を抽象化するインターフェース。
    // 仮データ用の LocalCitySnapshotRepository（JSONを読む）と、
    // 本番用の HttpCitySnapshotRepository（APIを呼ぶ）を差し替えて使う。
    // 通信を伴うため、結果はコールバックで返す。
    public interface ICitySnapshotRepository
    {
        void GetSnapshot(string cityId, CityMode mode, Action<CitySnapshot> onSuccess, Action<string> onError);
    }
}
