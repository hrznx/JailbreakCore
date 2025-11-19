namespace Jailbreak.Shared;

public interface ISpecialDay
{
    /// <summary>
    /// Human-friendly name of the special day. Used for display and comparisons.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Short description explaining in chat what the special day changes.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Called when the special day starts. Apply effects, register timers or hooks here.
    /// </summary>
    void Start();

    /// <summary>
    /// Called when the special day ends. Undo effects and clean up resources.
    /// </summary>
    void End();
}
