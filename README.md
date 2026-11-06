# Oh! WAN — Unity プロトタイプ

添付仕様「Oh!椀」を基にした、1画面・2対2の料理対戦ゲームです。添付 `ProjectSettings.zip` の Unity 6 / URP プロジェクトを土台に、C#で実装しています。追加のサードパーティライブラリ・外部モデルは使っていません。

## 最初に遊ぶ

1. ZIPを展開します。**Assets、Packages、ProjectSettings のあるフォルダー**を Unity Hub の「Add project from disk」で追加します。
2. Unity **6000.3.27f1**（添付設定の指定バージョン）で開き、Package Managerの読み込みを待ちます。
3. `Assets/Scenes/SampleScene.unity` を開き、**Play** を押します。Gameビューは16:10または16:9、1440×900相当を推奨します。
4. Gameビューをクリックしてフォーカスし、`LET'S COOK!` または Enter で開始します。
5. 初期状態はP1が人間、P2〜P4がCPU。画面下の `CPU > HUMAN` / `HUMAN > CPU` で各枠を切り替えられます。全枠CPUの観戦も可能です。

シーンに手動でオブジェクトを配置する必要はありません。`OwanGame` が実行時にカメラ、フィールド、料理人、食材、お椀、UIを生成します。元のサンプルシーンは残しています。

## 操作

| 枠 | チーム | 移動 | 食材切替 | 投げる |
|---|---|---|---|---|
| P1 | 1（赤） | WASD | Q / E | Space |
| P2 | 1（赤） | 矢印 | , / . | 右Ctrl |
| P3 | 2（青緑） | IJKL | Y / O | H |
| P4 | 2（青緑） | テンキー8 / 5 / 4 / 6 | テンキー7 / 9 | テンキー0 |

- **P1はマウスカーソルの方向を狙います。** 投擲はSpace長押し。左クリックはUI用です。
- 他のキーボード枠は自分のレーンに向けてまっすぐ投げます。左右移動で狙う位置を合わせてください。
- 食材は画面下の色付きボタンでも選べます。料理人の近くにも4種類を表示します。
- Escape：一時停止 / 再開。F1：操作説明。Enter：開始 / 試合終了後の再戦。
- ゲームパッドは接続順にP1〜P4へ割り当てます。対応する枠をHUMANにしてください。
- パッド：左スティック移動、右スティック照準、南側フェイスボタンで投擲、十字キー上・右・下・左で食材1〜4、L/Rショルダーで切替。
- **Joy-Conの振り検出・単体持ち・ペアリングは未実装です。** OSとUnity Input Systemから通常の `Gamepad` として認識される機器が対象です。Nintendo Switch向けビルドではありません。

## 遊べるルール

- 120秒の試合。上の赤レーンは左→右、下の青緑レーンは右→左へ流れます。料理人は自陣内だけを移動します。
- P1/P3：Beef・Cutlet・Shrimp・Tuna。P2/P4：Onion・Egg・Nori・Salmon。どのレシピにも2人の協力が必要です。
- 牛丼2点、天丼2点、カツ丼3点、海鮮丼4点。注文に完全一致したお椀が口に届くと得点し、次の注文へ進みます。
- 食材不足・余分な食材・ご飯だけは `PEH!`。注文は変わりません。同じ食材は重ねて数えません。
- 投げた食材は最初に触れたお椀またはイッスンズへ当たります。お椀の間を通れば相手レーンにも届き、妨害できます。
- 緑 `+` は現在の注文に不要な食材を1種類ずつ除去。紫 `!` は食材を1種類ずつ食べます。
- どちらのイッスンズも投擲が当たると反対レーンへ移動。緑は紫が近づくと逃げます。移動直後は1.5秒の恐怖による再移動猶予があります。
- CPUは相方と分担し、移動中のお椀を予測して投げます。時々間違え、悪いイッスンズも狙います。
- おなかゲージは得点とともに増えます。勝敗は得点で判定し、同点は引き分けです。

## 数値を調整する

**`Assets/Resources/OwanTuning.json` を変更してからPlayを開始**してください。再戦も含め、そのPlay中は読み込んだ設定を使います。Play中の実験ではHierarchyの `Oh! WAN - Prototype` → `OwanGame` → `Tuning` でも変更できます。Play中のInspector変更は保存されません。

| 項目 | 意味 / 単位 |
|---|---|
| MatchSeconds | 試合時間（秒）。新しい試合に反映 |
| LaneSpeed / BowlInterval | お椀の速度（ワールド単位/秒）/ 出現間隔（秒） |
| PlayerSpeed | 移動速度（単位/秒） |
| ThrowSpeed / ThrowRange | 投擲速度（単位/秒）/ 最大飛距離 |
| ThrowCooldown | 連続投擲の間隔（秒） |
| BellyMaximum | おなかゲージ上限（得点） |
| GoodSpeed / GoodCleanSeconds | 緑の歩行速度 / 1種類の除去時間（秒） |
| FearDistance | 緑が紫から逃げる距離 |
| BadSpeed / BadEatSeconds | 紫の歩行速度 / 1種類を食べる時間（秒） |
| CpuReactionSeconds | CPUの判断間隔（秒）。小さいほど強い |
| CpuMistakeChance / CpuAttackChance | 間違い / 妨害を狙う確率（0〜1） |

速度・時間・距離・ゲージ上限は正の値にしてください。レシピ・得点・4種類ずつの配分は `Assets/Owan/Core/GameData.cs` の1か所にまとめています。C++の `Tuning.h` に相当する定義は `Tuning.cs` です。

## ファイルとレビュー箇所

- `Assets/Owan/Core/`：Unityに依存しない試合、採点、投擲、CPU、イッスンズ。`Match.cs` が試合全体を管理します。
- `Assets/Owan/Runtime/`：Unityの図形描画、UI、キーボード／パッド入力、自動起動。
- `Assets/Resources/OwanTuning.json`：調整用の初期値。
- `Assets/Owan/Editor/PrototypeBuild.cs`：メニュー `Oh Wan` からWindows/Linux開発ビルド。
- `Tests/`：外部テストパッケージ不要の.NET 8ルールテスト。
- `Assets/Scenes/`、`Assets/Settings/`、`Packages/`、`ProjectSettings/`：添付のUnityプロジェクト設定。

プログラマーには、食材配分と完全一致判定、NPCの転送・恐怖の猶予、CPUの予測投擲をレビューしてほしいです。プランナーには、お椀の間隔、NPCの作業時間、P1の照準の感触を確認してほしいです。

## ビルドと検証

Unity Hubで対象プラットフォームのBuild Supportを追加し、`Oh Wan > Build Windows Prototype` または `Build Linux Prototype` を実行します。出力先は `Builds/` です。Unity標準のBuild ProfilesでもSampleSceneを指定してビルドできます。

CLI例（Unity実行ファイルのパスは環境に合わせて置換）：

```sh
Unity -batchmode -quit -projectPath /path/to/project -executeMethod Owan.Editor.PrototypeBuild.Windows -logFile build.log
```

ロジックテストは.NET SDK 8で実行できます。

```sh
dotnet run --project Tests/Owan.Core.Tests.csproj
```

制作環境ではコアのコンパイルと13件の自動テストに成功。CPU4人の120秒試合で両チームの得点を確認（固定seed=42：15対23）しました。**Unity Editorは制作環境に存在しないため、Unityでのコンパイル、描画、実際のキーボード・ゲームパッド入力、実行ファイルのビルドは未確認です。** このZIPはUnityプロジェクトで、ビルド済みゲームではありません。

Unityでの確認順：

1. Play後、赤・青緑のレーンが反対方向に流れることを確認。
2. P1を動かし、Q/Eで選んだ食材がマウス方向へ投げられることを確認。
3. 全枠をCPUにして、`YUM!`、注文更新、スコア、おなかゲージを確認。
4. 緑・紫への投擲でレーンが切り替わること、相手レーンへの妨害を確認。
5. Escapeで時間が止まること、120秒後の結果と再戦を確認。

元のWindows実行ファイルはソースが付属していないため利用していません。添付文書のC++/DirectX・Common/再利用の制約は、今回選択されたUnity版には適用していません。
