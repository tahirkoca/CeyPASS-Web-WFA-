# WFA Build and Deploy

- **Tetikleyici:** `main` branch’e push (WFA veya bağımlı projeler değişince) veya Actions sekmesinden manuel (workflow_dispatch).
- **Runner:** Self-hosted, label: `cey-deploy` (CeyPASS-Local-Server).
- **Sürüm:** `CeyPASS.WFA/Properties/AssemblyInfo.cs` içindeki `AssemblyVersion` kullanılır; sürümü manuel güncelleyip push edin.

## Sunucu kopyalama klasörü (DEPLOY_PATH)

Zip ve `update.xml` dosyaları runner’ın çalıştığı makinede bir klasöre kopyalanır. Varsayılan: `C:\CeyPASS-Updates`.

Farklı bir path kullanacaksanız repo **Settings → Secrets and variables → Actions** içinde:

- **Name:** `DEPLOY_PATH`
- **Value:** Örn. `C:\inetpub\CeyPASS-Updates` (IIS ile sunuyorsanız buna benzer bir path)

Runner servisi (NETWORK SERVICE) bu klasöre yazabiliyor olmalı; gerekirse klasör izinlerini buna göre ayarlayın.
