using Microsoft.Extensions.Options;
using Moq;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.AzureTableStorage.Models;
using Si.IdCheck.Unit.Tests.Helpers;
using Si.IdCheck.Workers.Application.Handlers;
using Si.IdCheck.Workers.Application.Reviewers;
using Si.IdCheck.Workers.Application.Reviewers.Omg;

namespace Si.IdCheck.Unit.Tests;

public class ReviewMatchHandlerTests
{
    private readonly Mock<ICloudCheckApiClient> _mockClient;
    private readonly Mock<IOptionsFactory<ReviewerSettings>> _mockSettings;
    private readonly Mock<IAzureTableStorageService<ReviewMatchLogEntity>> _mockTableStorageService;
    private readonly Mock<Func<string, IReviewer>> _mockReviewMatchFactory;

    public ReviewMatchHandlerTests()
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
                RiskTypes = ["PEP"],
                AssociationTypes = ["Person"],
                RelationshipTypes = ["Relationship"]
            });

        _mockTableStorageService = new Mock<IAzureTableStorageService<ReviewMatchLogEntity>>();

        _mockReviewMatchFactory = new Mock<Func<string, IReviewer>>();

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));
    }

    [Fact]
    public async Task Should_Not_Clear_Match()
    {
        var request = TestUtility.CreateReviewRequestWithHit();
        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Clear_Match()
    {
        var request = TestUtility.CreateReviewRequestWithoutHit();
        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);

        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_Without_BirthYear()
    {
        var request = TestUtility.CreateReviewRequestWithHitWithoutBirthYear();

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);

        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_Without_Person_Of_Interest_BirthYear()
    {
        var request = TestUtility.CreateReviewRequestWithHitWithoutBirthYear();

        request.MatchAssociates.AssociatesInRelationshipFilter = request.MatchAssociates.AssociatesInRelationshipFilter.Select(associate =>
        {
            associate.Response.Matches = associate.Response.Matches.Select(match =>
            {
                match.Dates = match
                    .Dates
                    .Select(date =>
                    {
                        date.Year = "1995";
                        return date;
                    })
                    .ToList();
                return match;
            }).ToList();

            return associate;
        })
        .ToList();

        request.PersonOfInterest.PersonDetail.BirthYear = null;
        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);

        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Clear_Disabled()
    {
        var request = TestUtility.CreateReviewRequestWithoutHit();

        _mockSettings.Setup(x => x.Create("omg"))
            .Returns(() => new ReviewerSettings
            {
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
                RiskTypes = ["PEP"],
                AssociationTypes = ["Person"],
                RelationshipTypes = ["Relationship"]
            });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);

        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Clear_Match_If_No_Associates()
    {
        var request = TestUtility.CreateReviewRequestWithoutHit();

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);

        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Clear_If_Match_Relationship_In_RelationshipsToFilter()
    {
        var request = TestUtility.CreateReviewRequestWithHit();
        request.MatchDetails.Associates = request
            .MatchDetails
            .Associates
            .Select(associate =>
            {
                associate.Relationship = associate.Relationship = "Test Relationship";
                return associate;
            }).ToList();

        _mockSettings.Setup(x => x.Create("omg"))
        .Returns(() => new ReviewerSettings
        {
            RelationshipsToFilter =
            [
                "Test Relationship"
            ],
            RiskTypes = ["PEP"],
            AssociationTypes = ["Person"],
            RelationshipTypes = ["Relationship"]
        });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);

        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }



}
