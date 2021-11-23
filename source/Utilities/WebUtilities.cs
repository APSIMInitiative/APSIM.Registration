using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace APSIM.Registration.Utilities
{
    public static class WebUtilities
    {
        /// <summary>Call REST web service.</summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="url">The URL of the REST service.</param>
        /// <returns>The return data</returns>
        public static T CallRESTService<T>(string url)
        {
            WebRequest wrGETURL;
            wrGETURL = WebRequest.Create(url);
            wrGETURL.Method = "GET";
            wrGETURL.ContentType = @"application/xml; charset=utf-8";
            wrGETURL.ContentLength = 0;
            using (HttpWebResponse webresponse = wrGETURL.GetResponse() as HttpWebResponse)
            {
                Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
                // read response stream from response object
                using (StreamReader loResponseStream = new StreamReader(webresponse.GetResponseStream(), enc))
                {
                    string st = loResponseStream.ReadToEnd();
                    if (typeof(T).Name == "Object")
                        return default(T);

                    XmlSerializer serializer = new XmlSerializer(typeof(T));

                    //ResponseData responseData;
                    return (T)serializer.Deserialize(new NamespaceIgnorantXmlTextReader(new StringReader(st)));
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