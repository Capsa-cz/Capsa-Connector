using Capsa_Connector.Core.Bases;

namespace Capsa_Connector.Model;

public class WorkspaceDashboardStructure
{
    public string? Title { get; set; }
    public string? Key { get; set; }
    public RelayCommand? ActionCommand { get; set; }
    public bool IsConnected { get; set; }
    public string? Letter { get; set; }
}