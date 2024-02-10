using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Models.Responses;

namespace Si.IdCheck.Unit.Tests.Helpers;
public static class TestUtility
{
    public static ReviewMatch CreateReviewRequestWithHit()
    {
        var associationReference = Guid.NewGuid().ToString();
        var matchId = Guid.NewGuid().ToString();
        var random = new Random();
        var matchPeid = random.Next(1, 100000);

        var match = new Match
        {
            Type = "Person",
            MatchId = matchId,
            Peid = matchPeid,
            BirthDates = new List<string>
            {
                "1995"
            },
        };

        var motherPeid = random.Next(1, 100000);
        var sonPeid = random.Next(1, 100000);

        var motherAssociate =
            new AssociateDetails
            {
                Peid = motherPeid,
                Relationship = "Mother"
            };
        var sonAssociate = new AssociateDetails
        {
            Peid = sonPeid,
            Relationship = "Son"
        };

        var matchDetails = new MatchDetails
        {
            Peid = matchPeid,
            Dates = new List<DateDetails>
            {
                new()
                {
                    Year = "1995",
                    Type = "Date of birth"
                }
            },
            Associates = new List<AssociateDetails>
            {
                motherAssociate,
                sonAssociate
            }
        };

        var matchAssociates = new List<PeidLookupResponse>
        {
            new()
            {
                Request = new RequestDetails
                {
                    Peid = matchPeid
                },
                Response = new ResponseDetails
                {
                    Matches = new List<MatchDetails>
                    {
                        new()
                        {
                            Peid = motherAssociate.Peid,
                            Dates = new List<DateDetails>
                            {
                                new()
                                {
                                    Year = "1990",
                                    Type = "Date of birth"
                                }
                            }
                        },
                        new()
                        {
                            Peid = sonAssociate.Peid,
                            Dates = new List<DateDetails>
                            {
                                new()
                                {
                                    Year = "2000",
                                    Type = "Date of birth"
                                }
                            }
                        }
                    }
                }
            }
        };

        var personOfInterest = new GetAssociationResponse
        {
            AssociationReference = associationReference,
            Matches = new List<Match>
            {
                match
            },
            PersonDetail = new PersonDetail
            {
                BirthYear = "1995"
            }
        };

        return new ReviewMatch
        {
            ClientId = "omg",
            PersonOfInterest = personOfInterest,
            Match = match,
            MatchDetails = matchDetails,
            MatchAssociates = new GetMatchAssociatesPersonDetailsResponse
            {
                AssociatesInRelationshipFilter = matchAssociates
            }
        };
    }

    public static ReviewMatch CreateReviewRequestWithoutHit()
    {
        var associationReference = Guid.NewGuid().ToString();
        var matchId = Guid.NewGuid().ToString();
        var random = new Random();
        var matchPeid = random.Next(1, 100000);

        var match = new Match
        {
            Type = "Person",
            MatchId = matchId,
            Peid = matchPeid,
            BirthDates = new List<string>
            {
                "1995"
            },
        };

        var motherPeid = random.Next(1, 100000);
        var sonPeid = random.Next(1, 100000);

        var motherAssociate =
            new AssociateDetails
            {
                Peid = motherPeid,
                Relationship = "Mother"
            };
        var sonAssociate = new AssociateDetails
        {
            Peid = sonPeid,
            Relationship = "Son"
        };

        var matchDetails = new MatchDetails
        {
            Peid = matchPeid,
            Dates = new List<DateDetails>
            {
                new()
                {
                    Year = "1995",
                    Type = "Date of birth"
                }
            },
            Associates = new List<AssociateDetails>
            {
                motherAssociate,
                sonAssociate
            }
        };

        var matchAssociates = new List<PeidLookupResponse>
        {
            new()
            {
                Request = new RequestDetails
                {
                    Peid = matchPeid
                },
                Response = new ResponseDetails
                {
                    Matches = new List<MatchDetails>
                    {
                        new()
                        {
                            Peid = motherAssociate.Peid,
                            Dates = new List<DateDetails>
                            {
                                new()
                                {
                                    Year = "1996",
                                    Type = "Date of birth"
                                }
                            }
                        },
                        new()
                        {
                            Peid = sonAssociate.Peid,
                            Dates = new List<DateDetails>
                            {
                                new()
                                {
                                    Year = "1994",
                                    Type = "Date of birth"
                                }
                            }
                        }
                    }
                }
            }
        };

        var personOfInterest = new GetAssociationResponse
        {
            AssociationReference = associationReference,
            Matches = new List<Match>
            {
                match
            },
            PersonDetail = new PersonDetail
            {
                BirthYear = "1995"
            }
        };

        return new ReviewMatch
        {
            ClientId = "omg",
            PersonOfInterest = personOfInterest,
            Match = match,
            MatchDetails = matchDetails,
            MatchAssociates = new GetMatchAssociatesPersonDetailsResponse
            {
                AssociatesInRelationshipFilter = matchAssociates
            }
        };
    }

    public static ReviewMatch CreateReviewRequestWithHitWithoutBirthYear()
    {
        var associationReference = Guid.NewGuid().ToString();
        var matchId = Guid.NewGuid().ToString();
        var random = new Random();
        var matchPeid = random.Next(1, 100000);

        var match = new Match
        {
            Type = "Person",
            MatchId = matchId,
            Peid = matchPeid,
            BirthDates = new List<string>
            {
                "1995"
            },
        };

        var motherPeid = random.Next(1, 100000);
        var sonPeid = random.Next(1, 100000);

        var motherAssociate =
            new AssociateDetails
            {
                Peid = motherPeid,
                Relationship = "Mother"
            };
        var sonAssociate = new AssociateDetails
        {
            Peid = sonPeid,
            Relationship = "Son"
        };

        var matchDetails = new MatchDetails
        {
            Peid = matchPeid,
            Dates = new List<DateDetails>
            {
                new()
                {
                    Year = "1995",
                    Type = "Date of birth"
                }
            },
            Associates = new List<AssociateDetails>
            {
                motherAssociate,
                sonAssociate
            }
        };

        var matchAssociates = new List<PeidLookupResponse>
        {
            new()
            {
                Request = new RequestDetails
                {
                    Peid = matchPeid
                },
                Response = new ResponseDetails
                {
                    Matches = new List<MatchDetails>
                    {
                        new()
                        {
                            Peid = motherAssociate.Peid,
                            Dates = new List<DateDetails>
                            {
                                new()
                                {
                                    Type = "Date of birth"
                                }
                            }
                        },
                        new()
                        {
                            Peid = sonAssociate.Peid,
                            Dates = new List<DateDetails>
                            {
                                new()
                                {
                                    Type = "Date of birth"
                                }
                            }
                        }
                    }
                }
            }
        };

        var personOfInterest = new GetAssociationResponse
        {
            AssociationReference = associationReference,
            Matches = new List<Match>
            {
                match
            },
            PersonDetail = new PersonDetail
            {
                BirthYear = "1995"
            }
        };

        return new ReviewMatch
        {
            ClientId = "omg",
            PersonOfInterest = personOfInterest,
            Match = match,
            MatchDetails = matchDetails,
            MatchAssociates = new GetMatchAssociatesPersonDetailsResponse
            {
                AssociatesInRelationshipFilter = matchAssociates
            }
        };
    }
}
