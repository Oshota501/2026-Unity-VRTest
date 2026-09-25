using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TownReview.Core.Reviews;
using UnityEngine;

namespace TownReview.Infrastructure
{
    // 仮データ用のIReviewRepository実装。
    // Assets/_Project/Data/ 内のJSONをTextAssetとして受け取り読み込む。
    // WebGL/AndroidビルドではSystem.IOでファイルに直接アクセスできないため、
    // Inspectorで割り当てられるTextAssetとして持たせている。
    public class LocalReviewRepository : IReviewRepository
    {
        private readonly List<Review> allReviews;

        public LocalReviewRepository(TextAsset reviewsJson)
        {
            allReviews = JsonConvert.DeserializeObject<List<Review>>(reviewsJson.text) ?? new List<Review>();
        }

        public void GetReviews(string meshCode, Action<IReadOnlyList<Review>> onSuccess, Action<string> onError)
        {
            List<Review> result = allReviews.Where(r => r.MeshCode == meshCode).ToList();
            onSuccess(result);
        }
    }
}
