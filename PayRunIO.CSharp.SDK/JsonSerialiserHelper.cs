// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonSerialiserHelper.cs" company="PayRun.IO">
//   PayRun.IO 2017
// </copyright>
// <summary>
//   The JSON serialiser helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PayRunIO.CSharp.SDK
{
    using System.IO;
    using System.Text;
    using System.Xml;

    using Newtonsoft.Json;

    using PayRunIO.DataAccess.Helpers;

    using Formatting = Newtonsoft.Json.Formatting;

    /// <summary>
    /// The JSON serialiser helper.
    /// </summary>
    public static class JsonSerialiserHelper
    {
        /// <summary>
        /// Serialise single DTO into a JSON stream.
        /// </summary>
        /// <param name="objectToSerialise">The object to serialise.</param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream Serialise(object objectToSerialise)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(XmlSerialiserHelper.Serialise(objectToSerialise));

            return Serialise(xmlDoc);
        }

        /// <summary>
        /// Serialise the xml document content as JSON.
        /// </summary>
        /// <param name="documentToSerialise">The document to serialise.</param>
        /// <returns>
        /// The <see cref="Stream"/>.
        /// </returns>
        public static Stream Serialise(XmlDocument documentToSerialise)
        {
            var json = JsonConvert.SerializeXmlNode(documentToSerialise.DocumentElement, Formatting.Indented);

            json = json.Replace("\"@xmlns:xsd\": \"http://www.w3.org/2001/XMLSchema\",\r\n", string.Empty);

            var hasXsiItems = json.Contains("\"@xsi:");

            if (!hasXsiItems)
            {
                json = json.Replace("\"@xmlns:xsi\": \"http://www.w3.org/2001/XMLSchema-instance\",\r\n", string.Empty);
            }

            var outputStream = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return outputStream;
        }
    }
}