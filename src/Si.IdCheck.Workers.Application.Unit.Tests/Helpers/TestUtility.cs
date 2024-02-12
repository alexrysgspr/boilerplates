using Si.IdCheck.ApiClients.CloudCheckzz.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;

namespace Si.IdCheck.Workers.Application.Unit.Tests.Helpers;
public static class TestUtility
{
    public static AssociateDetails CreateAssociate(string relationship)
    {
        return new AssociateDetails
        {
            Peid = new Random().Next(1, 100000),
            Relationship = relationship
        };
    }

    public static MatchDetails CreatePersonDetails(int peid, string birthYear, params AssociateDetails[] associates)
    {
        return new MatchDetails
        {
            Peid = peid,
            Dates = new List<DateDetails>
            {
                new()
                {
                    Year = birthYear,
                    Type = "Date of birth"
                }
            },
            Associates = associates
        };
    }

    public static PeidLookupResponse CreatePersonDetailsResponse(MatchDetails details)
    {
        return new PeidLookupResponse
        {
            Request = new RequestDetails
            {
                Peid = details.Peid
            },
            Response = new ResponseDetails
            {
                Matches = [details]
            }
        };
    }

    public static Match CreateMatch(int peid, string birthYear)
    {
        return new Match
        {
            Type = "Person",
            MatchId = Guid.NewGuid().ToString(),
            Peid = peid,
            BirthDates = [birthYear],
        };
    }
    

    public static GetAssociationResponse CreatePersonOfInterest(string birthYear)
    {
        return new GetAssociationResponse
        {
            AssociationReference = Guid.NewGuid().ToString(),
            PersonDetail = new PersonDetail
            {
                BirthYear = birthYear
            }
        };
    }

    public static ReviewMatch CreateReviewMatchRequest(GetAssociationResponse personOfInterest, Match match)
    {
        return new ReviewMatch
        {
            ClientId = "omg",
            MatchId = match.MatchId,
            PersonOfInterestBirthYear = personOfInterest.PersonDetail.BirthYear,
            Peid = match.Peid,
            AssociationReference = personOfInterest.AssociationReference
        };
    }
}
