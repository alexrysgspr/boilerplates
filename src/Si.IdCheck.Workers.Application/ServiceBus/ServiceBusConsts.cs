namespace Si.IdCheck.Workers.Application.ServiceBus;

public static class ServiceBusConsts
{
    public static string ConnectionStringName = "ServiceBus";
    public static string ClientName = "SwiftId";

    public static class OngoingMonitoringAlerts
    {
        public static class MessageTypes
        {
            public const string GetAssociations = nameof(GetAssociations);
            public const string GetAssociation = nameof(GetAssociation);
            public const string ReviewMatch = nameof(ReviewMatch);
        }
    }
}
