using ExcelClone.Values;

namespace ExcelClone.Components
{
    public class ExcelCell
    {
        public string Name { get; set; }
        public CellValue Value { get; set; } = new CellValue(CellValueType.Text);
        public string Formula { get; set; } = "";

        public ExcelCell(string name)
        {
            Name = name;
        }

        public void Clear()
        {
            Value.Clear();
            Formula = "";
        }
    }
}