namespace APSIM.Registration.Models
{
    /// <summary>
    /// Represents a member organisation of the AI.
    /// </summary>
    public class Organisation
    {
        /// <summary>
        /// Name of the organisation.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Domain name of the organisation (as used in member email addresses).
        /// </summary>
        /// <value></value>
        public string DomainName { get; private set; }

        /// <summary>
        /// Create a new <see cref="Organisation"/> instance.
        /// </summary>
        /// <param name="name">Name of the organisation.</param>
        /// <param name="domain">Domain name of the organisation (as used in member email addresses).</param>
        public Organisation(string name, string domain)
        {
            Name = name;
            DomainName = domain;
        }
    }
}
