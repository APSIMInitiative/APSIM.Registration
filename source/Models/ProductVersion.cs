using System;
using System.IO;

namespace APSIM.Registration.Models
{
    /// <summary>
    /// Encapsulates a version or upgrade of a <see cref="Product"/>.
    /// </summary>
    public class ProductVersion
    {
        /// <summary>
        /// Description or title of the product version.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Version number.
        /// </summary>
        public string Number { get; private set; }

        /// <summary>
        /// Link to more information about the version.
        /// </summary>
        public string InfoLink { get; private set; }

        /// <summary>
        /// Download link for Linux installer.
        /// </summary>
        public string DownloadLinkLinux { get; private set; }

        /// <summary>
        /// Download link for windows installer.
        /// </summary>
        public string DownloadLinkWindows { get; private set; }

        /// <summary>
        /// Download link for mac installer.
        /// </summary>
        public string DownloadLinkMac { get; private set; }

        /// <summary>
        /// Release date of this version.
        /// </summary>
        public DateTime ReleaseDate { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name or description of the version.</param>
        /// <param name="number">Version number.</param>
        /// <param name="infoUrl">Link to more info about this version.</param>
        /// <param name="releaseDate">Releaes date of this version.</param>
        /// <param name="linuxUrl">Link to Linux installer.</param>
        /// <param name="windowsURL">Link to windows installer.</param>
        /// <param name="macUrl">Link to mac installer.</param>
        public ProductVersion(string name, string number, string infoUrl, DateTime releaseDate, string windowsUrl, string linuxUrl, string macUrl)
        {
            Description = name;
            Number = number;
            InfoLink = infoUrl;
            ReleaseDate = releaseDate;
            DownloadLinkLinux = linuxUrl;
            DownloadLinkWindows = windowsUrl;
            DownloadLinkMac = macUrl;
        }

        public ProductVersion(Upgrade upgrade)
        {
            Description = upgrade.IssueTitle;
            // fixme - the version number deduction is a bit of a hack.
            Number = upgrade.ReleaseDate.ToString("yyyy.MM.dd.") + upgrade.IssueNumber;
            InfoLink = upgrade.IssueURL;
            ReleaseDate = upgrade.ReleaseDate;
            DownloadLinkWindows = upgrade.ReleaseURL;
            DownloadLinkLinux = Path.ChangeExtension(upgrade.ReleaseURL, ".deb"); //fixme
            DownloadLinkMac = Path.ChangeExtension(upgrade.ReleaseURL, ".dmg");
        }
    }
}
