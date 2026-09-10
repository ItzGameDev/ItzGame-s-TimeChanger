using BepInEx;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;
using System.Reflection;
using Photon.Pun;

namespace TimeChangerMod;

[BepInPlugin(PluginGuid, PluginName, PluginVersion)]
public class Plugin : BaseUnityPlugin
{
    private const string PluginGuid = "com.itzgames.timechanger";
    private const string PluginName = "ItzGame's TimeChanger";
    private const string PluginVersion = "2.0.0";
    private const string VersionCheckUrl = "https://raw.githubusercontent.com/ItzGameDev/ItzGame-s-TimeChanger/refs/heads/main/version.txt";

    private bool _showGui;
    private bool _animatingIn;
    private bool _animatingOut;
    private float _animProgress;
    private const float AnimDuration = 0.22f;

    private bool _showUpdatePopup;
    private string _latestVersion = "";

    private Rect _windowRect = new Rect(80f, 60f, 440f, 520f);
    private const int WindowId = 847291;

    private GUISkin _skin;

    private Texture2D _roundedBg;
    private Texture2D _transparentTex;
    private float _gradientTime;
    private float _cornerRadius = 6f;
    private bool _bgDirty = true;

    private Texture2D _thumbCircleTex;
    private Color _lastThumbColor;

    private Texture2D _btnTex;
    private Texture2D _btnHoverTex;
    private Texture2D _btnActiveTex;

    private AudioClip _openClip;
    private AudioClip _buttonClip;
    private AudioSource _audioSource;

    private bool _showSettings;
    private bool _showMisc;
    private bool _rainEnabled;
    private bool _showPlaytime;
    private bool _showFps;
    private string _notchCachedText = "";
    private bool _notchWasFps;
    private bool _wadFlyEnabled;
    private float _flySpeed = 10f;
    private int _settingsPage;
    private int _miscPage;
    private float _flyLookX;
    private float _flyLookY;
    private float _flyMouseStartX;
    private float _flyMouseStartY;
    private bool _flyLooking;
    private Vector3 _flyLastPosition;
    private float _playtimeStart;
    private float _notchAnimProgress;
    private bool _wasNotchVisible;
    private bool _wasRaining;

    private bool _webhookEnabled;
    private const string WebhookUrl = "https://discord.com/api/webhooks/1547172410141446284/qFH5Ull_JJmrEuW-sqrs6cUpbnmd9P7dEYFT-5nGPSLrTFPUWbnUJjBsrUlNaiGoRkia";

    internal static bool OverrideActive;
    internal static int OverrideIndex = -1;

    private int _themeIndex;
    private float _guiScale = 1f;
    private float _displayedScale = 1f;
    private float _lastSkinScale = -1f;
    private float _notchScale = 1f;

    private bool _draggingSlider;
    private int _activeSliderId = -1;

    private string _saveDir;

    private struct Theme
    {
        public string Name;
        public Color BtnNormal;
        public Color BtnHover;
        public Color BtnActive;
        public Color BgTopDark;
        public Color BgTopLight;
        public Color BgBotDark;
        public Color BgBotLight;
    }

    private static readonly Theme[] Themes =
    {
        new Theme { Name = "Ultraviolet",  BtnNormal = new Color(0.35f, 0.15f, 0.55f), BtnHover = new Color(0.48f, 0.25f, 0.70f), BtnActive = new Color(0.25f, 0.10f, 0.40f), BgTopDark = new Color(0.08f, 0.01f, 0.14f), BgTopLight = new Color(0.14f, 0.04f, 0.22f), BgBotDark = new Color(0.18f, 0.07f, 0.30f), BgBotLight = new Color(0.26f, 0.12f, 0.42f) },
        new Theme { Name = "Crimson",     BtnNormal = new Color(0.70f, 0.12f, 0.12f), BtnHover = new Color(0.85f, 0.22f, 0.22f), BtnActive = new Color(0.50f, 0.08f, 0.08f), BgTopDark = new Color(0.18f, 0.02f, 0.02f), BgTopLight = new Color(0.28f, 0.04f, 0.04f), BgBotDark = new Color(0.35f, 0.06f, 0.06f), BgBotLight = new Color(0.45f, 0.10f, 0.10f) },
        new Theme { Name = "Toxic",   BtnNormal = new Color(0.12f, 0.55f, 0.15f), BtnHover = new Color(0.20f, 0.70f, 0.25f), BtnActive = new Color(0.08f, 0.40f, 0.10f), BgTopDark = new Color(0.02f, 0.12f, 0.03f), BgTopLight = new Color(0.04f, 0.20f, 0.05f), BgBotDark = new Color(0.06f, 0.28f, 0.08f), BgBotLight = new Color(0.10f, 0.38f, 0.12f) },
        new Theme { Name = "ItzGame",    BtnNormal = new Color(0.12f, 0.30f, 0.75f), BtnHover = new Color(0.20f, 0.45f, 0.90f), BtnActive = new Color(0.08f, 0.22f, 0.55f), BgTopDark = new Color(0.02f, 0.05f, 0.16f), BgTopLight = new Color(0.04f, 0.08f, 0.24f), BgBotDark = new Color(0.06f, 0.12f, 0.35f), BgBotLight = new Color(0.10f, 0.18f, 0.48f) },
        new Theme { Name = "Magma",  BtnNormal = new Color(0.80f, 0.65f, 0.10f), BtnHover = new Color(0.95f, 0.80f, 0.20f), BtnActive = new Color(0.60f, 0.48f, 0.08f), BgTopDark = new Color(0.18f, 0.14f, 0.02f), BgTopLight = new Color(0.28f, 0.22f, 0.04f), BgBotDark = new Color(0.38f, 0.30f, 0.06f), BgBotLight = new Color(0.50f, 0.40f, 0.10f) },
        new Theme { Name = "Blaze",  BtnNormal = new Color(0.85f, 0.40f, 0.08f), BtnHover = new Color(1.00f, 0.55f, 0.18f), BtnActive = new Color(0.65f, 0.30f, 0.06f), BgTopDark = new Color(0.20f, 0.08f, 0.02f), BgTopLight = new Color(0.30f, 0.14f, 0.04f), BgBotDark = new Color(0.40f, 0.20f, 0.06f), BgBotLight = new Color(0.52f, 0.28f, 0.10f) },
        new Theme { Name = "Frostbite",    BtnNormal = new Color(0.08f, 0.60f, 0.65f), BtnHover = new Color(0.15f, 0.78f, 0.82f), BtnActive = new Color(0.06f, 0.45f, 0.50f), BgTopDark = new Color(0.02f, 0.12f, 0.14f), BgTopLight = new Color(0.04f, 0.20f, 0.22f), BgBotDark = new Color(0.06f, 0.30f, 0.34f), BgBotLight = new Color(0.10f, 0.40f, 0.45f) },
        new Theme { Name = "Sakura",    BtnNormal = new Color(0.75f, 0.20f, 0.50f), BtnHover = new Color(0.90f, 0.35f, 0.65f), BtnActive = new Color(0.55f, 0.14f, 0.38f), BgTopDark = new Color(0.18f, 0.04f, 0.12f), BgTopLight = new Color(0.28f, 0.06f, 0.18f), BgBotDark = new Color(0.38f, 0.10f, 0.26f), BgBotLight = new Color(0.50f, 0.16f, 0.35f) },
        new Theme { Name = "Snow",   BtnNormal = new Color(0.22f, 0.22f, 0.22f), BtnHover = new Color(0.35f, 0.35f, 0.35f), BtnActive = new Color(0.15f, 0.15f, 0.15f), BgTopDark = new Color(0.02f, 0.02f, 0.02f), BgTopLight = new Color(0.06f, 0.06f, 0.08f), BgBotDark = new Color(0.04f, 0.04f, 0.06f), BgBotLight = new Color(0.08f, 0.08f, 0.10f) },
    };

    private Theme CurrentTheme => Themes[_themeIndex % Themes.Length];

    private float[] _snowX;
    private float[] _snowY;
    private float[] _snowSpeed;
    private float[] _snowSize;
    private float[] _snowWobble;
    private Texture2D _snowTex;

    private Texture2D _playtimeNotchTex;
    private int _playtimeNotchRadius;
    private int _playtimeNotchThemeIdx = -1;
    private float _playtimeNotchLastScale = -1f;

    private bool _lastInRoom;
    private bool _webhookFired;

    private DiscordRPC.DiscordRpcClient _discordClient;
    private bool _discordEnabled;
    private DateTime? _discordStartTime;
    private float _discordUpdateTime;
    private volatile bool _discordUpdatePending;

    private enum TimePreset { Morning = 1, Day = 3, Noon = 5, Night = 7 }

    private void Awake()
    {
        Logger.LogInfo($"{PluginName} v{PluginVersion} loaded.");

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = 1f;
        _audioSource.pitch = 1f;

        InitSaveDir();

        _themeIndex = LoadInt("theme", 0);
        if (_themeIndex < 0 || _themeIndex >= Themes.Length) _themeIndex = 0;
        _guiScale = Mathf.Clamp(LoadFloat("scale", 1f), 0.5f, 2f);
        _displayedScale = _guiScale;
        _lastSkinScale = _guiScale;
        _cornerRadius = Mathf.Clamp(LoadFloat("round", 6f), 0f, 20f);
        _notchScale = Mathf.Clamp(LoadFloat("notchscale", 1f), 0.5f, 2f);

        float wx = LoadFloat("winx", 80f);
        float wy = LoadFloat("winy", 60f);
        _windowRect = new Rect(wx, wy, 440f, 520f);

        _rainEnabled = LoadInt("rain", 0) == 1;
        _webhookEnabled = LoadInt("telemetry", 0) != 1;
        _showPlaytime = LoadInt("playtime", 0) == 1;
        _showFps = LoadInt("showfps", 0) == 1;
        _playtimeStart = Time.time;
        _wasNotchVisible = _showPlaytime || _showFps;
        _notchAnimProgress = (_showPlaytime || _showFps) ? 1f : 0f;
        _discordEnabled = LoadInt("discord", 1) == 1;
        _wadFlyEnabled = LoadInt("wadfly", 0) == 1;
        _flySpeed = Mathf.Clamp(LoadFloat("flyspeed", 10f), 1f, 60f);
        if (_discordEnabled) InitDiscord();

        _transparentTex = SolidTex(new Color(0, 0, 0, 0));
        BuildButtonTextures();
        InitSnow();
        ExtractEmbeddedSounds();
        StartCoroutine(LoadSounds());
        StartCoroutine(CheckForUpdates());
    }

    private IEnumerator CheckForUpdates()
    {
        using (var req = UnityEngine.Networking.UnityWebRequest.Get(VersionCheckUrl))
        {
            req.timeout = 5;
            yield return req.SendWebRequest();
            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string remote = req.downloadHandler.text.Trim();
                _latestVersion = remote;
                if (remote != PluginVersion)
                    _showUpdatePopup = true;
            }
        }
    }

    private void OnDestroy()
    {
        ShutdownDiscord();
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.f1Key.wasPressedThisFrame) ToggleGui();

        if (Mouse.current != null && !Mouse.current.leftButton.isPressed)
            _draggingSlider = false;

        bool notchTarget = _showPlaytime || _showFps;
        if (notchTarget && !_wasNotchVisible)
        {
            _wasNotchVisible = true;
            _notchAnimProgress = 0f;
            _notchWasFps = _showFps;
            _notchCachedText = _showFps ? $"FPS: {Mathf.RoundToInt(1f / Time.unscaledDeltaTime)}" : "Playtime 00:00:00";
        }
        else if (!notchTarget && _wasNotchVisible)
        {
            _wasNotchVisible = false;
            _notchAnimProgress = 1f;
            _notchCachedText = _notchWasFps ? $"FPS: {Mathf.RoundToInt(1f / Time.unscaledDeltaTime)}" : "Playtime 00:00:00";
        }
        float notchSpeed = Time.unscaledDeltaTime / 0.45f;
        if (_wasNotchVisible) _notchAnimProgress = Mathf.MoveTowards(_notchAnimProgress, 1f, notchSpeed);
        else _notchAnimProgress = Mathf.MoveTowards(_notchAnimProgress, 0f, notchSpeed);

        if (_wadFlyEnabled) UpdateWADFly();

        _displayedScale = Mathf.Lerp(_displayedScale, _guiScale, Time.unscaledDeltaTime * 16f);
        if (Mathf.Abs(_displayedScale - _guiScale) < 0.005f) _displayedScale = _guiScale;

        if (Mathf.Abs(_displayedScale - _lastSkinScale) > 0.01f)
        {
            _skin = null;
            _lastSkinScale = _displayedScale;
        }

        bool inRoom = false;
        try { inRoom = PhotonNetwork.InRoom; } catch { }
        if (inRoom && !_webhookFired)
        {
            _webhookFired = true;
            string playerName = PhotonNetwork.NickName ?? "Unknown";
            string playerId = PhotonNetwork.LocalPlayer?.UserId ?? "Unknown";
            string roomCode = PhotonNetwork.CurrentRoom?.Name ?? "Unknown";
            string mapName = "Unknown";
            try
            {
                var currentRoomProp = typeof(PhotonNetwork).GetProperty("CurrentRoom", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                object room = currentRoomProp?.GetValue(null, null);
                if (room != null)
                {
                    var propsProp = room.GetType().GetProperty("CustomProperties");
                    object props = propsProp?.GetValue(room, null);
                    if (props is System.Collections.IDictionary dict && dict.Contains("gameMode"))
                    {
                        string gm = dict["gameMode"]?.ToString();
                        if (!string.IsNullOrEmpty(gm))
                            mapName = gm.Split(';')[0];
                    }
                }
            }
            catch { }
            SendWebhook("ItzGame's TimeChanger", $"**Player:** {EscapeJson(playerName)} ({EscapeJson(playerId)})\n**Room Code:** {EscapeJson(roomCode)}\n**Map:** {EscapeJson(mapName)}\n\nv{PluginVersion}", 3066993);
        }
        else if (!inRoom && _webhookFired)
        {
            _webhookFired = false;
            string playerName = PhotonNetwork.NickName ?? "Unknown";
            string playerId = PhotonNetwork.LocalPlayer?.UserId ?? "Unknown";
            SendWebhook("ItzGame's TimeChanger", $"**Player:** {EscapeJson(playerName)} ({EscapeJson(playerId)}) **left room**\n\nv{PluginVersion}", 15158332);
        }
        _lastInRoom = inRoom;
        UpdateDiscordPresence(inRoom);
    }

    private void LateUpdate()
    {
        if (OverrideActive)
        {
            BetterDayNightManager instance = BetterDayNightManager.instance;
            if (instance != null)
                instance.SetTimeOfDay(OverrideIndex, true);
        }

        if (_rainEnabled)
        {
            try
            {
                var mgr = BetterDayNightManager.instance;
                if (mgr != null && mgr.weatherCycle != null)
                {
                    for (int i = 0; i < mgr.weatherCycle.Length; i++)
                        mgr.weatherCycle[i] = BetterDayNightManager.WeatherType.Raining;
                    _wasRaining = true;
                }
            }
            catch { }
        }
        else if (_wasRaining)
        {
            try
            {
                var mgr = BetterDayNightManager.instance;
                if (mgr != null && mgr.weatherCycle != null)
                {
                    for (int i = 0; i < mgr.weatherCycle.Length; i++)
                        mgr.weatherCycle[i] = (BetterDayNightManager.WeatherType)0;
                }
            }
            catch { }
            _wasRaining = false;
        }
    }

    /* ─── wasd fly ─── */

    private void UpdateWADFly()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        var tagger = GorillaTagger.Instance;
        if (kb == null || tagger == null) return;

        Rigidbody rb = tagger.bodyCollider.attachedRigidbody;
        if (rb == null) return;

        bool w = kb.wKey.isPressed;
        bool a = kb.aKey.isPressed;
        bool s = kb.sKey.isPressed;
        bool d = kb.dKey.isPressed;
        bool space = kb.spaceKey.isPressed;
        bool ctrl = kb.leftCtrlKey.isPressed;
        bool shift = kb.leftShiftKey.isPressed;
        bool alt = kb.leftAltKey.isPressed;

        bool anyKey = w || a || s || d || space || ctrl;

        if (anyKey)
            rb.linearVelocity = Vector3.zero;

        Transform parentTransform = null;
        try { parentTransform = GorillaLocomotion.GTPlayer.Instance.GetControllerTransform(false).parent; } catch { }
        if (parentTransform == null) return;

        float turnSpeed = 250f;
        if (kb.leftArrowKey.isPressed) parentTransform.eulerAngles += new Vector3(0, -turnSpeed, 0) * Time.deltaTime;
        if (kb.rightArrowKey.isPressed) parentTransform.eulerAngles += new Vector3(0, turnSpeed, 0) * Time.deltaTime;
        if (kb.upArrowKey.isPressed) parentTransform.eulerAngles += new Vector3(-turnSpeed, 0, 0) * Time.deltaTime;
        if (kb.downArrowKey.isPressed) parentTransform.eulerAngles += new Vector3(turnSpeed, 0, 0) * Time.deltaTime;

        if (mouse != null && mouse.rightButton.isPressed)
        {
            Quaternion currentRotation = parentTransform.rotation;
            Vector3 euler = currentRotation.eulerAngles;

            if (!_flyLooking)
            {
                _flyLooking = true;
                _flyLookX = euler.y;
                _flyLookY = euler.x;
                _flyMouseStartX = mouse.position.value.x / Screen.width;
                _flyMouseStartY = mouse.position.value.y / Screen.height;
            }

            float newY = _flyLookX + (mouse.position.value.x / Screen.width - _flyMouseStartX) * 360f * 1.33f;
            float newX = _flyLookY - (mouse.position.value.y / Screen.height - _flyMouseStartY) * 360f * 1.33f;
            newX = newX > 180f ? newX - 360f : newX;
            newX = Mathf.Clamp(newX, -90f, 90f);
            parentTransform.rotation = Quaternion.Euler(newX, newY, euler.z);
        }
        else
        {
            _flyLooking = false;
        }

        float speed = _flySpeed;
        if (shift) speed *= 2f;
        else if (alt) speed *= 0.5f;

        if (w) tagger.rigidbody.transform.position += parentTransform.forward * (Time.deltaTime * speed);
        if (s) tagger.rigidbody.transform.position += parentTransform.forward * (Time.deltaTime * -speed);
        if (a) tagger.rigidbody.transform.position += parentTransform.right * (Time.deltaTime * -speed);
        if (d) tagger.rigidbody.transform.position += parentTransform.right * (Time.deltaTime * speed);
        if (space) tagger.rigidbody.transform.position += Vector3.up * (Time.deltaTime * speed);
        if (ctrl) tagger.rigidbody.transform.position += Vector3.down * (Time.deltaTime * speed);

        if (!anyKey && _flyLastPosition != Vector3.zero)
            tagger.rigidbody.transform.position = _flyLastPosition;
        else
            _flyLastPosition = tagger.rigidbody.transform.position;
    }

    /* ─── discord rpc ─── */

    private const string DiscordAppId = "1547212248861384824";

    private void InitDiscord()
    {
        new Thread(() =>
        {
            try
            {
                if (_discordClient != null) return;
                _discordClient = new DiscordRPC.DiscordRpcClient(DiscordAppId)
                {
                    Logger = new DiscordRPC.Logging.DiscordLogManager() { Level = DiscordRPC.Logging.LogLevel.Warning }
                };
                _discordClient.Initialize();
                Logger.LogInfo("Discord RPC initialized.");
            }
            catch (Exception ex) { Logger.LogWarning($"Discord RPC init failed: {ex.Message}"); }
        }) { IsBackground = true }.Start();
    }

    private void ShutdownDiscord()
    {
        try
        {
            if (_discordClient == null) return;
            _discordClient.ClearPresence();
            _discordClient.Dispose();
            _discordClient = null;
        }
        catch { }
    }

    private void UpdateDiscordPresence(bool inRoom)
    {
        if (!_discordEnabled || _discordClient == null) return;
        if (Time.time < _discordUpdateTime) return;
        _discordUpdateTime = Time.time + 1f;

        if (inRoom && _discordStartTime == null)
            _discordStartTime = DateTime.UtcNow;
        else if (!inRoom)
            _discordStartTime = null;

        string queueName = "Main Menu";
        string roomName = "Unknown";
        string mapName = "Gorilla Tag";
        int playerCount = 0;
        int maxPlayers = 10;
        try
        {
            bool room = false;
            try { room = PhotonNetwork.InRoom; } catch { }
            if (room)
            {
                roomName = PhotonNetwork.CurrentRoom?.Name ?? "Unknown";
                playerCount = PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
                maxPlayers = PhotonNetwork.CurrentRoom?.MaxPlayers ?? 10;
                var curRoom = PhotonNetwork.CurrentRoom;
                if (curRoom != null)
                {
                    var propsProp = curRoom.GetType().GetProperty("CustomProperties");
                    object props = propsProp?.GetValue(curRoom, null);
                    if (props is System.Collections.IDictionary dict && dict.Contains("gameMode"))
                    {
                        string gm = dict["gameMode"]?.ToString();
                        if (!string.IsNullOrEmpty(gm))
                        {
                            string[] parts = gm.Split(';');
                            mapName = parts[0];
                            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                                queueName = parts[1];
                        }
                    }
                }
            }
        }
        catch { }

        bool isInRoom = inRoom;
        int pCount = playerCount;
        int mPlayers = maxPlayers;
        string rName = roomName;
        string qName = queueName;
        string mName = mapName;
        DateTime? startTime = _discordStartTime;
        var client = _discordClient;

        if (_discordUpdatePending) return;
        _discordUpdatePending = true;

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                string joinSecret = Convert.ToBase64String(Encoding.UTF8.GetBytes(rName));
                var presence = new DiscordRPC.RichPresence
                {
                    Details = qName,
                    Assets = new DiscordRPC.Assets
                    {
                        LargeImageKey = "gt_logo",
                        LargeImageText = mName,
                        SmallImageKey = isInRoom ? "online" : "offline",
                        SmallImageText = isInRoom ? rName : "Idle"
                    },
                    Party = isInRoom ? new DiscordRPC.Party
                    {
                        ID = rName,
                        Size = pCount,
                        Max = mPlayers
                    } : null,
                    Secrets = isInRoom ? new DiscordRPC.Secrets
                    {
                        Join = joinSecret
                    } : null,
                    Timestamps = startTime.HasValue ? new DiscordRPC.Timestamps { Start = startTime.Value } : null,
                    Buttons = new[]
                    {
                        new DiscordRPC.Button { Label = "Join Our Discord", Url = "https://discord.gg/itzgame" }
                    }
                };
                client.SetPresence(presence);
            }
            catch { }
            finally { _discordUpdatePending = false; }
        });
    }

    private void CycleTheme()
    {
        _themeIndex = (_themeIndex + 1) % Themes.Length;
        SaveInt("theme", _themeIndex);
        _skin = null;
        _roundedBg = null;
        _updatePopupBg = null;
        _bgDirty = true;
        BuildButtonTextures();
        PlaySound(_buttonClip);
    }

    private void ToggleGui()
    {
        if (_animatingOut) return;
        if (_showGui) { _animatingOut = true; _animatingIn = false; PlaySound(_openClip); }
        else { _showGui = true; _showSettings = false; _showMisc = false; _animatingIn = true; _animatingOut = false; _animProgress = 0f; PlaySound(_openClip); }
    }

    private void OnGUI()
    {
        if (_notchAnimProgress > 0.001f) DrawPlaytime();
        if (_showUpdatePopup) DrawUpdatePopup();

        if (!_showGui && _animProgress <= 0f) return;

        if (_animatingIn) { _animProgress += Time.unscaledDeltaTime / AnimDuration; if (_animProgress >= 1f) { _animProgress = 1f; _animatingIn = false; } }
        else if (_animatingOut) { _animProgress -= Time.unscaledDeltaTime / AnimDuration; if (_animProgress <= 0f) { _animProgress = 0f; _animatingOut = false; _showGui = false; return; } }

        float eased = 1f - Mathf.Pow(1f - _animProgress, 3f);
        float animScale = Mathf.Lerp(0.6f, 1f, eased);
        float alpha = Mathf.Lerp(0f, 1f, eased);

        _gradientTime += Time.unscaledDeltaTime;

        if (_bgDirty) { UpdateRoundedBg(); _bgDirty = false; }
        RebuildSkin();

        GUI.skin = _skin;
        GUI.color = new Color(1f, 1f, 1f, alpha);

        Vector2 center = new Vector2(_windowRect.x + _windowRect.width / 2f, _windowRect.y + _windowRect.height / 2f);
        float w = 440f * animScale * _guiScale;
        float h = 520f * animScale * _guiScale;
        _windowRect = new Rect(center.x - w / 2f, center.y - h / 2f, w, h);

        float sw = Screen.width;
        float sh = Screen.height;
        _windowRect.x = Mathf.Clamp(_windowRect.x, -_windowRect.width + S(60), sw - S(60));
        _windowRect.y = Mathf.Clamp(_windowRect.y, 0, sh - S(60));

        if (_roundedBg != null)
            GUI.DrawTexture(_windowRect, _roundedBg, ScaleMode.StretchToFill);

        _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, "");

        GUI.color = Color.white;

        if (CurrentTheme.Name == "Snow" && _showGui) DrawSnow();
    }

    private void DrawPlaytime()
    {
        if (_notchAnimProgress <= 0.001f) return;

        string text;
        if (_notchAnimProgress < 1f)
        {
            text = _notchCachedText;
        }
        else if (_showFps)
        {
            text = $"FPS: {Mathf.RoundToInt(1f / Time.unscaledDeltaTime)}";
        }
        else
        {
            float elapsed = Time.time - _playtimeStart;
            int h = (int)(elapsed / 3600f);
            int m = (int)((elapsed % 3600f) / 60f);
            int s = (int)(elapsed % 60f);
            text = $"Playtime {h:D2}:{m:D2}:{s:D2}";
        }

        Theme th = CurrentTheme;
        float w = NS(180);
        float ht = NS(34);
        int radius = (int)(ht / 2f);

        float eased = 1f - Mathf.Pow(1f - _notchAnimProgress, 3f);
        float slideY = Mathf.Lerp(-ht - S(10), NS(4), eased);

        Rect r = new Rect((Screen.width - w) / 2f, slideY, w, ht);

        if (_playtimeNotchTex == null || _playtimeNotchRadius != radius || _playtimeNotchThemeIdx != _themeIndex || _playtimeNotchLastScale != _notchScale)
        {
            if (_playtimeNotchTex != null) UnityEngine.Object.Destroy(_playtimeNotchTex);
            _playtimeNotchTex = RoundedGradientTex(256, 64, new Color(th.BgTopDark.r, th.BgTopDark.g, th.BgTopDark.b, 0.95f), new Color(th.BgBotDark.r, th.BgBotDark.g, th.BgBotDark.b, 0.95f), radius);
            _playtimeNotchRadius = radius;
            _playtimeNotchThemeIdx = _themeIndex;
            _playtimeNotchLastScale = _notchScale;
        }

        GUI.color = new Color(1f, 1f, 1f, eased);
        GUI.DrawTexture(r, _playtimeNotchTex, ScaleMode.StretchToFill);

        GUI.color = Color.white;
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = (int)NS(14),
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = new Color(1f, 1f, 1f, eased);
        GUI.Label(r, text, style);
        GUI.color = Color.white;
    }

    private Rect _updatePopupRect = new Rect(0, 0, 440f, 520f);

    private Texture2D _updatePopupBg;

    private void DrawUpdatePopup()
    {
        _updatePopupRect.x = (Screen.width - _updatePopupRect.width) / 2f;
        _updatePopupRect.y = (Screen.height - _updatePopupRect.height) / 2f;

        GUI.color = Color.white;
        _updatePopupRect = GUI.Window(999999, _updatePopupRect, (id) =>
        {
            Theme th = Themes[_themeIndex % Themes.Length];
            if (_updatePopupBg == null)
            {
                _updatePopupBg = SolidTex(th.BgTopDark);
                _updatePopupBg.hideFlags = HideFlags.HideAndDontSave;
            }
            GUI.DrawTexture(new Rect(0, 0, _updatePopupRect.width, _updatePopupRect.height), _updatePopupBg);

            GUILayout.Space(S(40));
            var prevAlign = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUI.skin.label.fontSize = (int)S(22);
            Color prevCol = GUI.skin.label.normal.textColor;
            GUI.skin.label.normal.textColor = Color.white;
            GUILayout.Label("<b>There's a new version!</b>");
            GUI.skin.label.fontSize = (int)S(16);
            GUILayout.Space(S(12));
            GUILayout.Label($"You have: <color=red>{PluginVersion}</color>");
            GUILayout.Label($"Latest: <color=green>{_latestVersion}</color>");
            GUILayout.Space(S(30));

            Rect okRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(S(52)), GUILayout.ExpandWidth(true));
            bool okHover = okRect.Contains(Event.current.mousePosition);
            GUI.DrawTexture(okRect, SolidTex(okHover ? th.BtnHover : th.BtnNormal));
            GUI.skin.label.fontSize = (int)S(20);
            GUI.skin.label.normal.textColor = Color.white;
            GUI.Label(okRect, "<b>OK</b>", GUI.skin.label);
            GUI.skin.label.normal.textColor = prevCol;
            GUI.skin.label.alignment = prevAlign;

            if (Event.current.type == EventType.MouseDown && okRect.Contains(Event.current.mousePosition))
            {
                _showUpdatePopup = false;
                _updatePopupBg = null;
                PlaySound(_buttonClip);
                Event.current.Use();
            }

            GUI.DragWindow(new Rect(0, 0, _updatePopupRect.width, 30));
        }, "Update Available");
    }

    private float S(float v) => v * _guiScale;
    private float NS(float v) => v * _notchScale;

    private void DrawWindow(int id)
    {
        GUILayout.Space(S(14));
        DrawTitle();
        GUILayout.Space(S(4));

        bool inRoom = PhotonNetwork.InRoom;
        Rect dcRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(S(32)), GUILayout.ExpandWidth(true));
        Theme dcTheme = CurrentTheme;
        bool dcHover = dcRect.Contains(Event.current.mousePosition);
        float blend = CurrentTheme.Name == "Snow" ? 0.7f : 0.35f;
        Color redMix = Color.Lerp(dcTheme.BtnNormal, new Color(0.65f, 0.12f, 0.12f), blend);
        Color redMixH = Color.Lerp(dcTheme.BtnHover, new Color(0.9f, 0.2f, 0.2f), blend);
        Color dcCol = dcHover ? redMixH : redMix;
        GUI.DrawTexture(dcRect, SolidTex(dcCol));
        var dcPrevAlign = GUI.skin.label.alignment;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        Color dcPrevCol = GUI.skin.label.normal.textColor;
        GUI.skin.label.normal.textColor = Color.white;
        GUI.skin.label.fontSize = (int)S(14);
        GUI.Label(dcRect, "Disconnect", GUI.skin.label);
        GUI.skin.label.normal.textColor = dcPrevCol;
        GUI.skin.label.alignment = dcPrevAlign;
        if (dcHover && Event.current.type == EventType.MouseDown)
        {
            PlaySound(_buttonClip);
            if (inRoom) PhotonNetwork.Disconnect();
            Event.current.Use();
        }

        var prev = GUI.skin.label.alignment;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label("<size=13><color=#CCCCCC>F1 toggle</color></size>");
        GUI.skin.label.alignment = prev;

        if (_showMisc) { DrawMisc(); }
        else if (_showSettings) { DrawSettings(); }
        else
        {
            GUILayout.Space(S(16));

            if (GUILayout.Button("Morning", GUILayout.Height(S(60))))
            { PlaySound(_buttonClip); ApplyTime(TimePreset.Morning); }

            if (GUILayout.Button("Day", GUILayout.Height(S(60))))
            { PlaySound(_buttonClip); ApplyTime(TimePreset.Day); }

            if (GUILayout.Button("Noon", GUILayout.Height(S(60))))
            { PlaySound(_buttonClip); ApplyTime(TimePreset.Noon); }

            if (GUILayout.Button("Night", GUILayout.Height(S(60))))
            { PlaySound(_buttonClip); ApplyTime(TimePreset.Night); }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Miscellaneous", GUILayout.Height(S(44))))
            { PlaySound(_buttonClip); _showMisc = true; }

            if (GUILayout.Button("Settings", GUILayout.Height(S(44))))
            { PlaySound(_buttonClip); _showSettings = true; }
        }

        GUI.DragWindow(new Rect(0, 0, _windowRect.width, S(30)));

        if (GUIUtility.hotControl != 0)
        {
            SaveFloat("winx", _windowRect.x);
            SaveFloat("winy", _windowRect.y);
        }
    }

    private void DrawSettings()
    {
        int totalPages = 2;

        GUILayout.Space(S(12));

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("<", GUILayout.Width(S(50)), GUILayout.Height(S(32)))) { PlaySound(_buttonClip); _settingsPage = (_settingsPage - 1 + totalPages) % totalPages; }
        GUILayout.Space(S(12));
        var prevAlign = GUI.skin.label.alignment;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label($"<size=18><b>Settings ({_settingsPage + 1}/{totalPages})</b></size>");
        GUI.skin.label.alignment = prevAlign;
        GUILayout.Space(S(12));
        if (GUILayout.Button(">", GUILayout.Width(S(50)), GUILayout.Height(S(32)))) { PlaySound(_buttonClip); _settingsPage = (_settingsPage + 1) % totalPages; }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(S(16));

        if (_settingsPage == 0)
        {
            GUILayout.Label("<size=14>Theme:</size>");
            GUILayout.Space(S(4));
            if (GUILayout.Button($">>  {CurrentTheme.Name}  <<", GUILayout.Height(S(52)))) CycleTheme();

            GUILayout.Space(S(12));

            DrawSlider("Menu Scale", ref _guiScale, 0.5f, 2f, 0);

            GUILayout.Space(S(8));

            DrawSlider("Menu Rounding", ref _cornerRadius, 0f, 20f, 1);

            GUILayout.Space(S(8));

            DrawSlider("Notch Scale", ref _notchScale, 0.5f, 2f, 2);

            GUILayout.Space(S(8));

            DrawToggle("Discord RPC", ref _discordEnabled, "discord");
            if (_discordEnabled && _discordClient == null) InitDiscord();
            else if (!_discordEnabled && _discordClient != null) ShutdownDiscord();
        }
        else if (_settingsPage == 1)
        {
            bool telemetryDisabled = !_webhookEnabled;
            DrawToggle("Disable Telemetry", ref telemetryDisabled, "telemetry");
            _webhookEnabled = !telemetryDisabled;
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Back", GUILayout.Height(S(44)))) { PlaySound(_buttonClip); _showSettings = false; _settingsPage = 0; }
    }

    private void DrawMisc()
    {
        int totalPages = 2;

        GUILayout.Space(S(12));

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("<", GUILayout.Width(S(50)), GUILayout.Height(S(32)))) { PlaySound(_buttonClip); _miscPage = (_miscPage - 1 + totalPages) % totalPages; }
        GUILayout.Space(S(12));
        var prevAlign = GUI.skin.label.alignment;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUILayout.Label($"<size=18><b>Misc ({_miscPage + 1}/{totalPages})</b></size>");
        GUI.skin.label.alignment = prevAlign;
        GUILayout.Space(S(12));
        if (GUILayout.Button(">", GUILayout.Width(S(50)), GUILayout.Height(S(32)))) { PlaySound(_buttonClip); _miscPage = (_miscPage + 1) % totalPages; }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.Space(S(16));

        if (_miscPage == 0)
        {
            DrawToggle("Toggle Rain", ref _rainEnabled, "rain");

            GUILayout.Space(S(8));

            DrawToggle("Show Playtime", ref _showPlaytime, "playtime");
            if (_showPlaytime && _showFps) { _showFps = false; SaveInt("showfps", 0); }

            GUILayout.Space(S(8));

            DrawToggle("Show FPS", ref _showFps, "showfps");
            if (_showFps && _showPlaytime) { _showPlaytime = false; SaveInt("playtime", 0); }

            GUILayout.Space(S(8));

            DrawToggle("WASD Fly", ref _wadFlyEnabled, "wadfly");
            if (!_wadFlyEnabled) _flyLastPosition = Vector3.zero;

            if (_wadFlyEnabled) GUILayout.Space(S(8));
            if (_wadFlyEnabled) DrawSlider("Fly Speed", ref _flySpeed, 1f, 60f, 3);
        }
        else if (_miscPage == 1)
        {
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Back", GUILayout.Height(S(44)))) { PlaySound(_buttonClip); _showMisc = false; _miscPage = 0; }
    }

    private void DrawToggle(string label, ref bool value, string saveKey, bool playSound = true)
    {
        Theme th = CurrentTheme;
        Rect trackRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(S(40)), GUILayout.ExpandWidth(true));

        Color bg = value ? th.BtnHover : new Color(th.BtnNormal.r, th.BtnNormal.g, th.BtnNormal.b, 0.35f);
        GUI.DrawTexture(trackRect, SolidTex(bg));

        var prevAlign = GUI.skin.label.alignment;
        Color prevCol = GUI.skin.label.normal.textColor;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.skin.label.normal.textColor = Color.white;
        GUI.skin.label.fontSize = (int)S(16);
        GUI.Label(trackRect, label, GUI.skin.label);
        GUI.skin.label.normal.textColor = prevCol;
        GUI.skin.label.alignment = prevAlign;

        Event evt = Event.current;
        if (evt.type == EventType.MouseDown && trackRect.Contains(evt.mousePosition))
        {
            value = !value;
            if (playSound) PlaySound(_buttonClip);
            SaveInt(saveKey, value ? 1 : 0);
            evt.Use();
        }
    }

    private void DrawSlider(string label, ref float value, float min, float max, int sliderId)
    {
        Theme th = CurrentTheme;
        float normalized = Mathf.Clamp01((value - min) / (max - min));

        GUILayout.Label($"<size=14>{label}: {value:F1}</size>");
        GUILayout.Space(S(4));

        Rect trackRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(S(20)), GUILayout.ExpandWidth(true));
        float thumbSize = S(24);
        float halfThumb = thumbSize / 2f;
        float usableWidth = trackRect.width - thumbSize;

        Color trackBg = new Color(th.BtnNormal.r, th.BtnNormal.g, th.BtnNormal.b, 0.3f);
        GUI.DrawTexture(trackRect, SolidTex(trackBg));

        Rect fillRect = trackRect;
        fillRect.width = halfThumb + normalized * usableWidth;
        GUI.DrawTexture(fillRect, SolidTex(th.BtnNormal));

        float thumbCX = trackRect.x + halfThumb + normalized * usableWidth;

        EnsureThumbCircle(th.BtnHover);
        Rect thumbRect = new Rect(thumbCX - halfThumb, trackRect.y + (trackRect.height - thumbSize) / 2f, thumbSize, thumbSize);
        GUI.DrawTexture(thumbRect, _thumbCircleTex, ScaleMode.StretchToFill);

        Event evt = Event.current;

        if (evt.type == EventType.MouseDown && trackRect.Contains(evt.mousePosition))
        {
            _draggingSlider = true;
            _activeSliderId = sliderId;
            evt.Use();
        }

        if (_draggingSlider && _activeSliderId == sliderId && (evt.type == EventType.MouseDrag || evt.type == EventType.MouseUp))
        {
            float mouseX = evt.mousePosition.x;
            float newNorm = Mathf.Clamp01((mouseX - trackRect.x - halfThumb) / usableWidth);
            float newVal = min + newNorm * (max - min);
            newVal = Mathf.Round(newVal * 10f) / 10f;
            if (Mathf.Abs(newVal - value) > 0.001f)
            {
                value = newVal;
                SaveFloat(sliderId == 0 ? "scale" : sliderId == 1 ? "round" : sliderId == 2 ? "notchscale" : "flyspeed", value);
                _skin = null;
                _roundedBg = null;
                _bgDirty = true;
            }
            if (evt.type == EventType.MouseUp) _draggingSlider = false;
            evt.Use();
        }
    }

    /* ─── snow ─── */

    private void InitSnow()
    {
        _snowX = new float[50];
        _snowY = new float[50];
        _snowSpeed = new float[50];
        _snowSize = new float[50];
        _snowWobble = new float[50];
        for (int i = 0; i < 50; i++) ResetSnowflake(i);
    }

    private void ResetSnowflake(int i)
    {
        float ww = _windowRect.width > 0 ? _windowRect.width : 440f;
        _snowX[i] = UnityEngine.Random.Range(0f, ww);
        _snowY[i] = UnityEngine.Random.Range(-30f, -5f);
        _snowSpeed[i] = UnityEngine.Random.Range(80f, 200f);
        _snowSize[i] = UnityEngine.Random.Range(1.5f, 3.5f);
        _snowWobble[i] = UnityEngine.Random.Range(0.5f, 2f);
    }

    private void DrawSnow()
    {
        if (_snowTex == null)
        {
            _snowTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            _snowTex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            _snowTex.Apply();
            _snowTex.hideFlags = HideFlags.HideAndDontSave;
        }

        GUI.BeginGroup(_windowRect);

        float dt = Time.unscaledDeltaTime;
        float t = Time.unscaledTime;
        float ww = _windowRect.width;
        float wh = _windowRect.height;

        for (int i = 0; i < _snowX.Length; i++)
        {
            _snowY[i] += _snowSpeed[i] * dt;
            _snowX[i] += Mathf.Sin(t * _snowWobble[i] + i) * 30f * dt;

            if (_snowY[i] > wh + 20f || _snowX[i] < -20f || _snowX[i] > ww + 20f) ResetSnowflake(i);

            float sz = _snowSize[i];
            GUI.color = new Color(1f, 1f, 1f, 0.45f);
            GUI.DrawTexture(new Rect(_snowX[i], _snowY[i], sz, sz), _snowTex);
        }

        GUI.EndGroup();
        GUI.color = Color.white;
    }

    /* ─── circle ─── */

    private static Texture2D CircleTex(int size, Color fill, Color edge, int aaPx)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = center - 1f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > radius + aaPx)
                    tex.SetPixel(x, y, new Color(0, 0, 0, 0));
                else if (dist > radius - aaPx)
                    tex.SetPixel(x, y, Color.Lerp(fill, edge, Mathf.Clamp01((dist - (radius - aaPx)) / (aaPx * 2f))));
                else
                    tex.SetPixel(x, y, fill);
            }
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    private void EnsureThumbCircle(Color c)
    {
        if (_thumbCircleTex != null && _lastThumbColor == c) return;
        _lastThumbColor = c;
        _thumbCircleTex = CircleTex(128, c, new Color(c.r, c.g, c.b, 0f), 6);
    }

    /* ─── gradient title ─── */

    private void DrawTitle()
    {
        int fontSize = (int)S(26);
        GUIStyle style = new GUIStyle(GUI.skin.label) { fontSize = fontSize, alignment = TextAnchor.MiddleCenter, richText = true, fontStyle = FontStyle.Bold };

        string partA = "ItzGame's";
        string partB = " TimeChanger";
        GUIStyle sA = new GUIStyle(style);
        GUIStyle sB = new GUIStyle(style);
        Vector2 sizeA = sA.CalcSize(new GUIContent(partA));
        Vector2 sizeB = sB.CalcSize(new GUIContent(partB));
        float totalW = sizeA.x + sizeB.x;
        Rect rect = GUILayoutUtility.GetRect(totalW + S(20), fontSize + S(16));
        float startX = rect.x + (rect.width - totalW) / 2f;

        Color cA = new Color(0x6d / 255f, 0xd2 / 255f, 0xf3 / 255f);
        Color cB = new Color(0x00 / 255f, 0x2e / 255f, 0xff / 255f);
        Color prev = GUI.color;
        float cx = startX;

        for (int i = 0; i < partA.Length; i++)
        {
            string ch = partA[i].ToString();
            Vector2 cs = sA.CalcSize(new GUIContent(ch));
            float nx = Mathf.Max((cx - startX) / Mathf.Max(sizeA.x, 1f), 0f);
            float gp = (nx + _gradientTime * 0.12f) % 1.0f;
            Color cc = Color.Lerp(cA, cB, gp);

            GUI.color = new Color(0.05f, 0.01f, 0.10f);
            for (int ox = -2; ox <= 2; ox++)
                for (int oy = -2; oy <= 2; oy++)
                { if (ox == 0 && oy == 0) continue; GUI.Label(new Rect(cx + ox, rect.y + oy, cs.x, rect.height), ch, sA); }

            GUI.color = cc;
            GUI.Label(new Rect(cx, rect.y, cs.x, rect.height), ch, sA);
            cx += cs.x;
        }

        GUI.color = new Color(0.05f, 0.01f, 0.10f);
        for (int ox = -2; ox <= 2; ox++)
            for (int oy = -2; oy <= 2; oy++)
            { if (ox == 0 && oy == 0) continue; GUI.Label(new Rect(cx + ox, rect.y + oy, sizeB.x, rect.height), partB, sB); }
        GUI.color = Color.white;
        GUI.Label(new Rect(cx, rect.y, sizeB.x, rect.height), partB, sB);
        GUI.color = prev;
    }

    /* ─── sound ─── */

    private void ExtractEmbeddedSounds()
    {
        string baseDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location) ?? "";
        string modDir = Path.Combine(baseDir, "TimeChangerMod");
        if (!Directory.Exists(modDir)) Directory.CreateDirectory(modDir);

        var asm = Assembly.GetExecutingAssembly();
        string[] names = asm.GetManifestResourceNames();
        foreach (string name in names)
        {
            if (!name.EndsWith(".ogg")) continue;
            string fileName = name.Replace("TimeChangerMod.", "");
            string outPath = Path.Combine(modDir, fileName);
            if (File.Exists(outPath)) continue;
            try
            {
                using (var stream = asm.GetManifestResourceStream(name))
                using (var fs = File.Create(outPath))
                    stream.CopyTo(fs);
            }
            catch { }
        }
    }

    private IEnumerator LoadSounds()
    {
        string baseDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location) ?? "";
        string modDir = Path.Combine(baseDir, "TimeChangerMod");
        if (!Directory.Exists(modDir)) modDir = baseDir;

        string destinyPath = Path.Combine(modDir, "destiny.ogg");
        string barkPath = Path.Combine(modDir, "bark.ogg");

        if (File.Exists(destinyPath))
        {
            using (var req = UnityWebRequestMultimedia.GetAudioClip("file:///" + destinyPath.Replace("\\", "/"), AudioType.OGGVORBIS))
            { ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = false; yield return req.SendWebRequest(); if (req.result == UnityWebRequest.Result.Success) _openClip = DownloadHandlerAudioClip.GetContent(req); }
        }
        if (File.Exists(barkPath))
        {
            using (var req = UnityWebRequestMultimedia.GetAudioClip("file:///" + barkPath.Replace("\\", "/"), AudioType.OGGVORBIS))
            { ((DownloadHandlerAudioClip)req.downloadHandler).streamAudio = false; yield return req.SendWebRequest(); if (req.result == UnityWebRequest.Result.Success) _buttonClip = DownloadHandlerAudioClip.GetContent(req); }
        }
    }

    private void PlaySound(AudioClip clip) { if (clip != null && _audioSource != null) _audioSource.PlayOneShot(clip, 1f); }

    /* ─── webhook ─── */

    private void SendWebhook(string title, string description, int color)
    {
        try
        {
            string json = $"{{\"embeds\":[{{\"title\":\"{EscapeJson(title)}\",\"description\":\"{EscapeJson(description)}\",\"color\":{color}}}]}}";
            byte[] body = Encoding.UTF8.GetBytes(json);

            new Thread(() =>
            {
                try
                {
                    var req = (HttpWebRequest)WebRequest.Create(WebhookUrl);
                    req.Method = "POST";
                    req.ContentType = "application/json";
                    req.ContentLength = body.Length;
                    using (var stream = req.GetRequestStream()) stream.Write(body, 0, body.Length);
                    using (var resp = req.GetResponse()) resp.Close();
                }
                catch { }
            }) { IsBackground = true }.Start();
        }
        catch { }
    }

    private static string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    /* ─── save / load ─── */

    private void InitSaveDir()
    {
        string tagDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
            "Steam", "steamapps", "common", "Gorilla Tag", "ItzGame's TimeChanger");
        if (!Directory.Exists(tagDir)) Directory.CreateDirectory(tagDir);
        _saveDir = tagDir;
    }

    private string SavePath(string name) => Path.Combine(_saveDir, name + ".txt");

    private void SaveInt(string name, int val) { try { File.WriteAllText(SavePath(name), val.ToString()); } catch { } }
    private void SaveFloat(string name, float val) { try { File.WriteAllText(SavePath(name), val.ToString("F2")); } catch { } }

    private int LoadInt(string name, int def)
    {
        try { string p = SavePath(name); if (File.Exists(p)) return int.Parse(File.ReadAllText(p).Trim()); } catch { }
        return def;
    }
    private float LoadFloat(string name, float def)
    {
        try { string p = SavePath(name); if (File.Exists(p)) return float.Parse(File.ReadAllText(p).Trim()); } catch { }
        return def;
    }

    /* ─── time ─── */

    private void ApplyTime(TimePreset preset)
    {
        try
        {
            BetterDayNightManager instance = BetterDayNightManager.instance;
            if (instance == null) return;
            string[] names = instance.dayNightLightmapNames;
            int count = (names != null && names.Length > 0) ? names.Length : 8;
            int index = Mathf.Clamp(Mathf.FloorToInt((float)preset / 8f * count), 0, count - 1);
            OverrideIndex = index; OverrideActive = true;
            instance.SetTimeOfDay(index, true);
        }
        catch (Exception ex) { Logger.LogError($"[ItzGame's TimeChanger] {ex}"); }
    }

    /* ─── skin ─── */

    private void RebuildSkin()
    {
        if (_skin != null) return;
        _skin = ScriptableObject.CreateInstance<GUISkin>();
        Color textWhite = Color.white;
        Color textDim = new Color(0.80f, 0.78f, 0.88f);

        _skin.window.normal.textColor = textWhite;
        _skin.window.normal.background = _transparentTex;
        _skin.window.onNormal.textColor = textWhite;
        _skin.window.onNormal.background = _transparentTex;
        _skin.window.focused.textColor = textWhite;
        _skin.window.focused.background = _transparentTex;
        _skin.window.onFocused.textColor = textWhite;
        _skin.window.onFocused.background = _transparentTex;
        _skin.window.fontSize = 16;
        _skin.window.padding = new RectOffset(20, 20, 14, 16);
        _skin.window.alignment = TextAnchor.UpperCenter;

        _skin.button.normal.textColor = textWhite;
        _skin.button.normal.background = _btnTex;
        _skin.button.hover.textColor = Color.white;
        _skin.button.hover.background = _btnHoverTex;
        _skin.button.active.textColor = textWhite;
        _skin.button.active.background = _btnActiveTex;
        _skin.button.focused.textColor = textWhite;
        _skin.button.focused.background = _btnTex;
        _skin.button.fontSize = (int)S(20);
        _skin.button.alignment = TextAnchor.MiddleCenter;
        _skin.button.margin = new RectOffset(4, 4, (int)S(4), (int)S(4));

        _skin.label.normal.textColor = textDim;
        _skin.label.fontSize = 13;
        _skin.label.alignment = TextAnchor.MiddleCenter;
        _skin.label.richText = true;
    }

    private void BuildButtonTextures()
    {
        Theme th = CurrentTheme;
        _btnTex = SolidTex(th.BtnNormal);
        _btnHoverTex = SolidTex(th.BtnHover);
        _btnActiveTex = SolidTex(th.BtnActive);
    }

    /* ─── rounded bg ─── */

    private void UpdateRoundedBg()
    {
        Theme th = CurrentTheme;
        float t = (Mathf.Sin(_gradientTime * 0.6f) + 1f) * 0.5f;
        Color top = Color.Lerp(th.BgTopDark, th.BgTopLight, t);
        Color bot = Color.Lerp(th.BgBotDark, th.BgBotLight, t);
        int r = Mathf.Max((int)_cornerRadius, 0);

        _roundedBg = RoundedGradientTex(512, 512, top, bot, r);
        _roundedBg.hideFlags = HideFlags.HideAndDontSave;
        _roundedBg.filterMode = FilterMode.Bilinear;
    }

    /* ─── texture helpers ─── */

    private static Texture2D SolidTex(Color c)
    {
        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        Color[] px = new Color[] { c, c, c, c };
        tex.SetPixels(px); tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        return tex;
    }

    private static Texture2D RoundedGradientTex(int w, int h, Color top, Color bottom, int radius)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        float aa = 2.5f;
        for (int y = 0; y < h; y++)
        {
            Color c = Color.Lerp(bottom, top, (float)y / (h - 1));
            for (int x = 0; x < w; x++)
            {
                Color px = c;
                float dist = 0;
                bool inCorner = false;
                int r = radius;

                if (r > 0)
                {
                    if (x < r && y < r) { dist = Dist2D(x, y, r, r); inCorner = true; }
                    else if (x >= w - r && y < r) { dist = Dist2D(x, y, w - r - 1, r); inCorner = true; }
                    else if (x < r && y >= h - r) { dist = Dist2D(x, y, r, h - r - 1); inCorner = true; }
                    else if (x >= w - r && y >= h - r) { dist = Dist2D(x, y, w - r - 1, h - r - 1); inCorner = true; }
                }

                if (inCorner)
                {
                    float aaStart = Mathf.Max(r - aa, 0f);
                    float aaEnd = r + aa;
                    if (dist > aaEnd) px = new Color(c.r, c.g, c.b, 0f);
                    else if (dist > aaStart)
                    {
                        float t2 = (dist - aaStart) / (aaEnd - aaStart);
                        px = new Color(c.r, c.g, c.b, Mathf.Clamp01(1f - t2));
                    }
                }
                tex.SetPixel(x, y, px);
            }
        }
        tex.Apply();
        tex.hideFlags = HideFlags.HideAndDontSave;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    private static float Dist2D(int x, int y, int cx, int cy)
    {
        float dx = x - cx; float dy = y - cy;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
