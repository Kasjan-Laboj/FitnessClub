using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessClub
{
    public class TrainingSession
    {
        public int Id { get; set; }
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }  // Nowa właściwość
        public int ClientId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime DateTime { get; set; }
    }
}
