using System.Windows;

namespace PryGuard.UI.Controls.TextBoxes;
public class TextBoxProperties : DependencyObject
{
    #region MaxLengthProperty
    public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.RegisterAttached(
            "MaxLength", typeof(int), typeof(TextBoxProperties), new PropertyMetadata(100, OnMaxLengthChanged));

    public static int GetMaxLength(DependencyObject obj)
    {
        return (int)obj.GetValue(MaxLengthProperty);
    }

    public static void SetMaxLength(DependencyObject obj, int value)
    {
        obj.SetValue(MaxLengthProperty, value);
    }

    private static void OnMaxLengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is System.Windows.Controls.TextBox textBox)
        {
            textBox.TextChanged += (sender, args) =>
            {
                if (textBox.Text.Length > GetMaxLength(textBox))
                {
                    textBox.Text = textBox.Text.Substring(0, GetMaxLength(textBox)) + "...";
                }
            };
        }
    }
    #endregion
}
