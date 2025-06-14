using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
namespace Capsa_Connector.View.ViewComponents
{
    /// <summary>
    /// Interakční logika pro SettingsRow.xaml
    /// </summary>
    [ContentProperty(nameof(Children))]
    public partial class SettingsRow : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
           nameof(Children),  // Prior to C# 6.0, replace nameof(Children) with "Children"
           typeof(UIElementCollection),
           typeof(SettingsRow),
           new PropertyMetadata());
        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }


        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "title",
                typeof(string),
                typeof(SettingsRow),
                new PropertyMetadata("Nadpis"));

        public string title
        {
            get { return (string)GetValue(TitleProperty); }
            set
            {
                SetValue(TitleProperty, value);
                Trace.WriteLine($"Title property: {title}");
            }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                "description",
                typeof(string),
                typeof(SettingsRow),
                new PropertyMetadata("Popisek"));

        public string description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public SettingsRow()
        {
            InitializeComponent();
            Children = PART_Host.Children;
        }
    }
}
