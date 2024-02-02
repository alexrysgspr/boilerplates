namespace Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

public class PeidLookupResponse
{
    public ResponseDetails Response { get; set; }
    public RequestDetails Request { get; set; }
}

public class ResponseDetails
{
    public IList<MatchDetails> Matches { get; set; }
    public int Count { get; set; }
}

public class MatchDetails
{
    public ContentDetails Content { get; set; }
    public PersonalDetails Details { get; set; }
    public IList<DateDetails> Dates { get; set; }
    public IList<AssociateDetails> Associates { get; set; }
    public int Peid { get; set; }
    public IList<string> Images { get; set; }
    public IList<PeidLookupResponseCountry> Countries { get; set; }
}

public class ContentDetails
{
    public string Status { get; set; }
    public IList<PrimaryOccupationDetails> PrimaryOccupation { get; set; }
    public string Description { get; set; }
    public IList<PreviousRoleDetails> PreviousRoles { get; set; }
    public IList<string> Sources { get; set; }
}

public class PrimaryOccupationDetails
{
    public string To { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
}

public class PreviousRoleDetails
{
    public string To { get; set; }
    public string Category { get; set; }
    public string Title { get; set; }
    public string From { get; set; }
}

public class NameDetails
{
    public string Name { get; set; }
    public string Type { get; set; }
}

public class PersonalDetails
{
    public string Birthplace { get; set; }
    public IList<NameDetails> Names { get; set; }
    public string Gender { get; set; }
    public bool Deceased { get; set; }
}

public class DateDetails
{
    public string Type { get; set; }
    public string Date { get; set; }
    public string Year { get; set; }
}

public class AssociateDetails
{
    public string Relationship { get; set; }
    public string Description1 { get; set; }
    public int Peid { get; set; }
    public string Description2 { get; set; }
}

public class PeidLookupResponseCountry
{
    public string Name { get; set; }
    public string Type { get; set; }
}

public class RequestDetails
{
    public int Peid { get; set; }
}
