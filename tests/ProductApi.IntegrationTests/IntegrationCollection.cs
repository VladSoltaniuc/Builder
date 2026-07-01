// One factory shared across all integration test classes - migrations run once
using Xunit;

namespace ProductApi.IntegrationTests;

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFactory> { }
