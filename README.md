# Rurune Character MVP

Unity / C# による Windows 向け 3D キャラクターアプリの第1段階です。
購入モデル、アニメーション、VRChat SDK、シェーダーは含みません。
既存の Unity プロジェクトに導入するソース一式です。`ProjectSettings` と
`Packages/manifest.json` は含めていないため、このリポジトリ自体を Unity Hub で開く構成ではありません。

## 実装内容

- 右マウスドラッグで周回、ホイールでズーム、カメラリセット
- Inspector に登録した Animator ステートへのモーション切り替え
- 実在する BlendShape を選択する表情プリセットとリセット
- パッケージ追加不要の IMGUI 操作パネル
- `Tools > Rurune MVP > Create Starter Scene` による初期シーン生成

[構成・段階別計画](Docs/ImplementationPlan.md) / [Unity Editor 設定手順](Docs/UnitySetup.md) /
[手動検証項目と検証状況](Docs/Validation.md)

## 対応バージョン

提供された商品ページPDF（IKUSIA_JA.pdf、1ページ）の動作確認環境は **Unity 2022.3.6f1 / lilToon 1.7.3** です。
同ページは VRChat SDK Base / Avatars 3.5.2、VRChat Package Resolver Tool 0.1.28 も記載しています。
ただし3ページの v1.1.3 更新履歴には SDK 3.9.0 対応の記載があり、購入版の同梱資料も確認が必要です。
これらは商品側の記載であり、本MVPや単独Windowsアプリの動作確認結果ではありません。
詳細は [Unity設定手順](Docs/UnitySetup.md) を参照してください。
この環境には Unity Editor がなく、コンパイル、Play Mode、Windows ビルドは未検証です。

## 購入データの保護

モデル等は **`Assets/LocalOnly/` の下にのみ**導入してください。このディレクトリは Git 管理対象外です。
`.unitypackage` を読み込む前に、同梱ファイルの導入先を確認してください。
パッケージが別の固定パスを要求する場合は、導入前にそのパスを `.gitignore` に追加してください。
自動検出で購入データを完全に除外することはできません。コミット前に必ず差分を確認します。
購入モデルを含むシーンや Prefab も `Assets/LocalOnly/` に保存してください。

ふれあい、部屋での移動、デスクトップ透過・最前面化、AI 会話、音声入出力は今回の対象外です。
