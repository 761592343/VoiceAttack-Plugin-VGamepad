# VoiceAttack Plugin VGamepad

**适用于 VoiceAttack 2**（v1.6+，Plugin Interface v4）。不兼容 VoiceAttack 一代。

一个 VoiceAttack 2 插件，在系统中创建**完整的虚拟 Xbox 360 控制器**。通过 ViGEmBus 驱动生成原生 XInput 设备，任何支持手柄的游戏都能直接识别，无需额外转换。

```
语音 → VoiceAttack 2 → 本插件 → ViGEmBus(驱动) → 虚拟 Xbox 360 手柄 → 游戏
```

对比其他方案：

| | vJoy + XOutput | reWASD | 本插件 |
|---|---|---|---|
| 安装量 | 两个软件 | 一个商业软件 | 一个开源驱动 |
| 每次启动 | 手动开 XOutput | 自动 | 驱动自启，零操作 |
| 设备协议 | DirectInput → 转 XInput | XInput | **原生 XInput** |
| 侵入性 | 低 | 高（HOOK 层） | 低（标准驱动） |
| 费用 | 免费 | ~$7 | 免费 |
| 控制粒度 | 按钮+轴 | 全功能 | **全部按钮 / 扳机 / 摇杆 / 十字键** |

---

## 安装（推荐：直接下载 Release）

> 适合普通用户。无需安装 .NET SDK，无需编译。

### 第一步：安装 ViGEmBus 驱动（只需一次）

1. 下载：https://github.com/nefarius/ViGEmBus/releases/latest
2. 安装 `ViGEmBus_Setup_x64.msi`
3. **重启电脑**

> ViGEmBus 是开源虚拟手柄总线驱动。安装后开机自启，没有界面，没有广告。

### 第二步：下载并放置插件

1. 从 [Releases](https://github.com/761592343/VoiceAttack-Plugin-VGamepad/releases) 下载 `VoiceAttack-Plugin-VGamepad-v1.0.0.zip`
2. 解压，得到三个文件：
   ```
   VoiceAttack-Plugin-VGamepad.dll
   Nefarius.ViGEm.Client.dll
   README.md
   ```
3. 在 `%APPDATA%\VoiceAttack2\Apps\` 下新建文件夹 `VoiceAttack-Plugin-VGamepad`
4. 把三个文件全部复制进去

   最终目录结构：
   ```
   %APPDATA%\VoiceAttack2\Apps\VoiceAttack-Plugin-VGamepad\
   ├── VoiceAttack-Plugin-VGamepad.dll
   ├── Nefarius.ViGEm.Client.dll
   └── README.md
   ```

### 第三步：在 VoiceAttack 2 中启用插件

1. 打开 VoiceAttack 2
2. **Options → General → 勾选 "Enable Plugin Support"**
3. 重启 VoiceAttack 2
4. 检查日志，应该看到绿色信息：

   ```
   [VGamepad] Virtual Xbox 360 controller connected.
   ```

5. 验证：Win+R → 输入 `joy.cpl` → 应该能看到 "Xbox 360 Controller for Windows"

### 第四步：优化语音识别（建议）

VoiceAttack 2 → **Options → Recognition**：

| 设置项 | 推荐值 | 说明 |
|--------|--------|------|
| Minimum Confidence Level | **50** | 太低误触发，太高识别不到（如 57/75 被拒就说明要降） |
| Command Weight | 默认 | 有多个相似指令时调优先级 |

另外建议在 Windows 设置 → 语音 → 训练语音引擎，读几段话提升识别率。

---

## 指令参考

每条指令格式：`KEY:VALUE`

多条指令用 **分号** 分隔：`RT:255;A:1;HOLD:100;A:0;RT:0`

### 全部按钮

| 指令 | 对应 Xbox 按钮 | 取值 |
|------|---------------|------|
| `A:1` `A:0` | A 键 | 1=按下, 0=释放 |
| `B:1` `B:0` | B 键 | 同上 |
| `X:1` `X:0` | X 键 | 同上 |
| `Y:1` `Y:0` | Y 键 | 同上 |
| `LB:1` `LB:0` | 左肩键 (Left Bumper) | 同上 |
| `RB:1` `RB:0` | 右肩键 (Right Bumper) | 同上 |
| `LS:1` `LS:0` | 左摇杆按下 (Left Stick) | 同上 |
| `RS:1` `RS:0` | 右摇杆按下 (Right Stick) | 同上 |
| `START:1` `START:0` | Start 键 | 同上 |
| `BACK:1` `BACK:0` | Back 键 | 同上 |
| `UP:1` `UP:0` | 十字键 上 | 同上 |
| `DOWN:1` `DOWN:0` | 十字键 下 | 同上 |
| `LEFT:1` `LEFT:0` | 十字键 左 | 同上 |
| `RIGHT:1` `RIGHT:0` | 十字键 右 | 同上 |

### 扳机键（0-255）

| 指令 | 对应 | 示例 |
|------|------|------|
| `RT:255` | 右扳机 (Right Trigger) | `RT:0` = 释放, `RT:128` = 半程, `RT:255` = 到底 |
| `LT:255` | 左扳机 (Left Trigger) | 同上 |

> 扳机是线性输入。0 = 没按，255 = 按到底。

### 摇杆（-32768 ~ 32767）

0 = 居中，负 = 左/上，正 = 右/下。

| 指令 | 对应 | 示例 |
|------|------|------|
| `LX:0` | 左摇杆 X 轴 | `LX:-16384` = 左半, `LX:16384` = 右半 |
| `LY:0` | 左摇杆 Y 轴 | `LY:16384` = 前推（上）, `LY:-16384` = 后拉（下） |
| `RX:0` | 右摇杆 X 轴 | 同上 |
| `RY:0` | 右摇杆 Y 轴 | 同上 |

### 控制指令

| 指令 | 说明 |
|------|------|
| `RELEASE` | 释放所有按钮、扳机、摇杆（紧急停止/重置） |
| `HOLD:100` | 暂停 100 毫秒，同时设为默认按住时长 |
| `DELAY:100` | 暂停 100 毫秒，同时设为默认延迟 |

> `HOLD:n` 和 `DELAY:n` 实际效果都是暂停 n 毫秒。在多指令链中用于控制动作时序。

---

## VoiceAttack 2 命令配置

### 添加插件的通用步骤

1. 新建或编辑一个命令（"When I say: xxx"）
2. 添加动作：**Other → VoiceAttack Action → Execute an External Plugin Function**
3. 在 "Plugin" 下拉选择 **"VGamepad (Virtual Xbox 360)"**
4. 在 "Context" 框输入指令
5. 勾选 **"Wait until the function completes"**

> 不勾选 Wait 会导致异步执行、时序错乱。除非有特殊需求，一律勾选。

---

## 实战示例

### 霍格沃兹之遗：语音念咒施法

游戏机制：释放咒语 = RT（右扳机）+ A / B / X / Y（脸键）。

#### 方案 A：分步动作（清晰，推荐入门）

在 VoiceAttack 2 命令中放 7 个动作：

| 顺序 | 动作类型 | 设置 |
|------|----------|------|
| 1 | Execute Plugin | Context = `RT:255`，勾选 Wait |
| 2 | Pause | 0.05 秒 |
| 3 | Execute Plugin | Context = `A:1`，勾选 Wait |
| 4 | Pause | 0.10 秒 |
| 5 | Execute Plugin | Context = `A:0`，勾选 Wait |
| 6 | Pause | 0.05 秒 |
| 7 | Execute Plugin | Context = `RT:0`，勾选 Wait |

换咒语只需改第 3、5 步的 `A` → `B` / `X` / `Y`。

#### 方案 B：一行指令（简洁，推荐熟练后）

只放一个动作：

```
Context = RT:255;HOLD:50;A:1;HOLD:100;A:0;HOLD:50;RT:0
```

时序解析：
```
RT:255      ← 按下右扳机
HOLD:50     ← 等 50ms（让游戏检测到扳机按下）
A:1         ← 按下 A 键
HOLD:100    ← 按住 A 100ms
A:0         ← 释放 A
HOLD:50     ← 等 50ms
RT:0        ← 释放右扳机
```

> 如果游戏没反应，把 `HOLD` 值调大（试 150、200）。如果太慢，调小（试 60、80）。

#### 咒语按钮对照

霍格沃兹之遗默认键位（Xbox 手柄）：

| 咒语槽 | 按键 | 指令 |
|--------|------|------|
| 第 1 个 | A | `A:1` / `A:0` |
| 第 2 个 | B | `B:1` / `B:0` |
| 第 3 个 | X | `X:1` / `X:0` |
| 第 4 个 | Y | `Y:1` / `Y:0` |

### 其他游戏示例

模拟跳跃（A 键轻点）：
```
A:1;HOLD:80;A:0
```

长按 RB 使用技能：
```
RB:1;HOLD:2000;RB:0
```

同时按 LT + RT：
```
LT:255;RT:255
```

左摇杆前推 2 秒：
```
LY:16384;HOLD:2000;LY:0
```

连续按 A 三次：
```
A:1;HOLD:80;A:0;HOLD:80;A:1;HOLD:80;A:0;HOLD:80;A:1;HOLD:80;A:0
```

紧急停止：
```
RELEASE
```

---

## 故障排查

### 插件加载失败（红字 "FAILED to connect"）

- ViGEmBus 驱动没装或没启动 → 重新安装并重启
- 检查 DLL 是否在正确目录 `%APPDATA%\VoiceAttack2\Apps\VoiceAttack-Plugin-VGamepad\`
- 检查 Enable Plugin Support 是否勾选

### 游戏不响应虚拟手柄

- Win+R → `joy.cpl` → 确认 "Xbox 360 Controller for Windows" 存在
- 有些游戏需要在设置里手动选择输入设备

### 物理手柄冲突

一般不影响，Windows 支持多手柄。如果游戏只认物理手柄：
- 用 [HidHide](https://github.com/nefarius/HidHide) 隐藏物理手柄的 XInput 接口
- 或者在游戏设置里切换控制器优先级

### 语音识别被拒

日志显示 `'xxx' recognized, but rejected with confidence level XX/YY`：
- VoiceAttack 2 → Options → Recognition → 降低 **Minimum Confidence Level**（推荐 50）
- 训练 Windows 语音引擎
- 给命令多加几个别名

### 指令执行了但游戏没反应

- 增加延迟：把 `HOLD` 值从 100 调到 150 或 200
- 游戏靠轮询检测控制器状态，太快的按下/释放可能被跳过
- 霍格沃兹之遗需要先按扳机再按键，检查是否忘了加 `RT:255`

---

## 从源码构建（开发者）

> 普通用户请直接下载 Release zip，无需看这部分。

### 环境

- .NET 8 SDK
- Windows 或 WSL（可交叉编译）

### 构建

```powershell
git clone https://github.com/761592343/VoiceAttack-Plugin-VGamepad.git
cd VoiceAttack-Plugin-VGamepad
dotnet restore
dotnet build -c Release
```

产物在 `bin/Release/net8.0-windows/`：
- `VoiceAttack-Plugin-VGamepad.dll` —— 插件本体
- `Nefarius.ViGEm.Client.dll` —— ViGEm SDK（含内嵌原生驱动）

### 安装

将两个 DLL 复制到 `%APPDATA%\VoiceAttack2\Apps\VoiceAttack-Plugin-VGamepad\`。

### 修改 GUID

基于此代码二次开发时，需要生成新 GUID 替换 `Plugin.cs` 中 `VA_Id()` 的返回值，避免与官方版本冲突。

---

## 许可

MIT
