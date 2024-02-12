using Microsoft.Extensions.Options;
using Moq;
using Si.IdCheck.ApiClients.CloudCheckzz;
using Si.IdCheck.ApiClients.CloudCheckzz.Constants;
using Si.IdCheck.ApiClients.CloudCheckzz.Models.Requests;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.AzureTableStorage.Models;
using Si.IdCheck.Workers.Application.Handlers;
using Si.IdCheck.Workers.Application.Reviewers;
using Si.IdCheck.Workers.Application.Reviewers.Omg;
using Si.IdCheck.Workers.Application.Unit.Tests.Helpers;

namespace Si.IdCheck.Workers.Application.Unit.Tests;

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


    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Mother_BirthYear_Is_Lesser_Than_POI_BirthYear()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var motherAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Mother);
        var motherBirthYear = "1990";
        var motherDetails = TestUtility.CreatePersonDetails(motherAssociate.Peid, motherBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, motherAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(motherAssociate.Peid == request.Peid ? motherDetails : matchDetails)));


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Mother_BirthYear_Is_Empty()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var motherBirthYear = string.Empty;
        var motherAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Mother);
        var motherDetails = TestUtility.CreatePersonDetails(motherAssociate.Peid, motherBirthYear!);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, motherAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(motherAssociate.Peid == request.Peid ? motherDetails : matchDetails)));


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Father_BirthYear_Is_Lesser_Than_POI_BirthYear()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var fatherAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Father);
        var fatherBirthYear = "1990";
        var fatherDetails = TestUtility.CreatePersonDetails(fatherAssociate.Peid, fatherBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, fatherAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(fatherAssociate.Peid == request.Peid ? fatherDetails : matchDetails)));


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Father_BirthYear_Is_Empty()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var fatherBirthYear = string.Empty;
        var fatherAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Father);
        var fatherDetails = TestUtility.CreatePersonDetails(fatherAssociate.Peid, fatherBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, fatherAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(fatherAssociate.Peid == request.Peid ? fatherDetails : matchDetails)));


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Son_BirthYear_Is_Greater_Than_POI_BirthYear()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var sonAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Son);
        var sonBirthYear = "2000";
        var sonDetails = TestUtility.CreatePersonDetails(sonAssociate.Peid, sonBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, sonAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(sonDetails.Peid == request.Peid ? sonDetails : matchDetails)));


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Son_BirthYear_Is_Empty()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var sonBirthYear = string.Empty;
        var sonAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Son);
        var sonDetails = TestUtility.CreatePersonDetails(sonAssociate.Peid, sonBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, sonAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(sonDetails.Peid == request.Peid ? sonDetails : matchDetails)));


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Daughter_BirthYear_Is_Greater_Than_POI_BirthYear()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var daughterAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Son);
        var daughterBirthYear = "2000";
        var daughterDetails = TestUtility.CreatePersonDetails(daughterAssociate.Peid, daughterBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, daughterAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(daughterDetails.Peid == request.Peid ? daughterDetails : matchDetails)));


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Daughter_BirthYear_Is_Empty()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var daughterBirthYear = string.Empty;
        var daughterAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Son);
        var daughterDetails = TestUtility.CreatePersonDetails(daughterAssociate.Peid, daughterBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, daughterAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(daughterDetails.Peid == request.Peid ? daughterDetails : matchDetails)));


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Associate_Is_In_RelationshipsFilter()
    {
        var matchPeid = new Random().Next(0, 100000);
        var testRelationship = Guid.NewGuid().ToString();
        var associateBirthYear = "1995";
        var associate = TestUtility.CreateAssociate(testRelationship);
        var associateDetails = TestUtility.CreatePersonDetails(associate.Peid, associateBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, associateBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, associateBirthYear, associate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(associate.Peid == request.Peid ? associateDetails : matchDetails)));

        _mockSettings.Setup(x => x.Create("omg"))
            .Returns(() => new ReviewerSettings
            {
                RelationshipsToFilter =
                [
                    testRelationship
                ],
                RiskTypes = ["PEP"],
                AssociationTypes = ["Person"],
                RelationshipTypes = ["Relationship"]
            });


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_POI_BirthYear_Is_Empty()
    {
        var matchPeid = new Random().Next(0, 100000);
        var testRelationship = Guid.NewGuid().ToString();
        var associateBirthYear = "1995";
        var personOfInterestBirthYear = string.Empty;
        var associate = TestUtility.CreateAssociate(testRelationship);
        var associateDetails = TestUtility.CreatePersonDetails(associate.Peid, associateBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, associateBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, associateBirthYear, associate);
        var personOfInterest = TestUtility.CreatePersonOfInterest(personOfInterestBirthYear);

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(associate.Peid == request.Peid ? associateDetails : matchDetails)));

        _mockSettings.Setup(x => x.Create("omg"))
            .Returns(() => new ReviewerSettings
            {
                RelationshipsToFilter =
                [
                    testRelationship
                ],
                RiskTypes = ["PEP"],
                AssociationTypes = ["Person"],
                RelationshipTypes = ["Relationship"]
            });


        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Clear_Match_If_No_Associates()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(matchDetails)));

        _mockSettings.Setup(x => x.Create(It.IsAny<string>()))
            .Returns(() => new ReviewerSettings
            {
                ClearEnabled = true
            });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Clear_Match_If_Associates_Not_In_Relationship_Filter()
    {
        var associate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.BrotherInLaw);
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(matchDetails)));

        _mockSettings.Setup(x => x.Create(It.IsAny<string>()))
            .Returns(() => new ReviewerSettings
            {
                ClearEnabled = true,
                RelationshipsToFilter = [
                    "WIFE",
                    "HUSBAND",
                    "BROTHER",
                    "SISTER",
                    "SON",
                    "DAUGHTER",
                    "MOTHER",
                    "FATHER"
                ]
            });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Clear_Match_If_Mother_BirthYear_Is_Greater_Than_POI_BirthYear()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var motherAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Mother);
        var motherBirthYear = "2000";
        var motherDetails = TestUtility.CreatePersonDetails(motherAssociate.Peid, motherBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, motherAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(motherAssociate.Peid == request.Peid ? motherDetails : matchDetails)));

        _mockSettings.Setup(x => x.Create(It.IsAny<string>()))
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
                ]
            });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Clear_Match_If_Father_BirthYear_Is_Greater_Than_POI_BirthYear()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var fatherAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Father);
        var fatherBirthYear = "2015";
        var fatherDetails = TestUtility.CreatePersonDetails(fatherAssociate.Peid, fatherBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, fatherAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(fatherAssociate.Peid == request.Peid ? fatherDetails : matchDetails)));

        _mockSettings.Setup(x => x.Create(It.IsAny<string>()))
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
                ]
            });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Clear_Match_If_Son_BirthYear_Is_Lesser_Than_POI_BirthYear()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var sonAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Son);
        var sonBirthYear = "1990";
        var sonDetails = TestUtility.CreatePersonDetails(sonAssociate.Peid, sonBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, sonAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(sonDetails.Peid == request.Peid ? sonDetails : matchDetails)));

        _mockSettings.Setup(x => x.Create(It.IsAny<string>()))
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
                ]
            });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Clear_Match_If_Daughter_BirthYear_Is_Lesser_Than_POI_BirthYear()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var daughterAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Son);
        var daughterBirthYear = "1978";
        var daughterDetails = TestUtility.CreatePersonDetails(daughterAssociate.Peid, daughterBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, daughterAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(daughterDetails.Peid == request.Peid ? daughterDetails : matchDetails)));

        _mockSettings.Setup(x => x.Create(It.IsAny<string>()))
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
                ]
            });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_Not_Clear_Match_If_Clear_Disabled()
    {
        var matchPeid = new Random().Next(0, 100000);
        var matchBirthYear = "1995";
        var daughterAssociate = TestUtility.CreateAssociate(CloudCheckRelationshipConsts.Son);
        var daughterBirthYear = "1978";
        var daughterDetails = TestUtility.CreatePersonDetails(daughterAssociate.Peid, daughterBirthYear);
        var match = TestUtility.CreateMatch(matchPeid, matchBirthYear);
        var matchDetails = TestUtility.CreatePersonDetails(match.Peid, matchBirthYear, daughterAssociate);
        var personOfInterest = TestUtility.CreatePersonOfInterest("1995");

        personOfInterest.Matches = [match];

        _mockClient.Setup(x => x.LookupPeidAsync(It.IsAny<PeidLookupRequest>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns<PeidLookupRequest, string, string>((request, _, _) =>
                Task.FromResult(TestUtility.CreatePersonDetailsResponse(daughterDetails.Peid == request.Peid ? daughterDetails : matchDetails)));

        _mockSettings.Setup(x => x.Create(It.IsAny<string>()))
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
                ]
            });

        _mockReviewMatchFactory.Setup(x => x("omg"))
            .Returns(new OmgReviewer(_mockClient.Object, _mockSettings.Object, _mockTableStorageService.Object));

        var request = TestUtility.CreateReviewMatchRequest(personOfInterest, match);

        var handler = new ReviewMatchHandler(_mockReviewMatchFactory.Object);
        await handler.Handle(request, CancellationToken.None);

        _mockClient.Verify(x => x.ReviewMatchAsync(It.IsAny<ReviewMatchRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);

        _mockTableStorageService.Verify(x => x.InsertOrMergeAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
