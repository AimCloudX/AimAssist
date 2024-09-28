# プログラミングスニペット集
このファイルには、様々なプログラミングの便利なスニペットが含まれています。各スニペットは簡潔な形式で記述されています。
---
## aim
**Category:** Aim
```
AimNext
```
---
## Today
**Category:** DateTime
```powershell
{{Get-Date -Format "d"}}
```
---
## Now
**Category:** DateTime
```powershell
{{Get-Date -Format "HH:mm:ss"}}
```
---
## AppDataフォルダパス
**Category:** システムパス
```powershell
{{$env:APPDATA}}
```
---
## Downloadフォルダパス
**Category:** システムパス
```powershell
{{(New-Object -ComObject Shell.Application).Namespace('shell:Downloads').Self.Path}}
```
---
## 環境変数設定を開く
**Category:** システムコマンド
```
control.exe sysdm.cpl,,3
```
