# 都市API（CitySnapshot）受け入れ テスト項目

## A. 自動テスト（Unityなしで実行できる）

UnityEngine を使わない部分（JSONの読み込み・URLの組み立て・仕様チェック・仮データ読み込み）は
`Assets/_Project/Tests~/CityApi.Tests` の NUnit テストで確認できる（63件）。

```bash
# 事前に .NET 8 SDK をインストールしておく（Macなら https://dotnet.microsoft.com/download ）
dotnet test "Assets/_Project/Tests~/CityApi.Tests"
```

| 対象 | 主な確認内容 |
|---|---|
| CitySnapshotParser | サンプルJSONの全項目、generatedAt が文字列のまま、null → "" / 空リスト、未知フィールド（effects 等）を無視、壊れたJSON・HTMLは例外、404エラーJSONの読み込み |
| CityEnumParser | category 10種・emotion 4種・mode の変換、未知の値は other / neutral |
| CitySnapshotValidator | サンプルは警告0件、ID重複・存在しない buildingId・未知 category / emotion・voice 40文字超・size 0・bounds 逆転・不正な mode を警告 |
| CityApiEndpoints | 仕様書どおりのURL、末尾 / の除去、都市IDのエスケープ、空の値は例外 |
| LocalCitySnapshotRepository | now / ideal の切り替え、JSON未設定・壊れたJSONのエラー |

## B. Unityエディタで確認する項目（手動）

準備：「Unityエディタでの設定手順」（下の C）を済ませておく。

| No | 手順 | 期待結果 |
|---|---|---|
| B-1 | Source = LocalJson、Mode = Now で再生 | Console に「『サンプル市』（now）を読み込みました。建物 6 件 / 住民 5 人」と出る。警告（黄色）が出ない |
| B-2 | Hierarchy で `CityRoot` > `City_sample_now` を開く | Building_b001〜b006、Resident_a001〜a005 がある |
| B-3 | Sceneビューで建物を見る | 6個の箱が地面（y=0）の上に乗っている（半分埋まっていない）。b001（スーパー）は横長で90度回転、b006（公園）は薄い緑の板 |
| B-4 | b001 を選択して Inspector を見る | Position (20, 3, 40)、Rotation Y = 90、Scale (20, 6, 15)（Cubeは中心が原点なので y は高さの半分） |
| B-5 | 住民を見る | 5人のカプセルが立っている。色：a002/a004 黄（happy）、a001 灰（neutral）、a003 赤（annoyed）、a005 青（worried）。頭の色が髪色 |
| B-6 | 住民の頭上を見る | voice（例「夕方はレジが混む。朝は空いてる」）が表示され、カメラを回しても常にこちらを向く。文字化け（□）しない |
| B-7 | Resident_a001 の CityResident を右クリック → Test: Interact | 口コミパネルが開き、「■ 夕方はレジが混む…」「場所：サンプルマート（スーパー）」「気分：ふつう」とコメント全文が出る |
| B-8 | Resident_a005 で Test: Interact | 「場所」の行が出ない（buildingId が空のため） |
| B-9 | CitySnapshotLoader を右クリック → Reload | 街が作り直され、`City_sample_now` が1つだけ（重複しない） |
| B-10 | 再生中に Mode = Ideal にして Reload（またはボタンから LoadIdeal） | `City_sample_ideal` に置き換わり、建物 7 件 / 住民 6 人。a005 の吹き出しが「街灯が増えて夜も歩きやすい」 |
| B-11 | Now Json を空にして Ideal だけで再生 | Console にエラー「mode=now 用のJSONが設定されていません。」、街は生成されない（例外で止まらない） |
| B-12 | Generate Ground をオン | bounds の範囲（400m四方）に灰色の地面ができる |
| B-13 | CityRoot の位置を (100, 0, 0) にして再生 | 建物・住民がまとめて x+100 に移動する（PLATEAUとの位置合わせ用） |
| B-14 | Building Prefabs に Category = Supermarket のプレハブを1つ登録 | b001 だけがそのプレハブになり、他は仮の箱のまま |
| B-15 | 仮JSONの b001 の category を "castle" にして再生 | 黄色の警告「category "castle" は未知の値のため other として扱います」、灰色の箱で表示される |

## C. API（Source = Api）で確認する項目（サーバー完成後）

| No | 手順 | 期待結果 |
|---|---|---|
| C-1 | CityApiSettings の Base URL を実サーバー（または C-6 の仮サーバー）、City Id = sample で再生 | B-1〜B-8 と同じ結果になる |
| C-2 | City Id を存在しない値（例 `nothing`）にする | エラー「HTTP 404 NOT_FOUND: city not found（URL）」、アプリは止まらない |
| C-3 | Base URL を存在しないホスト（例 `https://invalid.example`）にする | エラー「通信に失敗しました: …」がタイムアウト秒以内に出る |
| C-4 | サーバーが mode=ideal に対して mode:"now" を返す | 警告「mode=ideal を要求しましたが、レスポンスの mode は "now" でした」 |
| C-5 | Now / Ideal をすばやく連続で切り替える | 最後に押した方の街だけが表示される（古いレスポンスで上書きされない） |
| C-6 | 本物のサーバーがまだない場合：ターミナルで `python3 "Assets/_Project/Tests~/mock_city_api.py"` を実行し、Base URL を `http://localhost:8080` にして C-1〜C-5 を行う | C-1〜C-5 と同じ結果（仮サーバーは sample 以外の cityId に 404、不正な mode に 400 を返す） |
| C-7 | Quest 実機（Android）で C-1 | 同じ結果。※ http:// の URL は Android では既定でブロックされるため、実機は https:// のサーバーで確認する |
| C-8 | WebGL ビルドで C-1 | 同じ結果。※ サーバー側で CORS（Access-Control-Allow-Origin）の許可が必要 |

## D. Unityエディタでの設定手順（テストの前準備）

B・C の確認の前に一度だけ行う。

1. `Town_Main` シーンを開く。
2. Hierarchy で右クリック → Create Empty。名前を `CityRoot` にし、Position を (0, 0, 0) にする。
3. `CityRoot` に Add Component → `CityBuilder`、続けて Add Component → `CitySnapshotLoader`。
4. `CitySnapshotLoader` の設定：
   - Source：`LocalJson`
   - Mode：`Now`
   - Now Json：`Assets/_Project/Data/SampleCitySnapshot_Now.json` をドラッグ
   - Ideal Json：`Assets/_Project/Data/SampleCitySnapshot_Ideal.json` をドラッグ
5. 吹き出しを出す場合：`CityRoot`（または UI用のオブジェクト）に Add Component → `ResidentVoiceBubbles`。Font に日本語入りの TMP フォントアセット（口コミパネルと同じもの）を設定する。
6. 口コミパネル（`ReviewPanel` が付いたオブジェクト）はそのままでよい（住民に触れたときのコメント表示に自動で対応している）。
7. API に切り替える場合：Project ウィンドウで `Assets/_Project/Data` を右クリック → Create → TownReview → City API Settings。できた `CityApiSettings` に Base URL / City Id を入れ、`CitySnapshotLoader` の Source を `Api`、Api Settings にそのファイルを設定する。

## E. SampleScene（Person を使った住民表示）で確認する項目

準備：プルリクエスト／作業説明の「SampleScene での設定手順」を済ませておく（`ApiPersonManager` に CitySnapshotLoader と ApiPersonVisualizer を付ける）。

| No | 手順 | 期待結果 |
|---|---|---|
| E-1 | `Assets/Scenes/SampleScene` を開いて再生 | Hierarchy の `ApiPersonManager` の下に `Person_a001`〜`Person_a005` ができる。Console に「建物 6 件 / 住民 5 人」。建物は作られない |
| E-2 | `Person_a001` を選択 | Position が (22, 0.5, 30)（Height Offset = 0.5 の場合）。Cube が床に半分埋まっていない |
| E-3 | WASD で `Person_a001` に 6m 以内まで近づく | Cube が少し浮き、頭上に「「夕方はレジが混む。朝は空いてる」」と、その下にコメント全文が20文字ごとに改行されて出る。文字がカメラの方を向く |
| E-4 | 6m より離れる | コメントが消え、Cube が元の高さに戻る |
| E-5 | 高い位置（Player の初期位置 y=10 など）から近づく | 高さの差に関係なく、水平距離 6m 以内で表示される |
| E-6 | Text Mode を Voice にして再生 | 一言だけ表示される |
| E-7 | 再生中に CitySnapshotLoader の Mode を Ideal にして右クリック → Reload | 住民が 6 人に置き換わる（古い住民が残らない）。a005 の一言が「街灯が増えて夜も歩きやすい」 |
| E-8 | 日本語フォントを設定していない状態で E-3 | 文字が □ になる（→ 設定手順の「日本語フォント」を行えば直る） |
| E-9 | Source = Api、仮APIサーバー（C-6）で E-1〜E-4 | 同じ結果 |
