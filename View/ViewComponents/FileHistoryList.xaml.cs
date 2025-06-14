using Capsa_Connector.Core.Bases;
using Capsa_Connector.Core.Tools;
using Capsa_Connector.Model;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.View.ViewComponents
{
    /// <summary>
    /// Interakční logika pro FileHistoryList.xaml
    /// </summary>
    public partial class FileHistoryList : UserControl
    {

        public static readonly DependencyProperty DashboardElementsProperty =
            DependencyProperty.Register(
                "DashboardElements",
                typeof(ObservableCollection<DashboardElementStructure>),
                typeof(FileHistoryList),
                new PropertyMetadata(new ObservableCollection<DashboardElementStructure>()));

        public ObservableCollection<DashboardElementStructure> DashboardElements
        {
            get { return (ObservableCollection<DashboardElementStructure>)GetValue(DashboardElementsProperty); }
            set
            {
                ObservableCollection<DashboardElementStructure> elementCollection = value;
                foreach (DashboardElementStructure element in elementCollection)
                {
                    element.ImageToPreviewImage = ExtIcons.getIconToExtension(element.FileExtension ?? "");
                }
                SetValue(DashboardElementsProperty, elementCollection);
            }
        }

        public RelayCommand OpenURL { get; set; }


        public FileHistoryList()
        {
            InitializeComponent();

            DashboardElements = new ObservableCollection<DashboardElementStructure>
            {
                new DashboardElementStructure
                {
                    EditTime = DateTime.Now,
                    FileExtension = "docx",
                    FileName = "Jméno souboru",
                    ImageToPreviewImage = null,
                }
            };

            OpenURL = new RelayCommand((o) =>
            {
                if (o == null) return;

                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = o.ToString(),
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch (Exception ex)
                {
                    v(ex.Message);
                    v(ex.StackTrace.ToString());
                }
                return;
            });
        }
    }
}
