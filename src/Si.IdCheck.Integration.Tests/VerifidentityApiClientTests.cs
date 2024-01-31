using Si.IdCheck.ApiClients.Verifidentity;
using Si.IdCheck.ApiClients.Verifidentity.Constants;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests;
using Si.IdCheck.ApiClients.Verifidentity.Models.Requests.PeidLookup;
using Si.IdCheck.ApiClients.Verifidentity.Models.Responses;

namespace Si.IdCheck.Integration.Tests;

[TestClass]
public class VerifidentityApiClientTests
{
    private VerifidentityApiClient _client;
    private VerifidentitySettings _settings;
    public TestContext TestContext { get; set; }


    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
    }

    [TestInitialize]
    public void Setup()
    {
        InitializeTest();
    }

    [TestMethod]
    public async Task Get_Association_Test()
    {
        var request = new GetAssociationRequest
        {
            AssociationReference = "b660742a-55fd-4594-8169-0c7a224666b1"
        };

        var response = await _client.GetAssociationAsync(request, _settings.ApiKey, _settings.ApiSecret);
    }

    [TestMethod]
    public async Task Get_Associations_Test()
    {
        var request = new GetAssociationsRequest
        {
            Cursor = 0,
            FilterAlertOnly = true,
            PageSize = 1000
        };


        var associations = new List<Association>();

        var isLastPage = false;

        while (!isLastPage)
        {
            var response = await _client.GetAssociationsAsync(request, _settings.ApiKey, _settings.ApiSecret);
            associations.AddRange(response.Associations);

            if (int.TryParse(response.Meta.NextCursor, out var next))
            {
                request.Cursor = next;
            }
            else
            {
                isLastPage = true;
            }
        }
    }

    [TestMethod]
    public async Task Review_Match_Test()
    {
        var request = new ReviewMatchRequest
        {
            AssociationReference = "113f6ed3-78af-417a-a0c9-c441d013752c",
            Review = new Review
            {
                Decision = VerifidentityDecisionConsts.Cleared,
                MatchId = "2dfdf7ba113f6ed3-78af-417a-a0c9-c441d013752c",
                Notes = "Not our client"
            }
        };

        var response = await _client.GetReviewMatchAsync(request, _settings.ApiKey, _settings.ApiSecret);
    }

    [TestMethod]
    public async Task Peid_Lookup_Test()
    {
        var request = new PeidLookupRequest
        {
            Peid = 13231726
        };

        var response = await _client.LookupPeidAsync(request, _settings.ApiKey, _settings.ApiSecret);
    }

    private void InitializeTest()
    {
        _settings = new VerifidentitySettings
        {
            ApiKey = TestContext.Properties["VerifidentitySettings:ApiKey"].ToString(),
            ApiSecret = TestContext.Properties["VerifidentitySettings:ApiSecret"].ToString(),
            BaseUrl = TestContext.Properties["VerifidentitySettings:BaseUrl"].ToString(),
        };


        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        _client = new VerifidentityApiClient(httpClient);
    }
}