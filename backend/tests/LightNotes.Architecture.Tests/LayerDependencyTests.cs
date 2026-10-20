using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace LightNotes.Architecture.Tests;

public class LayerDependencyTests
{
    private const string Domain = "LightNotes.Domain";
    private const string Application = "LightNotes.Application";
    private const string Infrastructure = "LightNotes.Infrastructure";
    private const string Api = "LightNotes.Api";

    [Fact]
    public void Domain_depends_on_nothing() =>
        AssertNoDependencies(
            Domain,
            Application,
            Infrastructure,
            Api,
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore"
        );

    [Fact]
    public void Application_does_not_depend_on_outer_layers() =>
        AssertNoDependencies(Application, Infrastructure, Api, "Microsoft.AspNetCore", "Npgsql");

    [Fact]
    public void Infrastructure_does_not_depend_on_Api() =>
        AssertNoDependencies(Infrastructure, Api);

    private static void AssertNoDependencies(string assemblyName, params string[] forbidden)
    {
        var result = Types
            .InAssembly(Assembly.Load(assemblyName))
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"{assemblyName} has forbidden dependencies: {string.Join(", ", result.FailingTypes ?? [])}"
        );
    }
}
