using AwesomeAssertions;
using System.Reflection;
using ServiceLib.Enums;
using ServiceLib.Handler.Builder;
using ServiceLib.Helper;
using ServiceLib.Models;
using Xunit;

namespace ServiceLib.Tests.CoreConfig.Context;

public class CoreConfigContextBuilderTests
{
    [Fact]
    public void GetSystemProxyRouteExcludeAddresses_ShouldConvertOnlyRouteCompatibleEntries()
    {
        const string exceptions = "localhost;127.*;10.*;172.16.*;192.168.*;1.2.3.4;fc00::/7;*.example.com;<local>";
        var method = typeof(CoreConfigContextBuilder).GetMethod(
            "GetSystemProxyRouteExcludeAddresses",
            BindingFlags.Static | BindingFlags.NonPublic);

        var result = method?.Invoke(null, [exceptions]).Should().BeAssignableTo<List<string>>().Subject;

        result.Should().Equal(
            "127.0.0.0/8",
            "10.0.0.0/8",
            "172.16.0.0/16",
            "192.168.0.0/16",
            "1.2.3.4",
            "fc00::/7");
    }

    [Fact]
    public void GetTunRouteExcludeAddresses_ShouldSplitPastedSystemProxyList()
    {
        var method = typeof(CoreConfigContextBuilder).GetMethod(
            "GetTunRouteExcludeAddresses",
            BindingFlags.Static | BindingFlags.NonPublic);
        var result = method?.Invoke(null,
                [new List<string> { "localhost;127.*;10.*;192.168.*", "172.16.0.0/12", "fc00::/7" }])
            .Should().BeAssignableTo<List<string>>().Subject;

        result.Should().Equal(
            "127.0.0.0/8",
            "10.0.0.0/8",
            "192.168.0.0/16",
            "172.16.0.0/12",
            "fc00::/7");
    }

    [Fact]
    public void NormalizeTunRouteExcludeAddresses_EmptyList_ShouldUseSystemProxyDefaults()
    {
        var method = typeof(CoreConfigContextBuilder).GetMethod(
            "NormalizeTunRouteExcludeAddresses",
            BindingFlags.Static | BindingFlags.NonPublic);
        var result = method?.Invoke(null, [null, Global.SystemProxyExceptionsWindows])
            .Should().BeAssignableTo<List<string>>().Subject;

        result.Should().Contain(
            "127.0.0.0/8",
            "10.0.0.0/8",
            "172.16.0.0/16",
            "172.31.0.0/16",
            "192.168.0.0/16");
        result.Should().NotContain("localhost");
    }

    [Fact]
    public void NormalizeTunRouteExcludeAddresses_PastedList_ShouldMigrateToCidrs()
    {
        var method = typeof(CoreConfigContextBuilder).GetMethod(
            "NormalizeTunRouteExcludeAddresses",
            BindingFlags.Static | BindingFlags.NonPublic);
        var result = method?.Invoke(null,
                [new List<string> { Global.SystemProxyExceptionsWindows }, Global.SystemProxyExceptionsWindows])
            .Should().BeAssignableTo<List<string>>().Subject;

        result.Should().OnlyContain(address => !address.Contains(';'));
        foreach (var address in result)
        {
            var parseAddress = () => IPNetwork2.Parse(address);
            parseAddress.Should().NotThrow();
        }
    }

    [Fact]
    public async Task ResolveNodeAsync_DirectCycleDependency_ShouldFailWithCycleError()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        CoreConfigTestFactory.BindAppManagerConfig(config);

        var groupAId = NewId("group-a");
        var groupBId = NewId("group-b");
        var groupA = CoreConfigTestFactory.CreatePolicyGroupNode(ECoreType.Xray, groupAId, "group-a", [groupBId]);
        var groupB = CoreConfigTestFactory.CreatePolicyGroupNode(ECoreType.Xray, groupBId, "group-b", [groupAId]);

        await UpsertProfilesAsync(groupA, groupB);

        var context = CoreConfigTestFactory.CreateContext(config, groupA, ECoreType.Xray);
        context.AllProxiesMap.Clear();

        var (_, validatorResult) = await CoreConfigContextBuilder.ResolveNodeAsync(context, groupA, false);

        validatorResult.Success.Should().BeFalse();
        validatorResult.Errors.Should().Contain(msg => ContainsCycleDependencyMessage(msg));
        context.AllProxiesMap.Should().NotContainKey(groupA.IndexId);
        context.AllProxiesMap.Should().NotContainKey(groupB.IndexId);
    }

    [Fact]
    public async Task ResolveNodeAsync_IndirectCycleDependency_ShouldFailWithCycleError()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        CoreConfigTestFactory.BindAppManagerConfig(config);

        var groupAId = NewId("group-a");
        var groupBId = NewId("group-b");
        var groupCId = NewId("group-c");
        var groupA = CoreConfigTestFactory.CreatePolicyGroupNode(ECoreType.Xray, groupAId, "group-a", [groupBId]);
        var groupB = CoreConfigTestFactory.CreatePolicyGroupNode(ECoreType.Xray, groupBId, "group-b", [groupCId]);
        var groupC = CoreConfigTestFactory.CreatePolicyGroupNode(ECoreType.Xray, groupCId, "group-c", [groupAId]);

        await UpsertProfilesAsync(groupA, groupB, groupC);

        var context = CoreConfigTestFactory.CreateContext(config, groupA, ECoreType.Xray);
        context.AllProxiesMap.Clear();

        var (_, validatorResult) = await CoreConfigContextBuilder.ResolveNodeAsync(context, groupA, false);

        validatorResult.Success.Should().BeFalse();
        validatorResult.Errors.Should().Contain(msg => ContainsCycleDependencyMessage(msg));
        context.AllProxiesMap.Should().NotContainKey(groupA.IndexId);
        context.AllProxiesMap.Should().NotContainKey(groupB.IndexId);
        context.AllProxiesMap.Should().NotContainKey(groupC.IndexId);
    }

    [Fact]
    public async Task ResolveNodeAsync_CycleWithValidBranch_ShouldSkipCycleAndKeepValidChild()
    {
        var config = CoreConfigTestFactory.CreateConfig();
        CoreConfigTestFactory.BindAppManagerConfig(config);

        var groupAId = NewId("group-a");
        var groupBId = NewId("group-b");
        var leafId = NewId("leaf");
        var groupA = CoreConfigTestFactory.CreatePolicyGroupNode(ECoreType.Xray, groupAId, "group-a", [groupBId, leafId]);
        var groupB = CoreConfigTestFactory.CreatePolicyGroupNode(ECoreType.Xray, groupBId, "group-b", [groupAId]);
        var leaf = CoreConfigTestFactory.CreateSocksNode(ECoreType.Xray, leafId, "leaf");

        await UpsertProfilesAsync(groupA, groupB, leaf);

        var context = CoreConfigTestFactory.CreateContext(config, groupA, ECoreType.Xray);
        context.AllProxiesMap.Clear();

        var (_, validatorResult) = await CoreConfigContextBuilder.ResolveNodeAsync(context, groupA, false);

        validatorResult.Success.Should().BeTrue();
        validatorResult.Errors.Should().BeEmpty();
        validatorResult.Warnings.Should().Contain(msg => ContainsCycleDependencyMessage(msg));

        context.AllProxiesMap.Should().ContainKey(leaf.IndexId);
        context.AllProxiesMap.Should().ContainKey(groupA.IndexId);
        context.AllProxiesMap.Should().NotContainKey(groupB.IndexId);
        groupA.GetProtocolExtra().ChildItems.Should().Be(leaf.IndexId);
    }

    private static string NewId(string prefix)
    {
        return $"{prefix}-{Guid.NewGuid():N}";
    }

    private static bool ContainsCycleDependencyMessage(string message)
    {
        return message.Contains("cycle dependency", StringComparison.OrdinalIgnoreCase)
               || message.Contains("循环依赖", StringComparison.Ordinal)
               || message.Contains("循環依賴", StringComparison.Ordinal)
               || message.Contains("циклическую зависимость", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task UpsertProfilesAsync(params ProfileItem[] profiles)
    {
        SQLiteHelper.Instance.CreateTable<ProfileItem>();
        foreach (var profile in profiles)
        {
            await SQLiteHelper.Instance.ReplaceAsync(profile);
        }
    }
}
