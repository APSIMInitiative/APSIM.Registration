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
        public string UserName { get; set; }

        /// <summary>
        /// Patch file name.
        /// </summary>
        public string PatchFileName { get; set; }

        /// <summary>
        /// Short patch file name.
        /// </summary>
        public string PatchFileNameShort { get; set; }

        /// <summary>
        /// Link to patch file.
        /// </summary>
        public string PatchFileURL { get; set; }

        /// <summary>
        /// Build description/title.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID of the bug addressed by this build.
        /// </summary>
        public int TaskID { get; set; }

        /// <summary>
        /// Start time of the build.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Amount of time taken by CI server (Bob/Jenkins) to run tests.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Revision number.
        /// </summary>
        public int Revision { get; set; }

        /// <summary>
        /// Version string including revision - e.g. "7.10-r4191".
        /// </summary>
        public string VersionString { get; set; }

        /// <summary>
        /// Xml URL.
        /// </summary>
        /// <remarks>What is this?</remarks>
        public string XmlUrl { get; set; }

        /// <summary>
        /// Status of the windows build.
        /// </summary>
        public string WindowsStatus { get; set; }

        /// <summary>
        /// Number of diffs in windows build.
        /// </summary>
        public int WindowsNumDiffs { get; set; }

        /// <summary>
        /// Link to windows binaries.
        /// </summary>
        public string WindowsBinariesURL { get; set; }

        /// <summary>
        /// Link to windows build tree.
        /// </summary>
        public string WindowsBuildTreeURL { get; set; }

        /// <summary>
        /// Link to diffs in windows build.
        /// </summary>
        public string WindowsDiffsURL { get; set; }

        /// <summary>
        /// Link to details about windows build.
        /// </summary>
        public string WindowsDetailsURL { get; set; }

        /// <summary>
        /// Link to the issue which was addressed by this build.
        /// </summary>
        public string IssueURL { get; set; }

        /// <summary>
        /// Link to windows installer.
        /// </summary>
        public string WindowsInstallerURL { get; set; }

        /// <summary>
        /// Link to windows installer.
        /// </summary>
        /// <remarks>todo: How is this different to <see cref="WindowsInstallerURL"/>?</remarks>
        public string WindowsInstallerFullURL { get; set; }

        /// <summary>
        /// Link to self-extracting archive of 32-bit windows binaries.
        /// </summary>
        public string Win32SFXURL { get; set; }

        /// <summary>
        /// Link to self-extracting archive of 64-bit windows binaries.
        /// </summary>
        public string Win64SFXURL { get; set; }

        /// <summary>
        /// Status of Linux build.
        /// </summary>
        public string LinuxStatus { get; set; }

        /// <summary>
        /// Number of diffs in Linux build.
        /// </summary>
        public int LinuxNumDiffs { get; set; }

        /// <summary>
        /// Link to Linux binaries.
        /// </summary>
        public string LinuxBinariesURL { get; set; }

        /// <summary>
        /// Link to diffs from Linux build.
        /// </summary>
        public string LinuxDiffsURL { get; set; }

        /// <summary>
        /// Link to details about Linux build.
        /// </summary>
        public string LinuxDetailsURL { get; set; }

        /// <summary>
        /// Was this build on Jenkins (t) or Bob (f)?
        /// </summary>
        public bool BuiltOnJenkins { get; set; }

        /// <summary>
        /// ID of the Jenkins job, or -1 if not built on Jenkins.
        /// </summary>
        public int JenkinsID { get; set; }

        /// <summary>
        /// Pull request ID.
        /// </summary>
        public int PullRequestID { get; set; }
    }
}
