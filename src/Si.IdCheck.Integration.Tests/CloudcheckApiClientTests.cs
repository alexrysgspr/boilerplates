using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.ApiClients.CloudCheck.Constants;
using Si.IdCheck.ApiClients.CloudCheck.Models.Requests;
using Si.IdCheck.ApiClients.CloudCheck.Models.Responses;

namespace Si.IdCheck.Integration.Tests;

[TestClass]
public class CloudCheckApiClientTests
{
    private CloudCheckApiClient _client;
    private CloudCheckSettings _settings;
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
            AssociationReference = "b660742a-55fd-4594-8169-0c7a224666b1",
            Review = new Review
            {
                Decision = CloudCheckDecisionConsts.Cleared,
                MatchId = "5a176003033ed23ee22269cdaa293982c5443e7bc1f30ec88622e73d2d6ab113",
                Notes = "Not our client"
            }
        };

        var response = await _client.ReviewMatchAsync(request, _settings.ApiKey, _settings.ApiSecret);
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
        _settings = new CloudCheckSettings
        {
            ApiKey = TestContext.Properties["CloudCheckSettings:ApiKey"].ToString(),
            ApiSecret = TestContext.Properties["CloudCheckSettings:ApiSecret"].ToString(),
            BaseUrl = TestContext.Properties["CloudCheckSettings:BaseUrl"].ToString(),
        };


        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(_settings.BaseUrl)
        };

        _client = new CloudCheckApiClient(httpClient);
    }
}