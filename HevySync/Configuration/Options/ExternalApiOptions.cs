namespace HevySync.Configuration.Options;

public class ExternalApiOptions
{
    public const string SectionName = "ExternalApiUrls";

    public string HevyApi { get; set; } = string.Empty;
}