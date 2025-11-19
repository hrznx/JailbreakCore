namespace LastRequests;

public class LRConfig
{
    public KnifeFight_LR KnifeFight { get; set; } = new();
}
public class KnifeFight_LR
{
    public bool Enable { get; set; } = true;
}
