using System.Collections.Generic;

namespace ExcelClone.Components;

public interface IDependencyTree
{
    public List<string> GetDependencies(string target);
    public List<string> GetDependants(string target);
    public void AddDependency(string source, string target);
    public void RemoveDependency(string source, string target);
    public void ClearDependencies(string target);
}