using System.Globalization;
using Jailbreak.Shared;
using Microsoft.Extensions.Logging;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Events;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace JailbreakCore;

public class Extensions(ISwiftlyCore core)
{
    private readonly ISwiftlyCore _Core = core;
    public void PrintToChatAll(string message, bool showPrefix, IPrefix prefixType)
    {
        string prefix = "";
        switch (prefixType)
        {
            case IPrefix.JB:
                prefix = "jb_prefix";
                break;

            case IPrefix.LR:
                prefix = "lr_prefix";
                break;

            case IPrefix.SD:
                prefix = "sd_prefix";
                break;

            default:
                prefix = "";
                break;
        }
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            if (showPrefix)
                player.SendMessage(MessageType.Chat, _Core.Translation.GetPlayerLocalizer(player)[prefix] + message);
            else
                player.SendMessage(MessageType.Chat, message);
        }
    }
    public void PrintToChatAll(string key, bool showPrefix = true, IPrefix prefixType = IPrefix.JB, params object[] args)
    {
        string prefix = "";
        switch (prefixType)
        {
            case IPrefix.JB:
                prefix = "jb_prefix";
                break;

            case IPrefix.LR:
                prefix = "lr_prefix";
                break;

            case IPrefix.SD:
                prefix = "sd_prefix";
                break;

            default:
                prefix = "";
                break;
        }
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            if (showPrefix)
                player.SendMessage(MessageType.Chat, _Core.Translation.GetPlayerLocalizer(player)[prefix] + _Core.Translation.GetPlayerLocalizer(player)[key, args]);
            else
                player.SendMessage(MessageType.Chat, _Core.Translation.GetPlayerLocalizer(player)[key, args]);
        }
    }
    public void PrintToAlertAll(string key, params object[] args)
    {
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            player.SendMessage(MessageType.Alert, _Core.Translation.GetPlayerLocalizer(player)[key, args]);
        }
    }
    public void PrintToCenterAll(string key, params object[] args)
    {
        foreach (var player in _Core.PlayerManager.GetAllPlayers())
        {
            player.SendMessage(MessageType.Center, _Core.Translation.GetPlayerLocalizer(player)[key, args]);
        }
    }
    public void AssignRandomWarden()
    {
        List<IPlayer> validPlayers = _Core.PlayerManager.GetAllPlayers().Where(p => p.Controller?.TeamNum == (int)Team.CT && p.Controller.PawnIsAlive).ToList();

        _Core.Logger.LogDebug("AssignRandomWarden candidates {Count}", validPlayers.Count);

        if (validPlayers.Count == 0)
        {
            _Core.Logger.LogDebug("AssignRandomWarden aborted: no eligible CT players");
            return;
        }

        IPlayer randomPlayer = validPlayers[new Random().Next(validPlayers.Count)];

        if (randomPlayer != null && randomPlayer.Controller.PawnIsAlive == true && randomPlayer.Controller?.TeamNum == (int)Team.CT)
        {
            _Core.Logger.LogDebug("AssignRandomWarden selecting player {Player}", randomPlayer.Controller.PlayerName ?? "<unknown>");
            JBPlayer randomJbPlayer = JailbreakCore.JBPlayerManagement.GetOrCreate(randomPlayer);
            randomJbPlayer.SetWarden(true);

            randomJbPlayer.Print(IHud.Chat, "warden_take", null, 0, true, IPrefix.JB);
        }
        else
        {
            _Core.Logger.LogDebug(
                "AssignRandomWarden rejected player; alive={Alive} team={Team}",
                randomPlayer?.Controller?.PawnIsAlive,
                randomPlayer?.Controller?.TeamNum
            );
        }
    }
    public void ShowInstructorHint(JBPlayer player, string text, int time = 5, float height = -40.0f, float range = -50.0f, bool follow = true,
    bool showOffScren = true, string iconOnScreen = "icon_bulb", string iconOffScreen = "icon_arrow_up", string cmd = "use_binding", bool showTextAlways = false, Color? color = null)
    {
        if (!player.IsValid)
            return;

        var hintColor = color ?? new Color(255, 0, 0);

        var gameInstructor = _Core.ConVar.Find<string>("sv_gameinstructor_enable");
        gameInstructor?.ReplicateToClient(player.Player.PlayerID, "1");

        _Core.Scheduler.NextTick(() =>
        {
            CreateInstructorHint(player, text, time, height, range, follow, showOffScren, iconOnScreen, iconOffScreen, cmd, showTextAlways, hintColor);
        });
    }
    private void CreateInstructorHint(JBPlayer player, string text, int time, float height, float range, bool follow, bool showOffScren, string iconOnScreen, string iconOffScreen, string cmd, bool showTextAlways, Color color)
    {
        var targetIndex = player.Controller.Index.ToString();
        var hintName = $"instructor_hint_{player.Player.PlayerID}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

        var entity = _Core.EntitySystem.CreateEntity<CEnvInstructorHint>();
        if (entity == null)
            return;

        entity.Name = hintName;
        entity.HintTargetEntity = targetIndex;
        entity.Static = follow;
        entity.Timeout = time;
        entity.Caption = text.Replace("\n", " ");
        entity.Color = color;
        entity.ForceCaption = showTextAlways;
        entity.Icon_Onscreen = iconOnScreen;
        entity.Icon_Offscreen = iconOffScreen;
        entity.NoOffscreen = showOffScren;
        entity.Binding = cmd;
        entity.IconOffset = height;
        entity.Range = range;
        entity.LocalPlayerOnly = false;

        entity.AcceptInput("ShowHint", value: "");

        if (time > 0)
        {
            _Core.Scheduler.Delay(time, () =>
            {
                if (entity.IsValid)
                {
                    entity.AcceptInput("Remove", value: "");
                }
            });
        }
    }
    public CancellationTokenSource StartTimer(int seconds, Action<int> onTick, Action onFinished)
    {
        int remaining = seconds;

        CancellationTokenSource? timer = null;

        timer = _Core.Scheduler.Delay(1, () =>
        {
            remaining--;

            if (remaining > 0)
                onTick?.Invoke(remaining);
            else
            {
                onFinished?.Invoke();
                timer?.Cancel();
            }
        });

        return timer;

    }
    public void ToggleBox(bool state, string callerName = "")
    {
        var teammatesEnemies = _Core.ConVar.Find<bool>("mp_teammates_are_enemies");

        teammatesEnemies?.SetInternal(state ? true : false);
        JailbreakCore.g_IsBoxActive = state ? true : false;

        int commandValue = state ? 0 : 1;
        string boxState = state ? $" {Helper.ChatColors.Green}ON{Helper.ChatColors.Default}" : $" {Helper.ChatColors.Red}OFF{Helper.ChatColors.Default}";
        _Core.Engine.ExecuteCommand($"sv_teamid_overhead {commandValue}");

        foreach (var jbPlayer in JailbreakCore.JBPlayerManagement.GetAllPlayers())
        {
            if (!string.IsNullOrEmpty(callerName))
            {
                jbPlayer.Print(IHud.Chat, "box_toggled", null, 0, true, IPrefix.JB, callerName, boxState);
            }
            if (!string.IsNullOrEmpty(JailbreakCore.Config.Sounds.Box.Path) && state)
            {
                jbPlayer.PlaySound(JailbreakCore.Config.Sounds.Box.Path, JailbreakCore.Config.Sounds.Box.Volume);
            }
        }
    }
    public HookResult OnBoxActive(CTakeDamageInfo info, JBPlayer attacker, JBPlayer victim)
    {
        if (JailbreakCore.g_IsBoxActive && attacker.Controller.TeamNum == victim.Controller.TeamNum && victim.Controller.TeamNum != (int)Team.T)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }
        return HookResult.Continue;
    }

    private void ForceEntityInput(string designerName, string input)
    {
        var entities = _Core.EntitySystem.GetAllEntitiesByDesignerName<CBaseEntity>(designerName);
        foreach (var entity in entities)
        {
            if (entity == null || !entity.IsValid)
                continue;

            entity.AcceptInput(input, value: "");
        }
    }

    public void ToggleCells(bool value, string callerName = "")
    {
        JailbreakCore.g_AreCellsOpened = value ? true : false;
        string status = JailbreakCore.g_AreCellsOpened ? $" {Helper.ChatColors.Green}opened{Helper.ChatColors.Default}" : $" {Helper.ChatColors.Red}closed{Helper.ChatColors.Default}";

        if (!string.IsNullOrEmpty(callerName))
            PrintToChatAll("cells_toggled", true, IPrefix.JB, callerName, status);

        if (JailbreakCore.g_AreCellsOpened)
        {
            ForceEntityInput("func_door", "Open");
            ForceEntityInput("func_movelinear", "Open");
            ForceEntityInput("func_door_rotating", "Open");
            ForceEntityInput("prop_door_rotating", "Open");
            ForceEntityInput("func_breakable", "Break");
        }
        else
        {
            ForceEntityInput("func_door", "Close");
            ForceEntityInput("func_movelinear", "Close");
            ForceEntityInput("func_door_rotating", "Close");
            ForceEntityInput("prop_door_rotating", "Close");
        }
    }
    public IMenuAPI CreateMenu(string title, IMenuAPI? parent = null)
    {
        var config = new MenuConfiguration
        {
            Title = title,
            HideTitle = false,
            HideFooter = false,
            PlaySound = true,
            MaxVisibleItems = 5,
            AutoIncreaseVisibleItems = true,
            FreezePlayer = false,
            AutoCloseAfter = 0f
        };

        var keyBinds = new MenuKeybindOverrides
        {
            Select = KeyBind.E,
            Move = KeyBind.S,
            MoveBack = KeyBind.W,
            Exit = KeyBind.Tab
        };

        var menu = _Core.MenusAPI.CreateMenu(
            configuration: config,
            keybindOverrides: keyBinds,
            parent: parent ?? null,
            optionScrollStyle: MenuOptionScrollStyle.CenterFixed,
            optionTextStyle: MenuOptionTextStyle.TruncateEnd
        );

        return menu;
    }
    public void ToggleBunnyhoop(bool state)
    {
        int value = state ? 1 : 0;
        string bhState = state ? "true" : "false";

        string isEnabled = state ?
        $" {Helper.ChatColors.Green}Enabled{Helper.ChatColors.Default}"
        : $" {Helper.ChatColors.Red}Disabled{Helper.ChatColors.Default}";

        _Core.Engine.ExecuteCommand($"sv_cheats {value}");
        _Core.Engine.ExecuteCommand($"sv_autobunnyhopping {bhState}");
        _Core.Engine.ExecuteCommand($"sv_enablebunnyhopping {bhState}");

        PrintToChatAll("bh_toggled", true, IPrefix.JB, isEnabled);
    }
}
