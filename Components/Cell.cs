using ExcelClone.Values;

namespace ExcelClone.Components;

public class SpreadsheetCell {
    public CellValue Value { get; private set; } = new CellValue(CellValueType.Text);
    public string Formula { get; private set; } = "";

    public void SetValue(CellValue value)
    {
        Value = value;
    }

    public void SetFormula(string formula)
    {
        Formula = formula;
    }

    public void Clear()
    {
        Value.Clear();
        Formula = "";
    }
}