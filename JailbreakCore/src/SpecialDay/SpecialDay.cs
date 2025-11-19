using System.ComponentModel;
using Jailbreak.Shared;
using SwiftlyS2.Shared;

namespace JailbreakCore;

public class SpecialDay
{
    private readonly List<ISpecialDay> Days = new();
    private ISpecialDay? ActiveDay;
    private ISpecialDay? PendingDay;
    private int CooldownInRounds = JailbreakCore.Config.SpecialDay.CooldownInRounds;
    public IReadOnlyList<ISpecialDay> GetAllDays() => Days;
    public ISpecialDay? GetActiveDay() => ActiveDay;

    public void Register(ISpecialDay day)
    {
        Days.Add(day);
    }
    public void Unregister(ISpecialDay day)
    {
        Days.Remove(day);
    }
    public void Select(JBPlayer player, string name)
    {
        if (CooldownInRounds > 0)
        {
            player.Print(IHud.Chat, "day_on_cooldown", showPrefix: true, prefixType: IPrefix.SD, args: CooldownInRounds);
            return;
        }

        PendingDay = Days.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (PendingDay != null)
            JailbreakCore.Extensions.PrintToChatAll("special_day_pending", true, IPrefix.SD, player.Controller.PlayerName, PendingDay.Name);

    }
    public void OnRoundStart()
    {
        if (CooldownInRounds > 0)
            CooldownInRounds--;

        if (PendingDay != null)
        {
            ActiveDay = PendingDay;
            PendingDay = null;

            ActiveDay.Start();
            JailbreakCore.Extensions.PrintToChatAll(ActiveDay.Description, showPrefix: true, prefixType: IPrefix.SD);

            CooldownInRounds = JailbreakCore.Config.SpecialDay.CooldownInRounds;
        }
    }
    public void OnRoundEnd()
    {
        ActiveDay?.End();
        ActiveDay = null;
    }
    public void EndDay()
    {
        ActiveDay?.End();
        ActiveDay = null;
    }
}
