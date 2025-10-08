using System.Collections.Generic;

namespace ExcelClone.Components;

public class DependencyTree : IDependencyTree
{
    private readonly Dictionary<string, List<string>> _dependencies = new();
    private readonly Dictionary<string, List<string>> _dependants = new();

    public List<string> GetDependencies(string target)
    {
        return _dependencies.ContainsKey(target) ? new List<string>(_dependencies[target]) : new List<string>();
    }

    public List<string> GetDependants(string target)
    {
        return _dependants.ContainsKey(target) ? new List<string>(_dependants[target]) : new List<string>();
    }
    
    public void AddDependency(string source, string target)
    {
        if (!_dependencies.ContainsKey(source))
            _dependencies[source] = new List<string>();

        if (!_dependencies[source].Contains(target))
            _dependencies[source].Add(target);

        if (!_dependants.ContainsKey(target))
            _dependants[target] = new List<string>();

        if (!_dependants[target].Contains(source))
            _dependants[target].Add(source);
    }

    public void RemoveDependency(string source, string target)
    {
        if (_dependencies.ContainsKey(source))
            _dependencies[source].Remove(target);

        if (_dependants.ContainsKey(target))
            _dependants[target].Remove(source);
    }

    public void ClearDependencies(string target)
    {
        var dependencies = new List<string>(GetDependencies(target));
        foreach (var dependency in dependencies)
        {
            RemoveDependency(target, dependency);
        }
    }
}