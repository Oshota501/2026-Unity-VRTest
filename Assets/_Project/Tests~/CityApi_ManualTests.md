# 都市API（CitySnapshot）受け入れ テスト項目

## A. 自動テスト（Unityなしで実行できる）

UnityEngine を使わない部分（JSONの読み込み・URLの組み立て・仕様チェック・仮データ読み込み・.env の読み込み）は
`Assets/_Project/Tests~/CityApi.Tests` の NUnit テストで確認できる（79件）。

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
| DotEnvParser | KEY=VALUE、コメント・空行・CRLF、引用符、行末コメント、URL内の #、export、キーの重複 |

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
| C-1 | `.env` の `CITY_API_BASE_URL` を実サーバー（または C-6 の仮サーバー）にし、CityApiSettings の City Id = sample で再生 | B-1〜B-8 と同じ結果になる |
| C-2 | City Id を存在しない値（例 `nothing`）にする | エラー「HTTP 404 NOT_FOUND: city not found（URL）」、アプリは止まらない |
| C-3 | `.env` の `CITY_API_BASE_URL` を存在しないホスト（例 `https://invalid.example`）にする | エラー「通信に失敗しました: …」がタイムアウト秒以内に出る |
| C-4 | サーバーが mode=ideal に対して mode:"now" を返す | 警告「mode=ideal を要求しましたが、レスポンスの mode は "now" でした」 |
| C-5 | Now / Ideal をすばやく連続で切り替える | 最後に押した方の街だけが表示される（古いレスポンスで上書きされない） |
| C-6 | 本物のサーバーがまだない場合：ターミナルで `python3 "Assets/_Project/Tests~/mock_city_api.py"` を実行し、`.env` に `CITY_API_BASE_URL=http://localhost:8080` と書いて C-1〜C-5 を行う | C-1〜C-5 と同じ結果（仮サーバーは sample 以外の cityId に 404、不正な mode に 400 を返す） |
| C-7 | Quest 実機（Android）で C-1 | 同じ結果。※ http:// の URL は Android では既定でブロックされるため、実機は https:// のサーバーで確認する |
| C-8 | WebGL ビルドで C-1 | 同じ結果。※ サーバー側で CORS（Access-Control-Allow-Origin）の許可が必要 |
| C-9 | `.env` を消して（または `CITY_API_BASE_URL` の行を消して）再生 | エラー「APIのURLがありません。プロジェクト直下の .env に CITY_API_BASE_URL=https://... を書いてください…」、アプリは止まらない |
| C-10 | 再生中に `.env` の URL を書き換え、CitySnapshotLoader を右クリック → Reload | 新しい URL に接続する（エディタは読み込みのたびに .env を読み直す） |
| C-11 | `.env` がある状態で実機ビルドする | ビルド後 `Assets/_Project/Data/Generated/Resources/BakedEnv.txt` に `CITY_API_BASE_URL` の行だけが書かれている。`git status` に出てこない（.gitignore 済み） |

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
7. API に切り替える場合：
   - プロジェクト直下（`Assets` フォルダと同じ場所）の `.env.example` をコピーして `.env` という名前にし、`CITY_API_BASE_URL=` の後ろにAPIのURLを書く。
   - Project ウィンドウで `Assets/_Project/Data` を右クリック → Create → TownReview → City API Settings。できた `CityApiSettings` に City Id を入れ、`CitySnapshotLoader` の Source を `Api`、Api Settings にそのファイルを設定する。

## E. SampleScene（BuildingManager / PersonManager に反映）で確認する項目

準備：SampleScene に空のオブジェクト `CityApi`（Position (0, 0, 0)）を作り、`CitySnapshotLoader` と `CitySnapshotVisualizer` を付ける。
Loader の設定は D-4 と同じ。Visualizer の Building Manager / Person Manager に Hierarchy の `BuildingManager` / `PersonManager` をドラッグする。

| No | 手順 | 期待結果 |
|---|---|---|
| E-1 | Source = LocalJson、Mode = Now で再生 | Console に「建物 6 件 / 住民 5 人」と出る。エラー（赤）が出ない |
| E-2 | Hierarchy で `CityApi` > `ApiBuildings` を開く | `Building_b001_サンプルマート(Cube)` など6個がすべて `(Cube)` 付きで並ぶ（Buildings.asset に b001〜b006 という名前のプレハブがないため） |
| E-3 | b001 を選択して Inspector を見る | Position (20, 3, 40)、Rotation Y = 90、Scale (20, 6, 15) |
| E-4 | 任意のプレハブ（例 `Tohu`）を複製して名前を `b001` にし、Buildings.asset の View に追加して再生 | b001 だけがそのプレハブになり（名前に `(Cube)` が付かない）、b002〜b006 は Cube のまま |
| E-5 | `CityApi` > `ApiResidents` を開く | `Resident_a001`〜`Resident_a005` がある |
| E-6 | プレイヤーを Resident_a001（x=22, z=30 付近）に近づける | voice「夕方はレジが混む。朝は空いてる」が表示され、離れると消える。x が同じでも z が離れている住民は表示されない（文字が □ になる場合は、Person が作る TextMeshPro に日本語フォントが設定されていない） |
| E-7 | 再生中に CitySnapshotLoader の Mode を Ideal にして右クリック → Reload | `ApiBuildings` が7件、`ApiResidents` が6人に置き換わる（古いものが残らない） |
| E-8 | BuildingManager / PersonManager の Spawn All On Start をオフにして再生 | Buildings.asset / PersonRegistry.asset の登録物は出ず、APIの建物・住民だけが出る |
| E-9 | CitySnapshotVisualizer を外して再生 | Console にエラー「表示先がありません。…」が出る（例外で止まらない） |
