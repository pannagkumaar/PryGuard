namespace PryGuard.DataModels;
public class PryGuardHistoryItem
{
    public string Time { get; set; }
    public string Description { get; set; }
    public string Link { get; set; }

    public PryGuardHistoryItem(string time, string description, string link)
    {
        Time = time;
        Description = description;
        Link = link;
    }
}

