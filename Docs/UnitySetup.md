# Unity Editor での手動設定

まとめて進める場合は [一括設定手順](BatchSetup.md) を参照してください。以下は個別設定の詳細です。

## 1. 最初に対応バージョンを確認する

提供資料：商品ページ https://booth.pm/ja/items/5957830 を印刷した `IKUSIA_JA.pdf`。
PDF本体や購入データはリポジトリへコピーしていません。

PDF 1ページの「導入方法」「注意事項」で確認できた動作確認環境:

|項目|商品側の記載|
|---|---|
|Unity|2022.3.6f1|
|シェーダー|lilToon 1.7.3|
|VRChat SDK Base / Avatars|各 3.5.2|
|VRChat Package Resolver Tool|0.1.28|

導入方法は Unity と VCC をインストールし、VCC で Avatar プロジェクトを作成、
シェーダーを導入後に unitypackage をインポートする流れです。これはVRChat向けの手順です。
本MVPは通常の Animator / SkinnedMeshRenderer を操作し、VRChat SDK 自体には直接依存しません。
ただし購入PrefabのコンポーネントがSDKを必要とする可能性があるため、同梱資料を優先します。

**版の違いに注意:** PDF 3ページの v1.1.3 更新履歴に SDK 3.9.0 対応の記載があります。
1ページの古い動作確認一覧から、現行購入版の必須SDKを3.5.2と断定しません。
手元のダウンロード版と同梱READMEを確認し、SDKを無条件にダウングレードしないでください。
PDFにある v1.1.4 の履歴も、ユーザーがその版を所持している証拠ではありません。

当面のEditor候補は明記された 2022.3.6f1 とし、購入版の同梱資料に別指定があれば再確認します。
レンダーパイプラインはこの資料の記載だけでは確定できません。
商品説明にはVRChat用途を想定し、他プログラムでの互換性を保証しない旨があります。
単独Windowsアプリでの材質・モーション等は別途検証が必要です。

PDF 2ページには顔の表情・調整用シェイプキー468個、MMD対応の記載がありますが、
個々のBlendShape名、Renderer階層、現在のモデル版の実数は実物で確認します。
利用規約本文と特記事項は外部リンクであり、このPDFだけではアプリへの組み込み・配布条件は未確認です。

Unity API の参照先（この環境では閲覧できず、公式記述との照合は未完了）:
- https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Animator.CrossFadeInFixedTime.html
- https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SkinnedMeshRenderer.SetBlendShapeWeight.html

## 2. ローカルプロジェクトを用意する

1. 購入モデルの指定に合う Unity Editor を Unity Hub からインストール。
2. Windows ビルドに必要な Build Support を追加し、指定パイプラインの3Dプロジェクトを作成。
3. 本リポジトリの `Assets/RuruneMVP` をプロジェクトの `Assets` にコピー。
   プロジェクト全体をGit管理する場合は本リポジトリの `.gitignore` も統合。
4. Project Settings > Player > Other Settings > Active Input Handling を
   **Input Manager (Old)** または **Both** にする。要求された場合は Editor を再起動。
   本MVPは旧 Input API を使用する。New Input System のみの構成は非対応。
5. Console のコンパイルエラーを確認。生成メニューが表示されない場合は先に解消する。

購入版の同梱資料とローカルでの検証前に、SDKやパッケージのバージョンを推測して固定しないでください。

## 3. モデルなしのシーンを生成

1. `Tools > Rurune MVP > Create Starter Scene` を実行。
2. 未保存のシーンがあれば確認ダイアログに従う。新しいシーンは
   `Assets/RuruneMVP/Scenes/MvpStarter.unity` に保存される。同名ファイルは上書きせず別名になる。
3. Play し、地面、UI、カメラの操作を確認。キャラクターと選択肢が空なのは初期状態として正常。
4. 生成シーンを `Assets/LocalOnly/Scenes/` に Save As してから購入モデルを置く。

## 4. 購入モデルをローカルで導入

1. `Assets/LocalOnly/` を作る。モデルと購入素材はここに置く。
2. unitypackage の Import ウィンドウでパスを確認。固定パスの依存アセットは
   **導入前にそのディレクトリを Git 除外**する。モデルの構造や導入先を勝手に変更せず提供元手順を優先。
3. 必要な依存関係を提供元の順序で導入。
4. 適切なモデル Prefab を `CharacterRoot` の子に置く。実物を見て向き・スケール・足元を調整。
5. Main Camera の OrbitCamera で Target が CharacterRoot であることを確認。
   Target Offset、距離、初期角度をモデルの大きさに合わせる。初期値はモデルの実寸を保証しない。
6. ピンクの材質等は対応シェーダー/パイプラインを確認。VRChat 固有機能が単独アプリでも
   動作するとは限らない。本MVPは VRChat Descriptor や Playable Layers を解釈しない。

## 5. Animator とモーション

1. モデル内の実際の Animator を確認。Humanoid / Generic と Avatar の有効性を Inspector で確認。
2. 購入 Prefab の Controller を直接書き換えず、アプリ用 Controller を作り Animator に割り当てる。
   購入クリップを参照する Controller は `Assets/LocalOnly/Animations/` に保存。
3. 利用許諾のあるクリップでステートを作成。待機をデフォルトに設定。
   Loop Time、足の位置、リターゲット可否を実際の Rig に合わせて確認。
4. 今回は位置固定。Animator の Apply Root Motion をオフにし、位置カーブのあるクリップは
   設定やクリップ側を確認。追加の自動遷移は、手動切り替えを打ち消さないように設定。
5. MVP Systems > CharacterMotionController の Animator にモデルの Animator を割り当て。
6. Motions の要素を追加し Label、State Path、Layer、Transition Seconds を設定。
   State Path は Animator ウィンドウで確認したレイヤー名を含む完全パス。
   例として `Base Layer.Idle` は形式の説明であり、モデルにその名前があるとは限らない。
7. Play してボタンを押す。存在しないパスは警告になり切り替えない。レイヤーの Weight も確認。

## 6. BlendShape と表情

1. モデル内の SkinnedMeshRenderer を実物で探す。顔のオブジェクト名や階層は決めつけない。
2. Renderer のメッシュにある BlendShapes を Inspector で確認し、提供元の説明と合わせて
   使う形状と適切な Weight を確認。重量範囲はモデル依存であり 0〜100 に固定していない。
3. MVP Systems > CharacterExpressionController の Expressions を追加。
4. Label、Shapes 配列を設定。各 Shapes に Renderer を割り当て、BlendShape のプルダウンから
   **存在する名前**を選ぶ。複数 Renderer と複数 Shape を1つの表情にまとめられる。
5. 同一表情内で同じ Renderer/Shape を重複登録しない。未割り当て、名前違い、空の表情は適用しない。
6. 表情ボタンで切り替える。以前の表情だけにある Shape は、最初に操作する直前の値に戻る。
   Reset expression またはコンポーネント無効化で操作した全 Shape を元の値に戻し、制御を終了。
7. Animator の表情カーブ、瞬き、リップシンク等との競合を確認。選択中の Shape は LateUpdate で
   上書きするため、それらとの自動合成はしない。メッシュを実行中に交換する運用も対象外。

Play Mode 中の Inspector 編集は終了時に失われます。停止した状態で設定し、ローカルシーンを保存してください。
UI は IMGUI の簡易パネルです。日本語の表示可否は環境のフォント依存なので、表示できない場合はラベルを英字にします。

## 7. Windows ビルド

1. Build Settings（Unityの版によっては Build Profiles）で Windows / x86_64 を選択。
2. 購入モデルを含む **ローカルシーン**を Scenes In Build に追加。
3. `Builds/Windows/` 等の Git 除外先へビルド。
4. 起動して Docs/Validation.md の項目を実施。通常のウィンドウアプリであり透過マスコットではない。
5. 配布前にモデル・モーション・シェーダー等の利用規約とデータ保護要件を確認。

## 8. Git に購入データを含めない確認

`git status --short --untracked-files=all` と `git diff --cached --name-only` で確認。
`git check-ignore Assets/LocalOnly/確認対象ファイル` で除外を確認できます。
購入データが既に追跡されている場合は .gitignore だけでは除外できません。
本MVPのテキストソース以外を一括追加せず、特にモデル入りシーン、Prefab、テクスチャ、
音声、アニメーション、購入アーカイブの混入を確認してください。
