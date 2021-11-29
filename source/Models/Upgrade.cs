using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APSIM.Registration.Models
{
    /// <summary>
    /// An class encapsulating an upgrade 
    /// </summary>
    public class Upgrade
    {
        /// <summary>
        /// Release date of the upgrade.
        /// </summary>
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// Number/ID of the issue addressed by this upgrade.
        /// </summary>
        public int IssueNumber { get; set; }

        /// <summary>
        /// Title of the issue addressed by this upgrade.
        /// </summary>
        public string IssueTitle { get; set; }

        /// <summary>
        /// URL of the issue addressed by this upgrade.
        /// </summary>
        public string IssueURL { get; set; }

        /// <summary>
        /// URL of the installer for this upgrade.
        /// </summary>
        public string ReleaseURL { get; set; }

        /// <summary>
        /// Revision number.
        /// </summary>
        public uint RevisionNumber { get; set; }

        /// <summary>
        /// Issue Number (obsolete).
        /// </summary>
        // Leaving this here for compatibility reasons.
        [Obsolete("Deprecated in favour of IssueNumber.")]
        public int issueNumber
        {
            get
            {
                return IssueNumber;
            }
            set
            {
                IssueNumber = value;
            }
        }
    }
}
