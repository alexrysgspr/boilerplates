namespace Boilerplate.Workers.Integration.Tests;

public class Tests : IClassFixture<FactoryBase>
{
    private readonly FactoryBase _factory;

    public Tests(FactoryBase factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Tests1()
    {
        
    }
}