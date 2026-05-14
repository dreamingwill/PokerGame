# PokerGame 项目说明

## 项目目标
开发一款支持好友在线开房同玩的德州扑克游戏。

## 目标平台
Android / iOS / PC

## 架构概览

### 服务端权威架构
所有游戏逻辑（发牌、判牌、押注结算）均在服务器执行，客户端只负责展示和传送玩家操作。

### 目录结构
- `PokerLogic/` — C# 核心算法原型（Cactus Kev 牌力评估）
- `PokerServer/` — Node.js 游戏服务器（Express + Socket.IO）

### 牌力评估算法
使用 **Cactus Kev + Paul Senzee 完美哈希**方案：
- 每张牌编码为一个 32 位整数
- 5 张牌评估走 Flushes / Unique5 / PerfectHash 三条路径
- 7 张牌枚举全部 21 种 C(7,5) 组合取最低分
- 分数范围 1（最强同花顺）～ 7462（最弱高牌），**分数越小牌越强**
- JS 版需用 `>>> 0` 保持无符号 32 位运算

## 当前进度
- [x] C# 牌力评估原型（支持 5 / 6 / 7 张）
- [x] Node.js 移植（PokerLogic.js + LookupTables.js）
- [x] Socket.IO 多房间基础联机（加入房间、发牌、判赢家广播）
- [x] 分阶段游戏流程（Preflop → Flop → Turn → River → Showdown）
- [x] 押注系统 v1（带入筹码、盲注、Fold/Check/Call/Bet/Raise、自动推进、底池/玩家筹码显示）
- [x] 平局分池处理（Showdown 同分玩家均分底池，余数给第一位赢家）
- [x] 前端视觉化（绿毡桌面、CSS 扑克牌、玩家座位环绕、底池居中、行动高亮、移动端响应式）
- [ ] 3+ 玩家支持（行动顺序、UTG 起手、真正边池）
- [ ] 重连/中途加入
- [ ] 正式客户端（Android / iOS / PC）

## v1 已知限制
- **仅支持双人 heads-up**：双人专属行动顺序
- **采用标准 heads-up 规则**：按钮位 = 小盲(SB)，preflop 先动；非按钮位 = 大盲(BB)，postflop 先动
- **无真正边池**：用"未跟注退还"近似处理 2 人 all-in 不同筹码深度场景，3+ 玩家不同 all-in 金额需要真正的边池系统
- **Bet vs Raise 已区分**：本街无人下注 → Bet（最小 BB）；已有下注 → Raise（须大于当前注）
- **牌局进行中禁止加入**：避免座位索引混乱
- **小盲/大盲写死 10/20**：将来按房间类型可配置（见下方"产品方向"）

## 产品方向（房间类型）
项目目标支持两种房间类型：
- **SNG（Sit-N-Go 单桌锦标赛）**：盲注按时间递增、玩家淘汰制，当前优先级。需配置盲注级别（如 10/20→20/40→…）、涨盲间隔（如 5min）、起始筹码、奖励结构
- **Cash 现金桌（9-max）**：盲注固定，玩家可随时带入/离开。次优先级

服务器房间状态需扩展 `roomType` / `blindLevels` / `currentLevel` / `levelStartTime` 等字段；现有写死的 SMALL_BLIND/BIG_BLIND 应改为从房间配置读取。

## 开发约定
- **JS 端（PokerServer/）是权威服务器逻辑**，C# 端仅作算法参考原型，新功能只需实现在 JS 端，无需保持双端同步。
- 游戏状态存储在服务器内存的 `roomGames` 对象中，Key 为房间号。
- Socket 事件命名使用 snake_case（如 `join_room`、`start_deal`）。
