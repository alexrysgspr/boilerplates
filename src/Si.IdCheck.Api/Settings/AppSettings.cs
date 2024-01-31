namespace Si.IdCheck.Api.Settings;

public class AppSettings
{
    public string AppStatusAccessKey { get; set; }
    public string KeyVaultUri { get; set; }
    public string Environment { get; set; }
    public bool? IsMaintenance { get; set; }
}
