using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StationeryStoreManagementSystem.UI.Controls
{
    /// <summary>
    /// Interaction logic for TextEntry.xaml
    /// </summary>
    public partial class TextEntry : UserControl
    {
        public class EmptyStringValidate : ValidationRule
        {
            public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            {
                if (!string.IsNullOrEmpty((string)value))
                {
                    return new ValidationResult(true, null);
                }

                return new ValidationResult(false, "*required field empty");
            }
        }

        public bool PositiveNumbersOnly
        {
            get { return (bool)GetValue(PositiveNumbersOnlyProperty); }
            set { SetValue(PositiveNumbersOnlyProperty, value); }
        }
        public static readonly DependencyProperty PositiveNumbersOnlyProperty =
            DependencyProperty.Register("PositiveNumbersOnly", typeof(bool), typeof(TextEntry), new PropertyMetadata(false));

        public bool NumbersOnly
        {
            get { return (bool)GetValue(NumbersOnlyProperty); }
            set { SetValue(NumbersOnlyProperty, value); }
        }
        public static readonly DependencyProperty NumbersOnlyProperty =
            DependencyProperty.Register("NumbersOnly", typeof(bool), typeof(TextEntry), new PropertyMetadata(false));


        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TextEntry), new PropertyMetadata(false));


        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText",
                                        typeof(string),
                                        typeof(TextEntry),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public string? Text
        {
            get { return (string)GetValue(TextProperty); }
            set 
            {
                if (value?.Trim().Length == 0)
                    value = null;
                SetValue(TextProperty, value); 
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                                        typeof(string),
                                        typeof(TextEntry),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register("MaxLength",
                                        typeof(int),
                                        typeof(TextEntry),
                                        new PropertyMetadata(0));




        public bool IsRequired
        {
            get { return (bool)GetValue(IsRequiredProperty); }
            set 
            {
                SetValue(IsRequiredProperty, value);
            }
        }
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.Register("IsRequired", typeof(bool), typeof(TextEntry), new PropertyMetadata(false, IsRequiredCallback));


        private static void IsRequiredCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            Binding binding = BindingOperations.GetBinding(((TextEntry)dependencyObject).TextBoxText, TextBox.TextProperty);
            binding.ValidationRules.Clear();
            if (((TextEntry)dependencyObject).IsRequired == true)
                binding.ValidationRules.Add(new EmptyStringValidate() { ValidatesOnTargetUpdated = true });
        }

        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }
        public static readonly DependencyProperty TextWrappingProperty =
            DependencyProperty.Register("TextWrapping", typeof(TextWrapping), typeof(TextEntry), new PropertyMetadata(TextWrapping.NoWrap));



        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public static readonly DependencyProperty AcceptsReturnProperty =
            DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(TextEntry), new PropertyMetadata(false));


        public TextEntry()
        {
            InitializeComponent();
            IsRequired = (bool)GetValue(IsRequiredProperty);
        }
        private static readonly Regex postiveNumbersRegex = new Regex("[^0-9]+");
        private static readonly Regex numbersRegex = new Regex("-?[^0-9]+");
        private bool MatchesRules(string text)
        {
            if (NumbersOnly==true)
                return !numbersRegex.IsMatch(text);
            else if (PositiveNumbersOnly == true)
                return !postiveNumbersRegex.IsMatch(text);
            else
                return true;
        }
        private void TextBoxText_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !MatchesRules(e.Text);
        }

        private void TextBoxText_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!MatchesRules(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
