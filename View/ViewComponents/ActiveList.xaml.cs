using Capsa_Connector.Model;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Capsa_Connector.View.ViewComponents
{
    /// <summary>
    /// Interakční logika pro ActiveList.xaml
    /// </summary>
    public partial class ActiveList : UserControl
    {

        public static readonly DependencyProperty ElementsProperty =
            DependencyProperty.Register(
                "Elements",
                typeof(ObservableCollection<ActiveListElement>),
                typeof(ActiveList),
                new PropertyMetadata(new ObservableCollection<ActiveListElement>()));

        public ObservableCollection<ActiveListElement> Elements
        {
            get { return (ObservableCollection<ActiveListElement>)GetValue(ElementsProperty); }
            set { SetValue(ElementsProperty, value); }
        }
        public ActiveList()
        {
            InitializeComponent();

            Elements = new ObservableCollection<ActiveListElement>
            {
                new ActiveListElement
                {
                    ElementName = "Jméno elementu",
                    ElementCommand = null,
                }
            };
        }
    }
}
