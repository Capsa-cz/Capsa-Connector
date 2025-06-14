using System.Threading;

namespace Capsa_Connector.Model
{
    public class TaskThreadCommandsStructure
    {
        public string name { get; set; }
        public CancellationTokenSource cancellationTokenSource { get; set; }

        public TaskThreadCommandsStructure(string _name, CancellationTokenSource _cancellationToken)
        {
            name = _name;
            cancellationTokenSource = _cancellationToken;
        }
    }
}
