using System.Windows;
using System.Windows.Controls;

namespace Capsa_Connector.View.ViewComponents
{
    /// <summary>
    /// Interakční logika pro SectionSeparator.xaml
    /// </summary>
    public partial class SectionSeparator : UserControl
    {
        public static readonly DependencyProperty separatorTextProperty =
            DependencyProperty.Register(
           "separatorText",
           typeof(string),
           typeof(SectionSeparator),
           new PropertyMetadata("Section separator"));

        public string separatorText
        {
            get { return (string)GetValue(separatorTextProperty); }
            set { SetValue(separatorTextProperty, value); }
        }
        public SectionSeparator()
        {
            InitializeComponent();
            separatorText = "Section separator";
        }
    }
}
