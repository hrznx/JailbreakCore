using Jailbreak.Shared;
using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.Natives;

namespace JailbreakCore;

public class LastRequest(ISwiftlyCore core)
{
    private readonly ISwiftlyCore _Core = core;
    private readonly List<ILastRequest> Requests = new();
    private ILastRequest? ActiveRequest;
    private CancellationTokenSource? PrepTimer;

    public IReadOnlyList<ILastRequest> GetRequests() => Requests;
    public ILastRequest? GetActiveRequest() => ActiveRequest;

    private static bool IsPrepTimeActive = false;

    public void Register(ILastRequest request)
    {
        Requests.Add(request);
    }
    public void SelectRequest(ILastRequest request, JBPlayer guardian, JBPlayer prisoner, string weaponName, string weaponId)
    {
        if (ActiveRequest != null)
        {
            prisoner.Print(IHud.Chat, "last_request_aleardy_active", null, 0, true, IPrefix.LR);
            return;
        }

        ActiveRequest = request;
        ActiveRequest.Prisoner = prisoner;
        ActiveRequest.Guardian = guardian;
        ActiveRequest.SelectedWeaponName = weaponName;
        ActiveRequest.SelectedWeaponID = weaponId;

        int prepDelay = 0;

        PrepTimer = _Core.Scheduler.RepeatBySeconds(1, () =>
        {
            prepDelay--;

            if (prepDelay <= 0)
            {
                StartRequest(guardian, prisoner);
                request.IsPrepTimerActive = false;
                IsPrepTimeActive = false;

                JailbreakCore.Extensions.StopAllPlayerLinkLasers();
                JailbreakCore.Extensions.StopAllPlayerBeacons();

                PrepTimer?.Cancel();
                PrepTimer = null;
            }
            else
            {
                request.IsPrepTimerActive = true;
                IsPrepTimeActive = true;

                JailbreakCore.Extensions.StartPlayerLinkLaser(prisoner, guardian, durationSeconds: 100);


                prisoner.Print(IHud.Html, "last_request_starting_html", null, 5, false, IPrefix.LR, request.Name, prepDelay, guardian.Controller.PlayerName);

                guardian.Print(IHud.Html, "last_request_starting_html", null, 5, false, IPrefix.LR, request.Name, prepDelay, prisoner.Controller.PlayerName);
            }
        });
    }
    private void StartRequest(IJBPlayer guardian, IJBPlayer prisoner)
    {
        JBPlayer? activeWarden = JailbreakCore.JBPlayerManagement.GetWarden();
        if (activeWarden != null)
            activeWarden.SetWarden(false);

        ActiveRequest?.Start(guardian, prisoner);
        if (ActiveRequest != null)
        {
            JailbreakCore.Extensions.PrintToChatAll("last_request_started", true, IPrefix.LR, ActiveRequest.Name, ActiveRequest.SelectedType!);
        }
    }
    public void EndRequest(IJBPlayer? winner = null, IJBPlayer? loser = null)
    {
        if (ActiveRequest != null)
        {
            ActiveRequest.End(winner, loser);
            ActiveRequest.Prisoner = null;
            ActiveRequest.Guardian = null;
            ActiveRequest = null;
        }

        PrepTimer?.Cancel();
        PrepTimer = null;
    }
    public void OnPlayerDeath(IJBPlayer player)
    {
        if (ActiveRequest == null)
            return;

        if (player == ActiveRequest.Prisoner)
        {
            EndRequest(ActiveRequest.Guardian, ActiveRequest.Prisoner);
        }
        else if (player == ActiveRequest.Guardian)
        {
            EndRequest(ActiveRequest.Prisoner, ActiveRequest.Guardian);
        }
    }
    public HookResult OnTakeDamage(CTakeDamageInfo info, JBPlayer attacker, JBPlayer victim)
    {
        if (ActiveRequest == null)
            return HookResult.Continue;

        if ((attacker != ActiveRequest.Prisoner && attacker != ActiveRequest.Guardian) ||
            (victim != ActiveRequest.Prisoner && victim != ActiveRequest.Guardian))
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        if (IsPrepTimeActive)
        {
            info.Damage = 0;
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }
}
