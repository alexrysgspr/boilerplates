using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Boilerplate.ApiClients.Boilerplate.Integration.Tests;

[TestClass]
public class CloudCheckApiClientTests
{
    private BoilerplateApiClient _client;
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
    public async Task Get_Test()
    {
        
    }


    private void InitializeTest()
    {
        
    }
}
