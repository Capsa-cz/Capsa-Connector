using Capsa_Connector.Core.Bases;

namespace Capsa_Connector.Model
{
    public class ActiveListElement
    {
        public string? ElementName { get; set; }
        public RelayCommand? ElementCommand { get; set; }
    }
}
