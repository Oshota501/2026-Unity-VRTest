#!/usr/bin/env python3
"""都市API（GET /v1/cities/{cityId}/snapshot）の仮サーバー。

本物のサーバーができる前に、Unity の CitySnapshotLoader（Source = Api）の通信部分を確認するために使う。
Assets/_Project/Data/SampleCitySnapshot_Now.json / _Ideal.json をそのまま返す。

使い方（リポジトリのルートで）:
    python3 "Assets/_Project/Tests~/mock_city_api.py"            # http://localhost:8080
    python3 "Assets/_Project/Tests~/mock_city_api.py" --port 9000

  プロジェクト直下の .env に CITY_API_BASE_URL=http://localhost:8080 と書き、CityApiSettings の City Id を sample にする。

仕様どおりの応答:
    200  /v1/cities/sample/snapshot?mode=now|ideal（mode 省略時は now）
    400  mode が now / ideal 以外
    404  cityId が sample 以外、またはパスが違う
"""
import argparse
import json
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from urllib.parse import parse_qs, urlparse

DATA_DIR = Path(__file__).resolve().parent.parent / "Data"
FILES = {
    "now": DATA_DIR / "SampleCitySnapshot_Now.json",
    "ideal": DATA_DIR / "SampleCitySnapshot_Ideal.json",
}
CITY_ID = "sample"


class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        url = urlparse(self.path)
        parts = [p for p in url.path.split("/") if p]

        # /v1/cities/{cityId}/snapshot
        if len(parts) != 4 or parts[0] != "v1" or parts[1] != "cities" or parts[3] != "snapshot":
            return self.send_error_json(404, "NOT_FOUND", "not found")
        if parts[2] != CITY_ID:
            return self.send_error_json(404, "NOT_FOUND", "city not found")

        mode = parse_qs(url.query).get("mode", ["now"])[0]
        if mode not in FILES:
            return self.send_error_json(400, "BAD_REQUEST", "mode must be now or ideal")

        self.send_json(200, FILES[mode].read_bytes())

    def send_error_json(self, status, code, message):
        body = json.dumps({"error": {"code": code, "message": message}}).encode("utf-8")
        self.send_json(status, body)

    def send_json(self, status, body):
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(body)))
        # WebGL ビルドから呼べるようにする
        self.send_header("Access-Control-Allow-Origin", "*")
        self.end_headers()
        self.wfile.write(body)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--port", type=int, default=8080)
    args = parser.parse_args()
    server = ThreadingHTTPServer(("0.0.0.0", args.port), Handler)
    print(f"仮の都市APIを起動しました: http://localhost:{args.port}/v1/cities/{CITY_ID}/snapshot?mode=now （Ctrl+C で終了）")
    server.serve_forever()


if __name__ == "__main__":
    main()
