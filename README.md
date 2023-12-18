# 20yymmdd_unity1week_template
unity1week用のテンプレート用

# Specification

## Unity version
- 2021.3.30f1

## Rendering
- Universal Rendering Pipeline 12

## Include package
- Text Mesh Pro
- UniTask

# Text Mesh Pro で日本語をつかう

1. Window -> TextMeshPro -> Font Asset Creater
2. Source Font File で、Fontsフォルダに入れたフォントファイルを設定する
3. CharacterSet をCustomCharacters に設定する。
4. Custom Character List に文字のファイルを貼り付ける。(japanese_full.txt)
5. GenerateFontAtlasを行う
6. 生成を確認してSaveする
7. 生成された .asset/.mat などは TextMeshPro/Resource/Font&Material に移動(Gitのサイズ制限を無視するため)
