# まとめて設定・確認する手順

頻繁な画面確認を減らすための更新です。今すぐ実行する必要はありません。
モデル本体・購入モーションはGitへ追加せず、手元だけで利用します。

## A. ファイルを更新（1回）

1. Unityの再生を停止し、シーンを保存してEditorを終了。
2. プロジェクト全体を別のフォルダへバックアップ。
3. GitHubの `feature/rurune-mvp` ブランチから Code > Download ZIP で最新版を取得。
4. 解凍した `Assets/RuruneMVP` の**中身**を、手元の同名フォルダへコピーしてコードを更新。
   `RuruneMVP/RuruneMVP` の二重階層にしない。既存の `.meta` は削除しない。
   `Assets/LocalOnly`、購入モデルのフォルダ、既存シーンを置き換える必要はありません。
5. Unity 2022.3.22f1 でプロジェクトを開く。赤いコンパイルエラーがあれば先に記録する。

## B. モデルとカメラをまとめて設定

1. 作業中の `RuruneApp` シーンを開く。
2. Hierarchy で、購入モデル全体の親（確認済みの例は `rurune_kaihen`）を選択。
3. Tools > Rurune MVP > **Quick Setup** を開く。
4. **Use Hierarchy selection and discover references** を押す。
   名前や階層を推測せず、選んだモデル内と同じシーンにあるコンポーネントを検出します。
5. Animator / Orbit camera / Motion controller / Expression controller / Control panel が埋まったか確認。
   候補が複数ある場合は空欄になるので、使いたいものをHierarchyからドラッグしてください。
   既存シーンでは通常 MVP Systems、Main Camera、Control Panel が対象です。
6. 地面も調整する場合は、Ground欄へHierarchyのGroundをドラッグ。不要なら空欄のまま。
7. **1. Save local copy + connect + fit camera / ground** を押す。

`Assets/LocalOnly/Scenes/RuruneApp_Configured*.unity` に別名保存してから設定します。
元のシーンと購入Prefab・メッシュ・Controllerアセットは変更しません。
既存の `Close Eyes` などの表情設定は保持します。

カメラは有効なモデルメッシュの実測範囲を元に、注視点と距離を設定します。
地面はその時点のメッシュ最下端より少し下へ移動します。
Groundが非表示なら非表示のままなので、再表示したい場合は停止中にチェックを入れます。
姿勢・衣装・非表示切り替えで範囲が変わるため、すべてのモーションで接地を保証するものではありません。
大きな小物まで含むとモデルが小さく収まることがあります。

## C. 表情を追加（既存のClose Eyesだけなら省略）

Quick SetupのRendererに顔のSkinnedMeshRenderer（確認済みの例はBody）を指定。
Name filterで絞り込み、実在するBlendShapeを選択し、Button labelとWeightを入力。
**2. Append expression preset**で既存設定を残して追加します。
確認済みの目閉じは `b_blink`、100。すでにClose Eyesを登録していれば追加不要です。
形状の意味や適正なWeightは自動判定しません。複数Shapeの組み合わせは従来のInspectorで設定できます。

## D. モーションをまとめて設定

### 使用する全身モーションのクリップがある場合

1. Quick SetupのClipsを開いて件数を指定し、利用できるAnimationClipをProjectからドラッグ。
2. 先頭に待機用クリップを置きます。各クリップは実際のRigに適合するものを使います。
3. **3. Create and assign app controller from clips** を押す。

`Assets/LocalOnly/Animations/AppMotions*.controller` を新規作成し、選んだAnimatorに割り当てます。
各クリップのボタンも自動登録します。既存のモーションボタン一覧は置き換えます。
Controllerの元アセット・購入クリップ・Loop等のImport設定は変更しません。
最初のクリップをデフォルトにし、自動遷移は作りません。Apply Root Motionはオフになります。
元のControllerが担当していた衣装・表情等の初期化は、新しいControllerに自動移植されません。
Genericの別骨格、アバターマスク、クリップ内の位置カーブ等による不整合は別途確認が必要です。

### クリップがまだ分からない場合

**Alternative: list states from current controller** で実際のステートパスをボタン登録できます。
同期レイヤーは除外し、サブステートマシンとOverride Controllerに対応します。
これは内容を動作として保証する機能ではありません。VRChatのFXステートは、
全身の待機・ポーズ・ダンスとは限らず、既存の自動遷移がボタンの指定を上書きする場合もあります。
クリップがなければ今回はDを省略し、「モーション素材未選定」として他をまとめて確認できます。
モーションが動かなかった場合、Controllerの差し替えを繰り返すより、使ったクリップ名と結果を報告してください。

## E. 一度の再生でまとめて確認

GameのScaleは1xにして、次をまとめて試します。

- Fit character：その時点の姿勢で全体を収める。
- View height：高さスライダー。小さくするとモデルが画面の上へ。
- 中央ボタンを押しながらドラッグ：上下左右へ画面内の位置を調整。
- 右ドラッグ：周回、ホイール：ズーム、Reset camera：最後にフィットした初期状態へ。
- Close Eyes / Reset expression：目閉じと復元。
- 登録した各モーション：切り替わるか、位置や衣装が不自然に変わらないか。
- 地面を再表示した場合：足が隠れないか。

再生中に調整した高さ・位置・Fit結果は停止後に保存されません。
初期表示を保存するには停止中にQuick Setupの手順Bを実行してください。
Editor上の確認が終わったら [Windows起動手順](WindowsBuild.md) へ進めます。

## F. 結果は1回で報告

```text
取り込み：OK / エラーあり
自動カメラ調整・高さ・位置：OK / 気になる点
地面：表示してOK / 非表示のまま / 問題あり
表情：OK / 問題あり
モーション：OK / 素材未選定 / 問題あり（クリップ名と症状）
```

問題があったときだけ、Game画面とConsoleの赤いエラーをまとめて送ってください。
Quick Setupの **Write local diagnostic report** で構成のテキストも作れます。
`Assets/LocalOnly/Diagnostics/` に保存され、自動送信されません。
診断レポートはモデル名・Renderer名等を含むため、共有前に内容を確認してください。
資格情報、ユーザー名、絶対パスは記録せず、Console内容も収集しません。

## ボタンが表示されるのにAnimatorの警告が出る場合

停止中にHierarchyのMVP Systemsを選び、Character Motion ControllerのAnimatorへ
Hierarchyのモデル本体（Animator付き）をドラッグし、ControllerがAppMotionsであることを確認します。
Character Rootには同じモデルのTransformを指定し、保存後に再生して試します。
最新版では未割り当て・無効化・Controllerなしを区別して表示します。
Quick Setupの手順BまたはDを再実行した場合はCharacter Rootも自動登録されます。
無効な参照を復旧する際は指定ルート配下の一意な候補だけを使い、コンポーネントを強制有効化しません。
