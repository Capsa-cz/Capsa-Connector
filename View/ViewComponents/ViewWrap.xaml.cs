using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Capsa_Connector.View.ViewComponents
{
    /// <summary>
    /// Interakční logika pro ViewWrap.xaml
    /// </summary>
    [ContentProperty(nameof(Children))]
    public partial class ViewWrap : UserControl
    {
        public static readonly DependencyPropertyKey ChildrenProperty = DependencyProperty.RegisterReadOnly(
           nameof(Children),  // Prior to C# 6.0, replace nameof(Children) with "Children"
           typeof(UIElementCollection),
           typeof(ViewWrap),
           new PropertyMetadata());
        public UIElementCollection Children
        {
            get { return (UIElementCollection)GetValue(ChildrenProperty.DependencyProperty); }
            private set { SetValue(ChildrenProperty, value); }
        }

        public static readonly DependencyProperty titleProperty =
            DependencyProperty.Register(
            "title",
            typeof(string),
            typeof(ViewWrap),
            new PropertyMetadata("Section separator"));

        public string title
        {
            get { return (string)GetValue(titleProperty); }
            set { SetValue(titleProperty, value); }
        }
        public ViewWrap()
        {
            InitializeComponent();
            title = "Section separator";
            Children = PART_Host.Children;
        }

    }
}
