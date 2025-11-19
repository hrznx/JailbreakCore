using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.SchemaDefinitions;

namespace Jailbreak.Shared;

public enum IJBRole
{
    Warden,
    Prisoner,
    Guardian,
    Rebel,
    Freeday,
    None
}
public enum IHud
{
    Chat,
    Center,
    Alert,
    Html,
    InstructorHint,
}
public enum IPrefix
{
    LR,
    SD,
    JB
}
public interface IJBPlayer
{
    /// <summary>
    /// Gets the IJBPlayer Controller
    /// </summary>
    CCSPlayerController Controller { get; }

    /// <summary>
    /// Gets the IJBPlayer Player
    /// </summary>
    IPlayer Player { get; }

    /// <summary>
    /// Gets the IJBPlayer PlayerPawn
    /// </summary>
    CCSPlayerPawn PlayerPawn { get; }

    /// <summary>
    /// Gets the IJBPlayer Pawn
    /// </summary>
    CBasePlayerPawn Pawn { get; }

    /// <summary>
    /// Current role of the player.
    /// </summary>
    IJBRole Role { get; }

    /// <summary>
    /// True if player is currently the Warden
    /// </summary>
    bool IsWarden { get; }

    /// <summary>
    /// True if player is currently Rebel
    /// </summary>
    bool IsRebel { get; }

    /// <summary>
    /// True if player is currently Freeday
    /// </summary>
    bool IsFreeday { get; }

    /// <summary>
    /// True if player && controller && playerpawn and pawn are valid
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Make the player a warden (or remove the warden status).
    /// </summary>
    void SetWarden(bool state);


    /// <summary>
    /// Make the player a rebel (or remove rebel status).
    /// </summary>
    void SetRebel(bool state);

    /// <summary>
    /// Give or remove freeday privileges.
    /// </summary>
    void SetFreeday(bool state);

    /// <summary>
    /// Forcefully set the player's role to the provided value.
    /// </summary>
    void SetRole(IJBRole role);

    /// <summary>
    /// false = turn invisible | true = turn visible
    /// </summary>
    /// <param name="state"></param>
    void SetVisible(bool state);

    /// <summary>
    /// Strips player weapons.
    /// </summary>
    /// <param name="keepKnife">Should keep knife?</param>
    void StripWeapons(bool keepKnife);

    /// <summary>
    /// Send a message to the player. `hud` accepts values like "chat",
    /// "center", "alert", "hint" or "html". Duration is used for html messages.
    /// Note: this uses built in GetPlayerLocalizer(player) from ISwiftlyCore.
    /// </summary>
    void Print(IHud hud, string? key = "", string? message = "", int duration = 0, bool showPrefix = true, IPrefix prefixType = IPrefix.JB, params object[] args);

}
