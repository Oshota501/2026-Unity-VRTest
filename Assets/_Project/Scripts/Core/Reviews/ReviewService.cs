using System;
using System.Collections.Generic;
using System.Linq;

namespace TownReview.Core.Reviews
{
    // 口コミ取得のビジネスロジックを扱う。
    // IReviewRepositoryが返した結果から、承認済み（Approved）の口コミだけを呼び出し元に渡す。
    public class ReviewService
    {
        private readonly IReviewRepository repository;

        public ReviewService(IReviewRepository repository)
        {
            this.repository = repository;
        }

        // 指定エリアの承認済み口コミを取得する。
        // timeOfDayを指定すると、その時間帯の口コミだけにさらに絞り込む。
        public void GetApprovedReviews(
            string meshCode,
            TimeOfDay? timeOfDay,
            Action<IReadOnlyList<Review>> onSuccess,
            Action<string> onError)
        {
            repository.GetReviews(
                meshCode,
                reviews =>
                {
                    IEnumerable<Review> approved = reviews.Where(r => r.Status == ReviewStatus.Approved);
                    if (timeOfDay.HasValue)
                    {
                        approved = approved.Where(r => r.TimeOfDay == timeOfDay.Value);
                    }
                    onSuccess(approved.ToList());
                },
                onError);
        }
    }
}
