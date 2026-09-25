using System;
using TownReview.Core.City;
using UnityEngine.Networking;

namespace TownReview.Infrastructure
{
    // 本番用の ICitySnapshotRepository 実装。GET /v1/cities/{cityId}/snapshot?mode=... を呼ぶ。
    // コルーチンやスレッドを使わず、UnityWebRequest の completed コールバックで結果を受け取る
    // （Quest・スマホ・WebGL のどれでも同じように動く）。
    public class HttpCitySnapshotRepository : ICitySnapshotRepository
    {
        private readonly string baseUrl;
        private readonly int timeoutSeconds;

        public HttpCitySnapshotRepository(string baseUrl, int timeoutSeconds = 10)
        {
            this.baseUrl = baseUrl;
            this.timeoutSeconds = timeoutSeconds;
        }

        public void GetSnapshot(string cityId, CityMode mode, Action<CitySnapshot> onSuccess, Action<string> onError)
        {
            string url;
            try
            {
                url = CityApiEndpoints.BuildSnapshotUrl(baseUrl, cityId, mode);
            }
            catch (ArgumentException e)
            {
                onError(e.Message);
                return;
            }

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = timeoutSeconds;
            request.SetRequestHeader("Accept", "application/json");
            request.SendWebRequest().completed += _ => OnCompleted(request, url, onSuccess, onError);
        }

        private static void OnCompleted(
            UnityWebRequest request,
            string url,
            Action<CitySnapshot> onSuccess,
            Action<string> onError)
        {
            // 結果を取り出したらすぐに破棄する（コールバック内で例外が出てもリクエストが残らないように）
            UnityWebRequest.Result result;
            long statusCode;
            string body;
            string requestError;
            using (request)
            {
                result = request.result;
                statusCode = request.responseCode;
                body = request.downloadHandler != null ? request.downloadHandler.text : "";
                requestError = request.error;
            }

            if (result == UnityWebRequest.Result.Success)
            {
                if (CitySnapshotParser.TryParse(body, out CitySnapshot snapshot, out string parseError))
                {
                    onSuccess(snapshot);
                }
                else
                {
                    onError($"レスポンスを読み込めませんでした（{url}）: {parseError}");
                }
                return;
            }

            if (result == UnityWebRequest.Result.ProtocolError)
            {
                // 400（mode が不正）/ 404（cityId が存在しない）など。エラー用JSONがあればその内容を出す。
                string detail = CitySnapshotParser.TryParseError(body, out CityApiError apiError)
                    ? $"{apiError.Code}: {apiError.Message}"
                    : requestError;
                onError($"HTTP {statusCode} {detail}（{url}）");
                return;
            }

            // 接続できない・タイムアウトなど
            onError($"通信に失敗しました: {requestError}（{url}）");
        }
    }
}
