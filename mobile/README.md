# 德扑道场 · Android 薄壳（Capacitor）

这个目录是 Android App 的**薄壳**：APK 里几乎不含游戏代码，启动后直接加载
`https://pokerdojo.space` 的最新版。

## 更新逻辑（重要）
- **游戏内容更新** = 照旧 `deploy.sh` 推服务器即可，用户下次打开 App 自动最新，**无需重发 APK**。
- **只有改原生壳**（App 图标 / 名字 / 启动图、加原生插件、升 SDK/Capacitor）才需要重新出 APK。

## 出 APK（云端，零本地依赖）
GitHub Actions 已配置（`.github/workflows/android.yml`）：
- push 到本目录、或在 Actions 页面手动 **Run workflow**，云端自动构建；
- 完成后在该次运行的 **Artifacts** 里下载 `pokerdojo-debug-apk`（`app-debug.apk`）。
- debug APK 可直接侧载安装（手机需允许「安装未知来源应用」）。

## 本地构建（可选，需 Android SDK + JDK 17）
```bash
cd mobile
npm install
npx cap add android      # 首次生成 android/ 工程
npx cap sync android
cd android && ./gradlew assembleDebug
# 产物：android/app/build/outputs/apk/debug/app-debug.apk
```

## 配置
- `capacitor.config.json`：`server.url` 指向线上域名；`appId=space.pokerdojo.app`；`appName=德扑道场`。
- `www/index.html`：仅断网兜底页，正常不显示。
