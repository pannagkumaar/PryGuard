using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Controls;
using PryGuard.Resources.Commands;
using Microsoft.Xaml.Behaviors;
using PryGuard.UI.Controls.Panels;

namespace PryGuard.UI.Controls.Buttons;
public class InitAddTabBtn
{
    private static Style GetBorderAddTabStyle()
    {
        var borderStyle = new Style(typeof(Border));
        borderStyle.Setters.Add(new Setter(Border.BorderThicknessProperty, new Thickness(1)));
        borderStyle.Setters.Add(new Setter(Border.CornerRadiusProperty, new CornerRadius(20)));
        borderStyle.Setters.Add(new Setter(FrameworkElement.CursorProperty, Cursors.Hand));
        borderStyle.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Transparent));

        var mouseEnterTrigger = new Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = true
        };
        mouseEnterTrigger.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Gray));
        var mouseLeaveTrigger = new Trigger
        {
            Property = UIElement.IsMouseOverProperty,
            Value = false
        };
        mouseLeaveTrigger.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Transparent));

        borderStyle.Triggers.Add(mouseEnterTrigger);
        borderStyle.Triggers.Add(mouseLeaveTrigger);
        _ = new Border
        {
            Width = 100,
            Height = 100,
            Style = borderStyle
        };
        return borderStyle;
    }

    public static AutoStackPanel CreateBtn(Action action)
    {
        var autoStackPanel = new AutoStackPanel
        {
            Margin = new Thickness(2, 0, 0, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Height = 25,
            Width = 25,
            MaxChildWidth = 238
        };

        var eventTrigger = new Microsoft.Xaml.Behaviors.EventTrigger { EventName = "MouseLeftButtonDown" };
        var invokeCommandAction = new InvokeCommandAction
        {
            Command = new RelayCommand(action)
        };
        eventTrigger.Actions.Add(invokeCommandAction);
        Interaction.GetTriggers(autoStackPanel).Add(eventTrigger);

        var border = new Border
        {
            Height = 25,
            Width = 25,
            Style = GetBorderAddTabStyle()
        };

        string xamlContent = @"
    <DrawingImage xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
        <DrawingImage.Drawing>
            <DrawingGroup ClipGeometry='M0,0 V512 H512 V0 H0 Z'>
                <DrawingGroup Opacity='1' Transform='0.1,0,0,-0.1,0,512'>
                    <GeometryDrawing Brush='White' Geometry='F1 M512,512z M0,0z M2471,5103C2447,5094 2408,5068 2385,5045 2305,4966 2310,5037 2310,3858L2310,2810 1262,2810C83,2810 154,2814 75,2735 -23,2637 -23,2483 75,2385 154,2306 83,2310 1262,2310L2310,2310 2310,1262C2310,83 2306,154 2385,75 2483,-23 2637,-23 2735,75 2814,154 2810,83 2810,1262L2810,2310 3858,2310C5037,2310 4966,2306 5045,2385 5143,2483 5143,2637 5045,2735 4966,2814 5037,2810 3858,2810L2810,2810 2810,3858C2810,5037 2814,4966 2735,5045 2666,5115 2566,5136 2471,5103z' />
                </DrawingGroup>
            </DrawingGroup>
        </DrawingImage.Drawing>
    </DrawingImage>";

        var addTabImage = (DrawingImage)XamlReader.Parse(xamlContent);
        var image = new Image
        {
            Margin = new Thickness(1, -1, 0, 0),
            Width = 12,
            Height = 16,
            Source = addTabImage
        };

        border.Child = image;
        autoStackPanel.Children.Add(border);
        return autoStackPanel;
    }
}
