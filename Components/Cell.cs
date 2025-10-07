using ExcelClone.Values;

namespace ExcelClone.Components;
public class ExcelCell {
    public string Formula { get; set; } = "";

    public void Clear()
    {
        Formula = "";
    }
}