using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;
using Moq;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.AzureTableStorage.Models;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.Workers.Application.Reviewers;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;
using Si.IdCheck.Workers.Application.Handlers;
using Si.IdCheck.Workers.Application.Models.Requests;
using Si.IdCheck.Workers.Application.Settings;
using Si.IdCheck.Workers.Application.Unit.Tests.Helpers;
using Si.IdCheck.Workers.Application.Unit.Tests.Mocks;

namespace Si.IdCheck.Workers.Application.Unit.Tests;
public class GetAssociationHandlerTests
{
    private readonly Mock<ICloudCheckApiClient> _mockClient;
    private readonly Mock<IOptionsFactory<ReviewerSettings>> _mockSettings;
    private readonly Mock<IAzureTableStorageService<ReviewMatchLogEntity>> _mockTableStorageService;
    private readonly Mock<Func<string, IReviewer>> _mockReviewMatchFactory;
    private readonly Mock<IAzureClientFactory<ServiceBusClient>> _mockAzureClientFactoryServiceBus;
    private readonly IOptions<ServiceBusSettings> _mockServiceBusSettings;

    public GetAssociationHandlerTests()
    {
        _mockClient = new Mock<ICloudCheckApiClient>();
        _mockSettings = new Mock<IOptionsFactory<ReviewerSettings>>();

        _mockSettings.Setup(x => x.Create("omg"))
            .Returns(() => new ReviewerSettings
            {
                ClearEnabled = true,
                RelationshipsToFilter =
                [
                    "WIFE",
                    "HUSBAND",
                    "BROTHER",
                    "SISTER",
                    "SON",
                    "DAUGHTER",
                    "MOTHER",
                    "FATHER"
                ],
                RiskTypes = ["RCA"],
                AssociationTypes = ["Person"],
                RelationshipTypes = ["Relationship"]
            });

        _mockTableStorageService = new Mock<IAzureTableStorageService<ReviewMatchLogEntity>>();

        _mockReviewMatchFactory = new Mock<Func<string, IReviewer>>();

        _mockAzureClientFactoryServiceBus = new Mock<IAzureClientFactory<ServiceBusClient>>();

        _mockAzureClientFactoryServiceBus
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns((() => new MockServiceBusClient()));

        _mockServiceBusSettings = Options.Create(new ServiceBusSettings());
    }

    [Fact]
    public async Task Should_Return_Matches_With_RiskTypes_RCA()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var match1RiskTypes = new List<RiskType>()
        {
            new()
            {
                Code = "RCA"
            }
        };

        var match2RiskTypes = new List<RiskType>()
        {
            new()
            {
                Code = "PEP"
            }
        };

        var match3RiskTypes = new List<RiskType>()
        {
            new()
            {
                Code = "RCA"
            },
            new()
            {
                Code = Guid.NewGuid().ToString()
            }
        };


        var match1 = TestUtility.CreateMatch(matchPeid, matchBirthYear, match1RiskTypes);
        var match2 = TestUtility.CreateMatch(matchPeid, matchBirthYear, match2RiskTypes);
        var match3 = TestUtility.CreateMatch(matchPeid, matchBirthYear, match3RiskTypes);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match1, match2, match3];

        _mockClient.Setup(x => x.GetAssociationAsync(It.IsAny<GetAssociationRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(() =>
                Task.FromResult(new GetAssociationResponse
                {
                    Matches = [match1, match2, match3]
                }));

        var request = new GetAssociation
        {
            ClientId = "omg",
            AssociationReference = Guid.NewGuid().ToString()
        };

        var handler = new GetAssociationHandler(_mockClient.Object, _mockSettings.Object, _mockAzureClientFactoryServiceBus.Object, _mockServiceBusSettings);
        var response = await handler.Handle(request, CancellationToken.None);

        Assert.Contains(response.Value.Matches, match => match.RiskTypes.Select(x => x.Code).Contains("RCA"));
    }
}
