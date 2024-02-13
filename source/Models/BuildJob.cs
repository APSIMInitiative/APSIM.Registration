using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APSIM.Registration.Models
{
    /// <summary>A class for holding info about an APSIM classic build.</summary>
    public class BuildJob
    {
        /// <summary>
        /// ID of the job.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// User who submitted the job.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Build description/title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// ID of the bug addressed by this build.
        /// </summary>
        public long BugID { get; set; }

        /// <summary>
        /// Did the build pass?
        /// </summary>
        public bool Pass { get; set; }

        /// <summary>
        /// Start time of the build.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Finish time of the build.
        /// </summary>
        public DateTime? FinishTime { get; set; }

        /// <summary>
        /// Number of diffs in build.
        /// </summary>
        public int? NumDiffs { get; set; }

        /// <summary>
        /// Revision number.
        /// </summary>
        public int RevisionNumber { get; set; }

        /// <summary>
        /// ID of the Jenkins job, or -1 if not built on Jenkins.
        /// </summary>
        public long JenkinsID { get; set; }

        /// <summary>
        /// Pull request ID.
        /// </summary>
        public int? PullRequestID { get; set; }
    }
}
