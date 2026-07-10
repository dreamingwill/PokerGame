# App 图标 / 启动图素材

把 **1024×1024（或更大）正方形 PNG** 命名为 `logo.png` 放在本目录，CI 会用
`@capacitor/assets` 自动生成 Android 各密度图标 + 自适应图标 + 启动图。

- 有 `logo.png` → 用它生成图标和启动图（居中放在深绿背景上）。
- 没有 `logo.png` → 沿用 Capacitor 默认图标（构建不会失败）。

背景色在 `.github/workflows/android.yml` 里用 `--iconBackgroundColor` 设定，
与德扑道场深绿主题一致。换图标只需替换 `logo.png` 后重新触发一次构建。
