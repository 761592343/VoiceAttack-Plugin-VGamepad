using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace VoiceAttackPluginVGamepad;

/// <summary>
/// VoiceAttack plugin: complete virtual Xbox 360 controller via ViGEmBus.
/// Exposes every button, trigger, thumbstick, and D-pad via context strings.
/// 
/// CONTEXT STRING FORMAT (case-insensitive):
/// 
///   Face buttons:    A:1  A:0   B:1  B:0   X:1  X:0   Y:1  Y:0
///   Shoulders:       LB:1 LB:0  RB:1 RB:0
///   Thumb clicks:    LS:1 LS:0  RS:1 RS:0
///   Start/Back:      START:1 START:0  BACK:1 BACK:0
///   D-pad:           UP:1 UP:0  DOWN:1 DOWN:0  LEFT:1 LEFT:0  RIGHT:1 RIGHT:0
/// 
///   Triggers (0-255):   RT:255  RT:0    LT:128  LT:0
///   Thumbsticks (-32768..32767):  LX:-16384  LY:16384  RX:0  RY:0
/// 
///   Multi-action (semicolon-separated):  RT:255;A:1  or  LX:16384;B:1;RT:128
///   Release all:     RELEASE
///
///   Timing config:   HOLD:150  (button hold for manual combos, default 100ms)
///                    TRIGGER:80 (pre/post delay for manual combos, default 50ms)
/// 
/// VoiceAttack usage for Hogwarts Legacy spell (RT + A):
///   Action 1: Context = "RT:255"
///   Action 2: Pause 0.05s
///   Action 3: Context = "A:1"
///   Action 4: Pause 0.10s
///   Action 5: Context = "A:0"
///   Action 6: Pause 0.05s
///   Action 7: Context = "RT:0"
/// 
///   Or use multi-action shorthand: "RT:255;HOLD:50;A:1;HOLD:100;A:0;HOLD:50;RT:0"
///   (Note: HOLD in multi-action uses the CURRENT hold value, sets new value for future)
/// </summary>
public class Plugin
{
    // ── Singleton state ──────────────────────────────────────────────
    private static ViGEmClient? _client;
    private static IXbox360Controller? _controller;
    private static readonly object _lock = new();
    private static bool _initialized;

    // Tunable timing (milliseconds)
    private static int _holdMs = 100;   // default hold duration for HOLD:n in multi-action
    private static int _delayMs = 50;   // default delay between sub-actions in multi-action

    // ── VoiceAttack Plugin Interface (v4) ────────────────────────────

    public static Guid VA_Id() =>
        Guid.Parse("70452065-2794-405C-8DBC-0BAEC8BD758C");

    public static string VA_DisplayName() => "VGamepad (Virtual Xbox 360)";

    public static string VA_DisplayInfo() =>
        "Virtual Xbox 360 controller. Context examples: A:1 B:0 RT:255 LX:-16384 RELEASE. " +
        "Multi-action: RT:255;A:1;HOLD:100;A:0;RT:0";

    public static void VA_Init1(dynamic vaProxy)
    {
        lock (_lock)
        {
            if (_initialized) return;
            try
            {
                _client = new ViGEmClient();
                _controller = _client.CreateXbox360Controller();
                _controller.Connect();
                _controller.AutoSubmitReport = true;
                _initialized = true;
                vaProxy.WriteToLog(
                    "[VGamepad] Virtual Xbox 360 controller connected.", "green");
            }
            catch (Exception ex)
            {
                vaProxy.WriteToLog(
                    $"[VGamepad] FAILED. ViGEmBus installed? {ex.Message}", "red");
                throw;
            }
        }
    }

    public static void VA_Exit1(dynamic vaProxy)
    {
        lock (_lock)
        {
            try { _controller?.Disconnect(); _client?.Dispose(); } catch { }
            _initialized = false;
        }
    }

    public static void VA_StopCommand()
    {
        lock (_lock)
        {
            try { ReleaseAllInternal(); } catch { }
        }
    }

    public static void VA_Invoke1(dynamic vaProxy)
    {
        string raw = (vaProxy.Context as string ?? "").Trim();
        if (string.IsNullOrEmpty(raw)) return;

        lock (_lock)
        {
            if (_controller == null || !_initialized)
            {
                vaProxy.WriteToLog("[VGamepad] Not connected.", "red");
                return;
            }
            try
            {
                // Split by semicolon for multi-action
                var actions = raw.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (var action in actions)
                {
                    Dispatch(action.Trim(), vaProxy);
                }
            }
            catch (Exception ex)
            {
                vaProxy.WriteToLog($"[VGamepad] Error: {ex.Message}", "red");
                try { ReleaseAllInternal(); } catch { }
            }
        }
    }

    // ── Command dispatcher ───────────────────────────────────────────

    private static void Dispatch(string cmd, dynamic vaProxy)
    {
        if (cmd.Equals("RELEASE", StringComparison.OrdinalIgnoreCase) ||
            cmd.Equals("RESET", StringComparison.OrdinalIgnoreCase))
        {
            ReleaseAllInternal();
            return;
        }

        // Parameterized commands: KEY:VALUE
        int colon = cmd.IndexOf(':');
        if (colon < 0)
        {
            vaProxy.WriteToLog($"[VGamepad] Unknown: '{cmd}'", "yellow");
            return;
        }

        var key = cmd[..colon].ToUpperInvariant();
        var valStr = cmd[(colon + 1)..];

        switch (key)
        {
            // ── Digital buttons ──
            case "A":  SetButton(Xbox360Button.A, valStr); break;
            case "B":  SetButton(Xbox360Button.B, valStr); break;
            case "X":  SetButton(Xbox360Button.X, valStr); break;
            case "Y":  SetButton(Xbox360Button.Y, valStr); break;
            case "LB": SetButton(Xbox360Button.LeftShoulder, valStr); break;
            case "RB": SetButton(Xbox360Button.RightShoulder, valStr); break;
            case "LS": SetButton(Xbox360Button.LeftThumb, valStr); break;
            case "RS": SetButton(Xbox360Button.RightThumb, valStr); break;
            case "START": SetButton(Xbox360Button.Start, valStr); break;
            case "BACK":  SetButton(Xbox360Button.Back, valStr); break;
            case "UP":    SetButton(Xbox360Button.Up, valStr); break;
            case "DOWN":  SetButton(Xbox360Button.Down, valStr); break;
            case "LEFT":  SetButton(Xbox360Button.Left, valStr); break;
            case "RIGHT": SetButton(Xbox360Button.Right, valStr); break;

            // ── Triggers (sliders, 0-255 byte) ──
            case "RT": SetSlider(Xbox360Slider.RightTrigger, valStr); break;
            case "LT": SetSlider(Xbox360Slider.LeftTrigger, valStr); break;

            // ── Thumbsticks (axes, -32768..32767 short) ──
            case "LX": SetAxis(Xbox360Axis.LeftThumbX, valStr); break;
            case "LY": SetAxis(Xbox360Axis.LeftThumbY, valStr); break;
            case "RX": SetAxis(Xbox360Axis.RightThumbX, valStr); break;
            case "RY": SetAxis(Xbox360Axis.RightThumbY, valStr); break;

            // ── Timing ──
            case "HOLD":
                if (int.TryParse(valStr, out int h)) { _holdMs = h; Thread.Sleep(h); }
                break;
            case "DELAY":
                if (int.TryParse(valStr, out int d)) { _delayMs = d; Thread.Sleep(d); }
                break;

            default:
                vaProxy.WriteToLog($"[VGamepad] Unknown key: '{key}'", "yellow");
                break;
        }
    }

    // ── Low-level setters ────────────────────────────────────────────

    private static void SetButton(Xbox360Button button, string val)
    {
        if (byte.TryParse(val, out byte v))
            _controller!.SetButtonState(button, v != 0);
    }

    private static void SetSlider(Xbox360Slider slider, string val)
    {
        if (byte.TryParse(val, out byte v))
            _controller!.SetSliderValue(slider, v);
    }

    private static void SetAxis(Xbox360Axis axis, string val)
    {
        if (short.TryParse(val, out short v))
            _controller!.SetAxisValue(axis, v);
    }

    // ── Emergency release ────────────────────────────────────────────

    private static void ReleaseAllInternal()
    {
        if (_controller == null) return;

        foreach (var btn in new[] {
            Xbox360Button.A, Xbox360Button.B, Xbox360Button.X, Xbox360Button.Y,
            Xbox360Button.LeftShoulder, Xbox360Button.RightShoulder,
            Xbox360Button.LeftThumb, Xbox360Button.RightThumb,
            Xbox360Button.Start, Xbox360Button.Back,
            Xbox360Button.Up, Xbox360Button.Down, Xbox360Button.Left, Xbox360Button.Right
        }) _controller.SetButtonState(btn, false);

        _controller.SetSliderValue(Xbox360Slider.LeftTrigger, 0);
        _controller.SetSliderValue(Xbox360Slider.RightTrigger, 0);
        _controller.SetAxisValue(Xbox360Axis.LeftThumbX, 0);
        _controller.SetAxisValue(Xbox360Axis.LeftThumbY, 0);
        _controller.SetAxisValue(Xbox360Axis.RightThumbX, 0);
        _controller.SetAxisValue(Xbox360Axis.RightThumbY, 0);
    }
}
