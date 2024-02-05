using Microsoft.Extensions.Options;
using Moq;
using Si.IdCheck.ApiClients.CloudCheck;
using Si.IdCheck.AzureTableStorage;
using Si.IdCheck.AzureTableStorage.Models;
using Si.IdCheck.Unit.Tests.Helpers;
using Si.IdCheck.Workers.Application.Handlers;
using Si.IdCheck.Workers.Application.Settings;
namespace Si.IdCheck.Unit.Tests;

public class ReviewMatchHandlerTests
{
    [Fact]
    public async Task Should_Not_Clear_Match()
    {
        var request = TestUtility.CreateReviewRequestWithHit();
        var mockClient = new Mock<ICloudCheckApiClient>();
        var mockTableStorageService = new Mock<IAzureTableStorageService<ReviewMatchLogEntity>>();
        var reviewMatchSettings = Options.Create(new ReviewMatchSettings
        {
            ClearEnabled = true
        });

        var cloudCheckSettings = Options.Create(new CloudCheckSettings());

        var handler = new ReviewMatchHandler(mockClient.Object, mockTableStorageService.Object, cloudCheckSettings, reviewMatchSettings);

        await handler.Handle(request, CancellationToken.None);
        
        mockTableStorageService.Verify(x => x.InsertAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_Clear_Match()
    {
        var request = TestUtility.CreateReviewRequestWithoutHit();
        var mockClient = new Mock<ICloudCheckApiClient>();
        var mockTableStorageService = new Mock<IAzureTableStorageService<ReviewMatchLogEntity>>();
        var reviewMatchSettings = Options.Create(new ReviewMatchSettings
        {
            ClearEnabled = true
        });

        var cloudCheckSettings = Options.Create(new CloudCheckSettings());

        var handler = new ReviewMatchHandler(mockClient.Object, mockTableStorageService.Object, cloudCheckSettings, reviewMatchSettings);

        await handler.Handle(request, CancellationToken.None);

        mockTableStorageService.Verify(x => x.InsertAsync(It.IsAny<ReviewMatchLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
