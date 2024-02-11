namespace Si.IdCheck.Workers.Application.ServiceBus;
public static class OngoingMonitoringAlertMessages
{
    public abstract class BaseAlertOngoingMonitoringAlertMessage
    {
        public string ClientId { get; set; }
    }
    public class GetAssociations : BaseAlertOngoingMonitoringAlertMessage;

    public class GetAssociation : BaseAlertOngoingMonitoringAlertMessage
    {
        public string AssociationReference { get; set; }
    }

    public class ReviewMatch : BaseAlertOngoingMonitoringAlertMessage
    {
        public string AssociationReference { get; set; }
        public string MatchId { get; set; }
        public int Peid { get; set; }
        public string PersonOfInterestBirthYear { get; set; }
    }
}
