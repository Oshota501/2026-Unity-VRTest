using System;
using System.Collections.Generic;

namespace TownReview.Core.Reviews
{
    // 口コミの取得方法を抽象化するインターフェース。
    // 仮データ用のLocalReviewRepositoryと、本番用のSupabaseReviewRepositoryを差し替えて使う。
    // 通信を伴う実装（Supabase）を想定し、結果はコールバックで返す。
    public interface IReviewRepository
    {
        // 指定した地域メッシュコードの口コミを取得する。
        void GetReviews(string meshCode, Action<IReadOnlyList<Review>> onSuccess, Action<string> onError);
    }
}
