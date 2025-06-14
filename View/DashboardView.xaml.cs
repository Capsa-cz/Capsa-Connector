using System.Windows.Controls;

namespace Capsa_Connector.View
{
    /// <summary>
    /// Interakční logika pro ConnectingView.xaml
    /// </summary>
    public partial class DashboardView : UserControl
    {
        //public ConnectingViewModel CVM { get; set; }
        public DashboardView()
        {
            InitializeComponent();
            //CVM = new ConnectingViewModel();
            //DataContext = CVM;
            //ConnectingViewModel cvm = new ConnectingViewModel();
            //this.DataContext = cvm;
        }


        /*
        public ConnectingViewModel MYCVM { get; set; }
        public ConnectingView(ConnectingViewModel MYCVM)
        {
            this.MYCVM = MYCVM; 
            DataContext = this.MYCVM;
            InitializeComponent();
        }
        */

    }
}
