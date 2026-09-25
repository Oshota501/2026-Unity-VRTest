# プロジェクト概要

遠方へ引っ越す人（主に新卒の大学生）が、住む前にその地域の実情（治安・騒音・交通量・雰囲気）を体験できるアプリ。
PLATEAUの3D都市モデルで街を再現し、街を歩く住民アバターに触れると、その地点の口コミが表示される。
昼・夜・休日などの時間帯を切り替えると、照明・環境音・表示される口コミが変わる。

# 開発者について

- 開発者はUnity初心者。コードを書いたら、Unityエディタ上で何をすればよいか（どのオブジェクトに何を付けるか、どこで何を設定するか）を必ず手順として日本語で説明すること。
- 説明・コメントは日本語で書くこと。

# 開発環境

- Unity 6 / URP（Universal Render Pipeline）
- 開発マシン：M4チップのMacBook（macOS）
- 最初のビルド先：Meta Quest（Android）
- 後から追加するビルド先：iPhone（iOS）、Web（WebGL）
- 入力：Input System（アクションで定義し、端末ごとに割り当てを変える）
- VR実装：OpenXR（`com.unity.xr.openxr`）＋ XR Interaction Toolkit（`com.unity.xr.interaction.toolkit`）＋ Unity OpenXR: Meta（`com.unity.xr.meta-openxr`）でMeta Questに対応する。Meta固有の独自SDK（Meta XR SDK / Oculus Integration）は使わない
- 3D都市データ：PLATEAU SDK for Unity
- バックエンド：Supabase（PostgreSQL + PostGIS）。Unityからは UnityWebRequest で REST API を呼ぶ
- JSON：Newtonsoft Json（com.unity.nuget.newtonsoft-json）。JsonUtility は配列を扱えないため使わない
- Firebase は WebGL に対応していないため使わない

# 設計方針

「街と口コミの本体」と「操作方法（VR・スマホ・Web）」を分ける。4つの層で構成する。

- Core：口コミ、エリア、アバター、時間帯などアプリの本体。VR・タッチ・マウスなど操作方法に関するコードを一切書かない。OpenXR や XR Interaction Toolkit など、XR関連の名前空間を参照しない。
- Infrastructure：外部との通信（Supabase など）。
- Platform：端末ごとのプレイヤー操作（VR / Desktop / Mobile / Web）。ここだけが端末ごとに異なる。
- UI：口コミパネル、時間帯切り替えボタンなどの表示。

重要なルール：
- 触れられるものは `IInteractable`（`Interact()` メソッドを持つ）を実装する。Platform 層がそれぞれの方法（VRコントローラー、タップ、クリック）で `Interact()` を呼ぶ。
- 口コミの取得は `IReviewRepository` 経由で行う。仮データ用の `LocalReviewRepository`（JSONファイルを読む）と本番用の `SupabaseReviewRepository` を差し替えられるようにする。
- 口コミはPLATEAUと同じ地域メッシュコードを持つ。表示中のエリアの口コミだけを取得して通信量を抑える。
- 口コミは評価項目を固定する（夜の治安、夜の騒音、休日の騒音、日中の交通量、生活の便利さを5段階）＋自由記述。時間帯（昼・夜・休日）を持つ。審査状態（審査中・承認・却下）を持ち、承認済みだけを表示する。
- 名前空間は `TownReview.Core`、`TownReview.Infrastructure`、`TownReview.Platform.VR`、`TownReview.UI` のように層ごとに分ける。

# フォルダ構成

自分たちで作るものはすべて `Assets/_Project/` の下に置く。

```
Assets/
├── _Project/
│   ├── Scenes/            Town_Main.unity（街のメインシーン）
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── Reviews/       Review.cs, IReviewRepository.cs, ReviewService.cs
│   │   │   ├── Area/          AreaManager.cs, MeshCode.cs
│   │   │   ├── City/          都市API（GET /v1/cities/{cityId}/snapshot）のデータ・読み込み・建物と住民の配置
│   │   │   ├── Avatars/       ResidentAvatar.cs, AvatarSpawner.cs
│   │   │   ├── Interaction/   IInteractable.cs
│   │   │   └── TimeOfDay/     TimeOfDayController.cs
│   │   ├── Infrastructure/    LocalReviewRepository.cs, SupabaseReviewRepository.cs, 都市APIの通信（CitySnapshotLoader など）
│   │   ├── Platform/
│   │   │   ├── VR/
│   │   │   ├── Desktop/       （エディタ確認用・将来のWeb版デスクトップ操作の下地）
│   │   │   ├── Mobile/        （後で追加）
│   │   │   └── Web/           （後で追加）
│   │   └── UI/                ReviewPanel.cs など
│   ├── Prefabs/           Rigs/, Avatars/, UI/
│   ├── Input/             Input System のアクション定義
│   ├── Art/               Materials, Models, Textures, Audio
│   ├── CityData/          PLATEAUで取り込んだ街のデータ
│   ├── Data/              設定ファイル、仮の口コミJSONなど
│   ├── Settings/          URPの設定など
│   └── Tests~/            Unityの外で動かすテスト（dotnet test）とテスト項目。「~」で終わるフォルダはUnityが読み込まない
└── （SDKやサンプル）      触らない
```

# Unityで作業するときの禁止事項・注意事項

- `.unity`（シーン）、`.prefab`、`.asset`、`.meta` ファイルを直接編集しない。シーンやプレハブの変更が必要な場合は、エディタでの操作手順を説明する。
- 既存ファイルの移動・名前変更をしない。必要な場合は、Unityエディタ上で行う手順を説明する（外部で動かすと `.meta` との対応が壊れるため）。
- 新しい `.cs` ファイルの作成は可。`.meta` は Unity が自動生成するので作らない。
- MonoBehaviour を継承するクラスは、ファイル名とクラス名を必ず一致させる。
- `Library/`、`Temp/`、`Logs/`、`UserSettings/`、`obj/` には触らない。
- `Assets/_Project/` の外（SDK、自動生成フォルダ、`Packages/` 内のパッケージ本体）のファイルを編集しない。パッケージの追加が必要な場合は理由と手順を説明する。
- APIキーなどの秘密情報をコードに直接書かない。設定は ScriptableObject など `Data/` の設定ファイルに分ける。
- Quest・スマホ・WebGL すべてで動くコードを書く（スレッドを使わない、`System.IO` によるファイル読み込みに頼らない、など WebGL の制約に注意）。
- パフォーマンスは Quest と WebGL を基準にする（毎フレームの重い処理や不要な割り当てを避ける）。

# 開発の段階

1. 準備：`.gitignore`（Unity用）、Git LFS、フォルダ構成の作成
2. Quest で最小限の体験：PLATEAUの街を1メッシュ表示し、仮の口コミ（JSON）を持ったアバターに触れると口コミパネルが出る
3. Supabase を作って接続（`SupabaseReviewRepository` に差し替え）
4. 時間帯と環境音の切り替え
5. スマホ用・Web用の操作を追加してビルド

現在の段階：1〜2
