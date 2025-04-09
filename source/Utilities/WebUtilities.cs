using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace APSIM.Registration.Utilities
{
    public static class WebUtilities
    {
        /// <summary>
        /// Default deserialization options for reading responses.
        /// This is used to perform a case-insensitive deserialization.
        /// </summary>
        private static readonly JsonSerializerOptions deserializationOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
        };

        /// <summary>
        /// Make a POST request to a REST web API.
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="endpoint">The request URI.</param>
        /// <param name="payload">Request payload</param>
        public static async Task<T> PostAsync<T>(string endpoint, object payload = null)
        {
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.PostAsJsonAsync(endpoint, payload))
                {
                    response.EnsureSuccessStatusCode();
                    // todo - change to a stream approach when we move to .net 6
                    return JsonSerializer.Deserialize<T>(await response.Content.ReadAsByteArrayAsync(), deserializationOptions);
                }
            }
        }

        /// <summary>Helper class to ignore namespaces when de-serializing</summary>
        private class NamespaceIgnorantXmlTextReader : XmlTextReader
        {
            /// <summary>Constructor</summary>
            /// <param name="reader">The text reader.</param>
            public NamespaceIgnorantXmlTextReader(TextReader reader) : base(reader) { }

            /// <summary>Override the namespace.</summary>
            public override string NamespaceURI
            {
                get { return ""; }
            }
        }
    }
}