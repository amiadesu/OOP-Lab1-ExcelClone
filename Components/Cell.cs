namespace ExcelClone.Components
{
    public class ExcelCell
    {
        public string Name { get; set; }
        public string RealValue { get; set; } = "";
        public string DisplayedValue { get; set; } = "";
        public string Formula { get; set; } = "";

        public ExcelCell(string name)
        {
            Name = name;
            // DisplayedValue = name;
        }

        public void Clear()
        {
            RealValue = "";
            DisplayedValue = Name;
            Formula = "";
        }
    }
}