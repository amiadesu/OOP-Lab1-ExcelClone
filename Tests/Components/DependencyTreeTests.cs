using ExcelClone.Components;

namespace Tests.Components;

public class DependencyTreeTests
{
    private readonly IDependencyTree _dependencyTree;
    
    public DependencyTreeTests()
    {
        _dependencyTree = new DependencyTree();
    }

    [Fact]
    public void DependencyVSDependantTest()
    {
        _dependencyTree.AddDependency("A", "B");
        _dependencyTree.AddDependency("B", "C");

        var bDependencies = _dependencyTree.GetDependencies("B");
        var bDependants = _dependencyTree.GetDependants("B");

        Assert.NotEmpty(bDependencies);
        Assert.NotEmpty(bDependants);
        Assert.Equal("C", bDependencies[0]);
        Assert.Equal("A", bDependants[0]);
    }

    [Fact]
    public void RemoveExistingDependencyTest()
    {
        _dependencyTree.AddDependency("A", "B");
        _dependencyTree.AddDependency("B", "C");

        var aDependenciesBefore = _dependencyTree.GetDependencies("A");
        var aDependantsBefore = _dependencyTree.GetDependants("A");
        var bDependenciesBefore = _dependencyTree.GetDependencies("B");
        var bDependantsBefore = _dependencyTree.GetDependants("B");

        _dependencyTree.RemoveDependency("A", "B");

        var aDependenciesAfter = _dependencyTree.GetDependencies("A");
        var aDependantsAfter = _dependencyTree.GetDependants("A");
        var bDependenciesAfter = _dependencyTree.GetDependencies("B");
        var bDependantsAfter = _dependencyTree.GetDependants("B");

        Assert.NotEqual(aDependenciesBefore, aDependenciesAfter);
        Assert.Equal(aDependantsBefore, aDependantsAfter);
        Assert.Equal(bDependenciesBefore, bDependenciesAfter);
        Assert.NotEqual(bDependantsBefore, bDependantsAfter);
    }

    [Fact]
    public void RemoveNotExistingDependencyTest()
    {
        _dependencyTree.AddDependency("A", "B");
        _dependencyTree.AddDependency("B", "C");

        var aDependenciesBefore = _dependencyTree.GetDependencies("A");
        var aDependantsBefore = _dependencyTree.GetDependants("A");
        var cDependenciesBefore = _dependencyTree.GetDependencies("C");
        var cDependantsBefore = _dependencyTree.GetDependants("C");

        _dependencyTree.RemoveDependency("C", "A");

        var aDependenciesAfter = _dependencyTree.GetDependencies("A");
        var aDependantsAfter = _dependencyTree.GetDependants("A");
        var cDependenciesAfter = _dependencyTree.GetDependencies("C");
        var cDependantsAfter = _dependencyTree.GetDependants("C");

        Assert.Equal(aDependenciesBefore, aDependenciesAfter);
        Assert.Equal(aDependantsBefore, aDependantsAfter);
        Assert.Equal(cDependenciesBefore, cDependenciesAfter);
        Assert.Equal(cDependantsBefore, cDependantsAfter);
    }
}