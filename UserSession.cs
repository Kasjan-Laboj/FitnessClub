using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitnessClub
{
    /// <summary>
    /// Represents a user session with information about the current employee.
    /// </summary>
    internal class UserSession
    {
        /// <summary>
        /// Gets or sets the ID of the current employee. Default value is -1.
        /// WE GETTING THE ID BUT FOR NOW WE DONT USE IT
        /// </summary>
        public static int CurrentEmployeeId { get; set; } = -1;
    }
}
