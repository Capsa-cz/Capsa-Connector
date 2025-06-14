using Capsa_Connector.Core.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capsa_Connector.Model
{
    public class ActiveEditWindowDataStructure
    {
        public string? Name { get; set; }
        public DateTime? EditTime { get; set; }
        public RelayCommand? CloseCommand { get; set; }
        public ActiveFile ActiveFile { get; set; }
    }
}
