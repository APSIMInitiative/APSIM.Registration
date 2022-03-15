using System;

namespace APSIM.Registration.Models
{
    /// <summary>
    /// Encapsulates an Apsim next gen release.
    /// </summary>
    public class Release
    {
        /// <summary>
        /// Create a new <see cref="Release"/> instance.
        /// </summary>
        /// <param name="releaseDate">Release date.</param>
        /// <param name="issue">Issue number addressed by this release.</param>
        /// <param name="title">Release title/description.</param>
        /// <param name="downloadLinkDebian">Download link for debian installer.</param>
        /// <param name="downloadLinkWindows">Download link for windows installer.</param>
        /// <param name="downloadLinkMacOS">Download linke for macOS installer.</param>
        /// <param name="infoUrl">Link to more info about the release.</param>
        /// <param name="version">Full version number of the release.</param>
        /// <param name="revision">Revision number of the release.</param>
        public Release(DateTime releaseDate, uint issue, string title, string downloadLinkDebian, string downloadLinkWindows, string downloadLinkMacOS, string infoUrl, string version, uint revision)
        {
            ReleaseDate = releaseDate;
            Issue = issue;
            Title = title;
            DownloadLinkDebian = downloadLinkDebian;
            DownloadLinkWindows = downloadLinkWindows;
            DownloadLinkMacOS = downloadLinkMacOS;
            InfoUrl = infoUrl;
            Version = version;
            Revision = revision;
        }

        /// <summary>
        /// Release date.
        /// </summary>
        public DateTime ReleaseDate { get; private init; }

        /// <summary>
        /// Issue number/ID.
        /// </summary>
        public uint Issue { get; private init; }

        /// <summary>
        /// Release title.
        /// </summary>
        public string Title { get; private init; }

        /// <summary>
        /// Download link for Debian installer.
        /// </summary>
        public string DownloadLinkDebian { get; private init; }

        /// <summary>
        /// Download link for windows installer.
        /// </summary>
        public string DownloadLinkWindows { get; private init; }

        /// <summary>
        /// Download link for macOS installer.
        /// </summary>
        public string DownloadLinkMacOS { get; private init; }

        /// <summary>
        /// URL of release info (the github issue addressed by the release).
        /// </summary>
        public string InfoUrl { get; set; }

        /// <summary>
        /// Version number.
        /// </summary>
        public string Version { get; private init; }

        /// <summary>
        /// Revision number (this is included in the version number).
        /// </summary>
        /// <value></value>
        public uint Revision { get; private init; }
    }
}
