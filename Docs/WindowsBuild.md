# 次のまとめ確認：Windowsアプリとして起動

## 現在の状態

Editorでモデル・カメラ・表情・モーションボタンによる再生が確認された段階です。
`hover/1/idle` はユーザーの確認で姿勢が不自然でした。原因は未特定です。
ステートへ切り替えられることと、クリップの姿勢がアプリ用途に適切なことは別です。
このクリップをダンス・汎用待機として合格扱いにせず、今回は保留します。
Windows上のビルドと実行は未検証です。

## 1. 更新

Unityの再生を停止して保存し、Editorを終了します。
いつものGitHub `feature/rurune-mvp` からCode > Download ZIPで最新版を取得し、
`Assets/RuruneMVP` の中身を既存の同名フォルダへ上書きします。
既存.meta、LocalOnly、購入モデルを削除しないでください。
バックアップを保持し、Unity 2022.3.22f1で開きます。

## 2. idleは当面使わない

動作が正常なpetanを使用して確認します。idleは押さなくても構いません。
ボタンからも除外したい場合は、停止中にHierarchyのMVP Systemsを選び、
Character Motion Controller > Motionsでidleの要素だけを削除します。
購入クリップやAnimator Controller内のステートは削除不要です。
既存Controllerを作り直す必要もありません。

## 3. Windows向けにビルド

1. 設定済みの `Assets/LocalOnly/Scenes/RuruneApp_Configured*.unity` を開きます。
   最も新しい設定を保存したシーンを使います。新規シーン生成は不要です。
2. 再生は停止したまま、Tools > Rurune MVP > **Build Windows App** を押します。
3. 完了まで待ちます。初回のビルドには時間がかかります。
4. 成功すると出力フォルダが開きます。

出力先はプロジェクトの `Builds/Windows/Mvp_日時/` です。
Windows x86_64 の開発用ビルドです。同じ出力フォルダを上書きせず、新規フォルダを使います。
ビルド対象シーンは今開いているもの1つだけに限定し、Build Settingsのシーン一覧は書き換えません。
ただしResources、StreamingAssets、参照する素材等は通常のUnityビルドルールに従って含まれます。
製品名・解像度・全画面等のPlayer Settingsは現在のプロジェクト設定を使用します。
Unityがビルドターゲットを切り替える場合があります。

Windows Build Supportがないというエラーの場合は、Unity Hub > Installs >
2022.3.22f1の歯車 > Add modulesから使用中のScripting Backendに必要なモジュールを追加します。
IL2CPPを使う設定ならVisual StudioのC++ツール等も必要になる場合があります。
既存のScripting BackendやSDKを闇雲に変更せず、エラー文を確認します。

## 4. 一度の起動で確認

出力先の **RuruneApp.exe** をダブルクリックします。
EXEだけを取り出さず、`RuruneApp_Data`、UnityPlayer.dll等と同じ場所に置いたまま起動します。

- モデルと衣装が表示される。
- カメラの回転・ズーム・高さ調整・Fitが使える。
- Close EyesとReset expressionが使える。
- petanが再生される。idleの姿勢は今回の合否から除外。
- 終了はWindowsの閉じるボタン、またはAlt+F4。

通常のWindowsアプリを起動する段階であり、デスクトップ透過・最前面固定は未実装です。
EditorのGame ScaleはEXEには適用されません。EXE内の表示や衣装がEditorと異なる場合は記録します。
VRChat専用コンポーネントやEditor専用のビルド処理が通常ビルドでも動くかは未確認です。

## 5. 報告する内容

```text
ビルド：成功 / 失敗（最初の赤いエラー）
EXE起動：OK / 問題あり
モデル・衣装：OK / 問題あり
カメラ：OK / 問題あり
表情：OK / 問題あり
petan：OK / 問題あり
```

失敗した場合だけConsoleの赤いエラーをまとめて送ってください。
出力フォルダのBuildResult.txtはビルド結果の要約で、詳細なConsoleログは含みません。
EXE実行後の問題ならUnity Player.logが必要になる場合がありますが、まず症状をまとめれば十分です。

ビルドには購入モデル等が含まれます。自分のPCでの動作確認用に扱い、Gitへ追加しないでください。
リポジトリの.gitignoreはBuildsを除外していますが、ローカルプロジェクト側にも除外が必要です。
外部配布・アップロードの可否は利用規約本文と特記事項を別途確認する必要があります。
