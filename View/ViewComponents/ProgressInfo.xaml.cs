using System.Windows;
using System.Windows.Controls;

namespace Capsa_Connector.View.ViewComponents
{
    /// <summary>
    /// Interakční logika pro ProgressInfo.xaml
    /// </summary>
    public partial class ProgressInfo : UserControl
    {
        public static readonly DependencyProperty imageSourceProperty =
            DependencyProperty.Register(
            "imageSource",
            typeof(string),
            typeof(ProgressInfo),
            new PropertyMetadata("ímageSource"));

        public string imageSource
        {
            get { return (string)GetValue(imageSourceProperty); }
            set { SetValue(imageSourceProperty, value); }
        }

        public static readonly DependencyProperty textProperty =
            DependencyProperty.Register(
            "text",
            typeof(string),
            typeof(ProgressInfo),
            new PropertyMetadata("Text"));

        public string text
        {
            get { return (string)GetValue(textProperty); }
            set { SetValue(textProperty, value); }
        }

        public static readonly DependencyProperty valueProperty =
            DependencyProperty.Register(
            "value",
            typeof(int),
            typeof(ProgressInfo),
            new PropertyMetadata(0));

        public int value
        {
            get { return (int)GetValue(valueProperty); }
            set { SetValue(valueProperty, value); }
        }

        public object imageConverter;

        public ProgressInfo()
        {
            InitializeComponent();
            text = string.Empty;
            imageSource = string.Empty;
            value = 25;
        }
    }
}

