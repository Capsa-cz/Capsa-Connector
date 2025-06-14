using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Capsa_Connector.View.ViewComponents
{
    /// <summary>
    /// Interakční logika pro DeveloperShow.xaml
    /// </summary>
    ///
    [ContentProperty(nameof(Children))]
    public partial class DeveloperShow : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
           nameof(Children),  // Prior to C# 6.0, replace nameof(Children) with "Children"
           typeof(UIElementCollection),
           typeof(DeveloperShow),
           new PropertyMetadata());
        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }

        public static readonly DependencyProperty isDeveloperShowProperty =
            DependencyProperty.Register(
            "isDeveloperShow",
            typeof(bool),
            typeof(DeveloperShow),
            new PropertyMetadata(true));

        public bool isDeveloperShow
        {
            get { return (bool)GetValue(isDeveloperShowProperty); }
            set { SetValue(isDeveloperShowProperty, value); }
        }
        public DeveloperShow()
        {
            InitializeComponent();
            isDeveloperShow = true;
            Children = PART_Host.Children;
        }
    }
}
