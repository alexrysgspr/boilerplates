namespace Si.IdCheck.ApiClients.Verifidentity.Constants;
public static class VerifidentityDecisionConsts
{
    /// <summary>
    /// Indicates that this match is not considered as a risk, but you want to be alerted if changes to the matched profile occur later.
    /// </summary>
    public const string Cleared = "CLEARED";
    /// <summary>
    /// Indicates that this match is not considered as a risk and you do not want to be alerted again, even if changes to the matched profile occur later.
    /// </summary>
    public const string PermanentlyCleared = "PERMANENTLY_CLEARED";
    /// <summary>
    /// Indicates that this matched profile is considered a risk. Alerts will be raised against a Match on any further updates to the matched profile.
    /// </summary>
    public const string Confirmed = "CONFIRMED";
    /// <summary>
    /// Indicates that this matched profile is considered a risk. This is a variant of the CONFIRMED state, which can be used to record different states in the user’s decision process. Alerts will be raised against a Match on any further updates to the matched profile.
    /// </summary>
    public const string ConfirmedRiskMitigated = "CONFIRMED_RISK_MITIGATED";
}
