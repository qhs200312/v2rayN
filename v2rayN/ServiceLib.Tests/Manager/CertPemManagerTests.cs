using AwesomeAssertions;
using ServiceLib.Manager;
using Xunit;

namespace ServiceLib.Tests.Manager;

public class CertPemManagerTests
{
    [Fact]
    public void BuildCertificateChainPolicy_BeforeInitialization_ShouldUseSystemRoots()
    {
        var manager = new CertPemManager();

        manager.BuildCertificateChainPolicy().Should().BeNull();
    }
}
