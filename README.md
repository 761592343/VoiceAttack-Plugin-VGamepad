# VoiceAttack Plugin VGamepad

VoiceAttack 插件 —— 完整的虚拟 Xbox 360 控制器。通过 ViGEmBus 驱动创建原生 XInput 设备，游戏直接识别。

## 原理

```
语音 → VoiceAttack → VoiceAttack-Plugin-VGamepad.dll → ViGEmClient SDK → ViGEmBus(驱动) → 虚拟Xbox360 → 游戏
```

对比其他方案：

| | vJoy + XOutput | reWASD | 本插件 |
|---|---|---|---|
| 安装量 | 两个软件 | 一个商业软件 | 一个开源驱动 |
| 启动 | 每次手动开 XOutput | 自动 | 驱动自启，零操作 |
| 设备协议 | DirectInput→转XInput | XInput | **原生 XInput** |
| 侵入性 | 低 | 高（HOOK 层） | 低（标准驱动） |
| 费用 | 免费 | ~$7 | 免费 |
| 控制粒度 | 按钮+轴 | 全功能 | **所有按钮/扳机/摇杆/十字键** |

## 安装

### 第一步：安装 ViGEmBus 驱动（只需一次）

1. 下载：https://github.com/nefarius/ViGEmBus/releases/latest
2. 安装 `ViGEmBus_Setup_x64.msi`
3. **重启电脑**

> ViGEmBus 是开源虚拟手柄总线驱动。安装后开机自启，没有界面，没有广告，在后台静默运行。

### 第二步：复制插件文件

把以下两个 DLL 复制到 VoiceAttack 的插件目录：

```
源文件（在项目 bin/Release/net8.0-windows/ 下）：
  VoiceAttack-Plugin-VGamepad.dll
  Nefarius.ViGEm.Client.dll

目标目录：
  %APPDATA%\VoiceAttack2\Apps\VGamepad\
```

> 如果 `HermesVGamepad` 目录不存在就手动创建。两个文件必须放在同一个目录里。

### 第三步：启用插件

1. 打开 VoiceAttack
2. **Options → General → 勾选 "Enable Plugin Support"**
3. 重启 VoiceAttack
4. 检查日志，应该看到绿色信息：

   ```
   [HermesVGamepad] Virtual Xbox 360 controller connected.
   ```

5. 验证：Win+R → 输入 `joy.cpl` → 应该能看到 "Xbox 360 Controller for Windows"

### 第四步：优化语音识别（建议）

VoiceAttack → **Options → Recognition**：

| 设置项 | 推荐值 | 说明 |
|--------|--------|------|
| Minimum Confidence Level | **50** | 太低误触发，太高识别不到。57/75 被拒就说明要降 |
| Command Weight | 默认 | 有多个相似指令时调优先级 |

另外建议在 Windows 设置 → 语音 → 训练语音引擎，读几段话提升识别率。

---

## 指令参考

每条指令格式：`KEY:VALUE`

多条指令用 **分号** 分隔，一次性执行：`RT:255;A:1;HOLD:100;A:0;RT:0`

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

### 扳机键（Slider，0-255）

| 指令 | 对应 | 示例 |
|------|------|------|
| `RT:255` | 右扳机 (Right Trigger) | `RT:0` = 释放, `RT:128` = 半程, `RT:255` = 到底 |
| `LT:255` | 左扳机 (Left Trigger) | 同上 |

> 扳机是线性输入，用字节表示力度。0 = 没按，255 = 按到底。

### 摇杆（Axis，-32768 ~ 32767）

0 = 居中，负 = 左/上，正 = 右/下。

| 指令 | 对应 | 示例 |
|------|------|------|
| `LX:0` | 左摇杆 X 轴 | `LX:-16384` = 左半, `LX:16384` = 右半 |
| `LY:0` | 左摇杆 Y 轴 | `LY:-16384` = 前推, `LY:16384` = 后拉 |
| `RX:0` | 右摇杆 X 轴 | 同上 |
| `RY:0` | 右摇杆 Y 轴 | 同上 |

### 控制指令

| 指令 | 说明 |
|------|------|
| `RELEASE` | 释放所有按钮、扳机、摇杆（紧急停止/重置） |
| `HOLD:100` | 暂停 100 毫秒，同时设为默认按住时长 |
| `DELAY:100` | 暂停 100 毫秒，同时设为默认延迟 |

> `HOLD:n` 和 `DELAY:n` 的区别仅在于语义（一个表示"按住多久"，一个表示"等待多久"），实际效果都是 Thread.Sleep(n)。在多指令链中用于控制动作之间的时序。

---

## VoiceAttack 命令配置模板

### 添加插件的通用步骤

1. 新建或编辑一个命令（"When I say: xxx"）
2. 添加动作：**Other → VoiceAttack Action → Execute an External Plugin Function**
3. 在 "Plugin" 下拉选择 **"VGamepad (Virtual Xbox 360)"**
4. 在 "Context" 框输入指令（参考上面表格）
5. 勾选 **"Wait until the function completes"**

> 如果不勾选 Wait，插件调用会异步执行，可能导致时序错乱。除非有特殊需求，一律勾选。

---

## 实战示例

### 霍格沃兹之遗：语音念咒施法

游戏机制：释放咒语 = RT（右扳机）+ A/B/X/Y（脸键）。

#### 方案 A：分步动作（清晰，推荐入门）

VoiceAttack 命令中放 7 个动作：

| 顺序 | 动作类型 | 设置 |
|------|----------|------|
| 1 | Execute Plugin | Context = `RT:255`，勾选 Wait |
| 2 | Pause | 0.05 秒 |
| 3 | Execute Plugin | Context = `A:1`，勾选 Wait |
| 4 | Pause | 0.10 秒 |
| 5 | Execute Plugin | Context = `A:0`，勾选 Wait |
| 6 | Pause | 0.05 秒 |
| 7 | Execute Plugin | Context = `RT:0`，勾选 Wait |

换咒语只需改步骤 3 和 5 的 A → B / X / Y。

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

#### 其他咒语按钮对照

霍格沃兹之遗默认键位（Xbox 手柄）：

| 咒语槽 | 按键 | 指令 |
|--------|------|------|
| 第 1 个 | A | `A:1` / `A:0` |
| 第 2 个 | B | `B:1` / `B:0` |
| 第 3 个 | X | `X:1` / `X:0` |
| 第 4 个 | Y | `Y:1` / `Y:0` |

### 其他游戏示例

#### 模拟跳跃（A 键轻点）

```
A:1;HOLD:80;A:0
```

#### 长按 RB 使用技能

```
RB:1;HOLD:2000;RB:0
```

#### 同时按 LT + RT

```
LT:255;RT:255
```

#### 左摇杆前推（走路）+ 加速

```
LY:-16384;HOLD:2000;LY:0
```

#### 连续按 A 三次（菜单快速确认）

```
A:1;HOLD:80;A:0;HOLD:80;A:1;HOLD:80;A:0;HOLD:80;A:1;HOLD:80;A:0
```

#### 紧急停止一切输入

```
RELEASE
```

---

## 故障排查

### 插件加载失败（红字 "FAILED to connect"）

- ViGEmBus 驱动没装或没启动 → 重新安装并重启
- 检查 DLL 是否放在正确目录 `%APPDATA%\VoiceAttack2\Apps\VGamepad\`
- 检查 Enable Plugin Support 是否勾选

### 游戏不响应虚拟手柄

- Win+R → `joy.cpl` → 确认 "Xbox 360 Controller for Windows" 存在
- 有些游戏需要在设置里手动选择输入设备
- 检查物理手柄是否抢占了 Player 1 位置（见下方）

### 物理手柄和虚拟手柄冲突

通常不影响。如果游戏只认物理手柄：
- 用 **HidHide**（https://github.com/nefarius/HidHide）隐藏物理手柄的 XInput 接口
- 或者在游戏设置里切换控制器优先级

### 语音识别被拒（recognized but rejected）

日志显示 `'xxx' recognized, but rejected with confidence level XX/YY`：
- VoiceAttack → Options → Recognition → 降低 **Minimum Confidence Level**（推荐 50）
- 训练 Windows 语音引擎
- 给命令多加几个别名

### 指令执行了但游戏没反应

- 增加延迟：把 `HOLD` 值从 100 调到 150 或 200
- 游戏靠轮询检测控制器状态，太快的按下/释放可能被跳过
- 检查是不是忘了加 `RT:255`（霍格沃兹之遗需要先按扳机再按键）

---

## 编译（开发者）

如果需要自己修改插件：

### 环境

- .NET 8 SDK
- Windows 或 WSL（可交叉编译）

### 构建

```powershell
cd HermesVGamepad
dotnet restore
dotnet build -c Release
```

产物：
- `bin/Release/net8.0-windows/VoiceAttack-Plugin-VGamepad.dll` —— 插件本体
- `bin/Release/net8.0-windows/Nefarius.ViGEm.Client.dll` —— ViGEm SDK（含内嵌原生驱动）

两个都要复制到 VoiceAttack Apps 目录。

### 修改 GUID

如果基于此代码二次开发，需要生成新 GUID 替换 `Plugin.cs` 中的 `VA_Id()` 返回值，避免与原始版本冲突。

---

## 许可

MIT
