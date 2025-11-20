using System;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace Jailbreak.Shared;

/// <summary>
/// Root contract exposed by JailbreakCore for third-party plugins.
/// </summary>
public interface IJailbreakApi
{
    /// <summary>Access helpers for registering and querying Last Requests.</summary>
    ILastRequestService LastRequest { get; }

    /// <summary>Access helpers for registering and querying Special Days.</summary>
    ISpecialDayService SpecialDay { get; }

    /// <summary>Utilities for resolving Jailbreak specific player data.</summary>
    IPlayerService Players { get; }

    /// <summary>Native hook helpers exposed by the core plugin.</summary>
    IHookService Hooks { get; }

    /// <summary>Utility helpers shared by the core plugin.</summary>
    IUtilityService Utilities { get; }
}

/// <summary>Surface for Last Request registration and control.</summary>
public interface ILastRequestService
{
    /// <summary>Register a new Last Request implementation.</summary>
    /// <param name="request">Instance to register.</param>
    void Register(ILastRequest request);

    /// <summary>Returns the active Last Request, if any.</summary>
    ILastRequest? GetActive();

    /// <summary>Gets every registered Last Request.</summary>
    IReadOnlyList<ILastRequest> GetAll();

    /// <summary>Forcefully ends the current Last Request, when present.</summary>
    void EndActive();
}

/// <summary>Surface for Special Day registration and control.</summary>
public interface ISpecialDayService
{
    /// <summary>Register a new Special Day implementation.</summary>
    /// <param name="day">Instance to register.</param>
    void Register(ISpecialDay day);

    /// <summary>Unregisters an existing special day.</summary>
    /// <param name="day"></param>
    void Unregister(ISpecialDay day);

    /// <summary>Returns the active Special Day, if any.</summary>
    ISpecialDay? GetActive();

    /// <summary>Gets every registered Special Day.</summary>
    IReadOnlyList<ISpecialDay> GetAll();

    /// <summary>Forcefully ends the active Special Day, when present.</summary>
    void EndActive();
}

/// <summary>Player-centric helpers provided by the core plugin.</summary>
public interface IPlayerService
{
    /// <summary>Gets the Jailbreak player wrapper for a controller.</summary>
    /// <param name="controller">The Swiftly controller reference.</param>
    IJBPlayer? GetPlayer(IPlayer controller);

    /// <summary>Retrieves the current Warden, when available.</summary>
    IJBPlayer? GetWarden();
}

/// <summary>Native hook helpers exposed by the core plugin.</summary>
public interface IHookService
{
    /// <summary>
    /// Subscribes to the core TakeDamage hook.
    /// Returning <see cref="HookResult.Handled"/> skips the original native call.
    /// </summary>
    /// <param name="callback">Delegate invoked when TakeDamage is executed.</param>
    IDisposable HookTakeDamage(Func<DamageHookContext, HookResult> callback);
}

/// <summary>Miscellaneous helpers exposed by the core plugin.</summary>
public interface IUtilityService
{
    /// <summary>
    /// Starts a repeating countdown timer.
    /// </summary>
    /// <param name="seconds">Total duration in seconds.</param>
    /// <param name="onTick">Invoked every second with the remaining time.</param>
    /// <param name="onFinished">Executed once when the countdown finishes.</param>
    CancellationTokenSource StartTimer(int seconds, Action<int> onTick, Action onFinished);

    /// <summary>
    /// Toggles the cells
    /// </summary>
    /// <param name="state">true: open, false: close</param>
    /// <param name="callerName">caller name, leave empty if none.</param>
    public void ToggleCells(bool state, string callerName = "");


    /// <summary>
    /// Prints to all player's chat.
    /// </summary>
    /// <param name="key"> Key that is localized in JailbreakCore translation folder.</param>
    /// <param name="showPrefix">Show prefix, if enabled, select a prefix type.</param>
    /// <param name="prefixType">Prefix type: LR, SD, JB</param>
    /// <param name="args">Arguments</param>
    void PrintToChatAll(string key, bool showPrefix = true, IPrefix prefixType = IPrefix.JB, params object[] args);

    /// <summary>
    /// Prints to all player's chat, but not localized from core messages.
    /// </summary>
    /// <param name="message">Message you wanna send.</param>
    /// <param name="showPrefix">Show prefix?</param>
    /// <param name="prefixType">Prefix type.</param>
    void PrintToChatAll(string message, bool showPrefix, IPrefix prefixType);

    /// <summary>
    /// Prints to all player's center alert.
    /// </summary>
    /// <param name="key">Key that is localized in JailbreakCore translation folder.</param>
    /// <param name="args">Arguments</param>
    void PrintToAlertAll(string key, params object[] args);

    /// <summary>
    /// Prints to all player's center.
    /// </summary>
    /// <param name="key">Key that is localized in JailbreakCore translation folder.</param>
    /// <param name="args">Arguments</param>
    void PrintToCenterAll(string key, params object[] args);

    /// <summary>
    /// Draws and maintains a laser between two players until stopped.
    /// </summary>
    /// <param name="playerA">First endpoint.</param>
    /// <param name="playerB">Second endpoint.</param>
    /// <param name="colorOverride">Leave null to auto-pick a default color.</param>
    /// <param name="width">Laser width.</param>
    /// <param name="heightOffset">Offset from the player's origin (default eye-level).</param>
    /// <param name="durationSeconds">Auto-removal after this many seconds; zero/negative keeps it active.</param>
    Guid DrawLaserBetweenPlayers(IJBPlayer playerA, IJBPlayer playerB, Color? colorOverride = null, float width = 2.0f, float heightOffset = 64.0f, float durationSeconds = 0f);

    /// <summary>
    /// Removes a previously created link laser.
    /// </summary>
    void RemovePlayerLinkLaser(Guid effectId);

    /// <summary>
    /// Removes every active link laser.
    /// </summary>
    void RemoveAllPlayerLinkLasers();

    /// <summary>
    /// Creates a circular beacon animation that follows the target player.
    /// </summary>
    Guid CreatePlayerBeacon(IJBPlayer player, Color? colorOverride = null, int segments = 20, float startRadius = 20.0f, float radiusStep = 10.0f, float durationSeconds = 0.9f, float heightOffset = 5.0f, float stepIntervalSeconds = 0.1f, float width = 2.0f, bool loop = false);

    /// <summary>
    /// Stops a running beacon animation.
    /// </summary>
    void RemovePlayerBeacon(Guid beaconId);

    /// <summary>
    /// Stops and clears all custom player beacons.
    /// </summary>
    void RemoveAllPlayerBeacons();
}

/// <summary>Context supplied to TakeDamage hook subscribers.</summary>
public sealed class DamageHookContext
{
    private CTakeDamageInfo _info;

    public DamageHookContext(CTakeDamageInfo info, IJBPlayer attacker, IJBPlayer victim)
    {
        _info = info;
        Attacker = attacker;
        Victim = victim;
    }

    /// <summary>Mutable reference to the damage payload.</summary>
    public ref CTakeDamageInfo Info => ref _info;

    /// <summary>Jailbreak wrapper for the attacker.</summary>
    public IJBPlayer Attacker { get; }

    /// <summary>Jailbreak wrapper for the victim.</summary>
    public IJBPlayer Victim { get; }
}
