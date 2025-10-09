using System;
using System.Collections.Generic;
using ExcelClone.Components;
using ExcelClone.Resources.Localization;
using ExcelClone.Types;
using ExcelClone.Utils;
using ExcelClone.Values;

namespace ExcelClone.Services;

public class SpreadsheetService : ISpreadsheetService
{
    private readonly ICellStorage _cellStorage;
    private readonly IDependencyTree _dependencyTree;
    private readonly IFormulaParserService _formulaParserService;

    public SpreadsheetService(ICellStorage storage, IDependencyTree dependencyTree, IFormulaParserService formulaParserService)
    {
        _cellStorage = storage;
        _dependencyTree = dependencyTree;
        _formulaParserService = formulaParserService;
    }

    public string? UpdateCellFormula(string cellReference, string formula)
    {
        _cellStorage.SetCellFormula(cellReference, formula);

        return Evaluate(cellReference);
    }

    private string? Evaluate(string cellReference)
    {
        var result = _cellStorage.UpdateCellValue(cellReference, _formulaParserService);

        if (result is null)
        {
            return null;
        }

        try
        {
            var error = ProcessResult(cellReference, result);
            if (!string.IsNullOrEmpty(error))
            {
                return error;
            }
        }
        catch (Exception e)
        {
            return e.Message;
        }
        
        return result?.ErrorMessage;
    }

    private string? ProcessResult(string cellReference, ValueEvaluationResult result)
    {
        UpdateDependencies(cellReference, result.Dependencies);

        var error = RecalculateDependants(cellReference);

        return error;
    }

    private void UpdateDependencies(string cellReference, List<string> dependencies)
    {
        _dependencyTree.ClearDependencies(cellReference);

        foreach (var dependency in dependencies)
        {
            _dependencyTree.AddDependency(cellReference, dependency);
        }
    }


    private string? RecalculateDependants(string cellReference)
    {
        var visited = new HashSet<string>();
        var stack = new Stack<string>();
        stack.Push(cellReference);

        while (stack.Count > 0)
        {
            var current = stack.Pop();

            if (visited.Contains(current))
            {
                _cellStorage.SetCellErrorValue(current, CellValueType.RefError);
                
                return DataProcessor.FormatResource(
                    AppResources.CircularDependencyDetected,
                    ("CellName", current)
                );
            }

            visited.Add(current);

            var dependants = _dependencyTree.GetDependants(current);
            foreach (var dependant in dependants)
            {
                var formula = _cellStorage.GetCellFormula(dependant);

                var result = _formulaParserService.Evaluate(formula);
                ProcessResult(dependant, result);

                stack.Push(dependant);
            }
        }

        return null;
    }
}