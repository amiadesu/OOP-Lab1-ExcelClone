

using ExcelClone.Components;
using ExcelClone.Constants;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace ExcelClone.Utils;

public static class UIGenerator
{
    public static Label GenerateCell(ExcelCell excelCell, int col, int row, ref AbsoluteLayout gridLayout)
    {
        var cell = new Label
        {
            Text = excelCell.Value.ToString(),
            WidthRequest = Literals.cellWidth,
            HeightRequest = Literals.cellHeight,
            FontSize = 10,
            TextColor = ColorConstants.valueColor,
            BackgroundColor = ColorConstants.cellBackgroundColor,
            HorizontalTextAlignment = TextAlignment.Start,
            VerticalTextAlignment = TextAlignment.Center,
            Padding = new Thickness(2, 0, 2, 0)
        };

        var border = new Border
        {
            Stroke = Colors.Black,
            StrokeThickness = 1,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0) },
            Background = Colors.Transparent,
            Content = cell,
            Padding = new Thickness(0)
        };

        gridLayout.Children.Add(border);
        gridLayout.SetLayoutBounds(border, new Rect(
            col * Literals.cellWidth, row * Literals.cellHeight, Literals.cellWidth, Literals.cellHeight
        ));
        gridLayout.SetLayoutBounds(cell, new Rect(
            col * Literals.cellWidth, row * Literals.cellHeight, Literals.cellWidth, Literals.cellHeight
        ));

        return cell;
    }

    public static Label GenerateColumnHeader(string name, ref StackLayout columnLayout)
    {
        var header = new Label
        {
            Text = name,
            WidthRequest = Literals.cellWidth,
            HeightRequest = Literals.cellHeight,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorConstants.columnLabelColor,
            BackgroundColor = ColorConstants.columnLabelBackgroundColor,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        columnLayout.Children.Add(header);

        return header;
    }

    public static Label GenerateRowHeader(string name, ref StackLayout rowLayout)
    {
        var header = new Label
        {
            Text = name,
            WidthRequest = Literals.cellWidth / 2.5,
            HeightRequest = Literals.cellHeight,
            FontSize = 11,
            FontAttributes = FontAttributes.Bold,
            TextColor = ColorConstants.rowLabelColor,
            BackgroundColor = ColorConstants.rowLabelBackgroundColor,
            HorizontalTextAlignment = TextAlignment.Center,
            VerticalTextAlignment = TextAlignment.Center
        };

        rowLayout.Children.Add(header);

        return header;
    }

    public static Button GenerateRecentFileButton(string fileName, ref VerticalStackLayout verticalStackLayout)
    {
        var button = new Button
        {
            Text = fileName,
            BackgroundColor = Colors.Gray,
            HeightRequest = 40
        };

        verticalStackLayout.Children.Add(button);

        return button;
    }
}