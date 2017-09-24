// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlSerialiserHelper.cs" company="PayRun.IO">
//   PayRun.IO 2017
// </copyright>
// <summary>
//   The xml serialiser helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PayRunIO.CSharp.SDK
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Serialization;

    using Newtonsoft.Json;

    using PayRunIO.Models;

    /// <summary>
    /// The xml serialiser helper.
    /// </summary>
    public static class XmlSerialiserHelper
    {
        /// <summary>
        /// The xml serialiser dictionary.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, XmlSerializer> XmlSerialisers = new ConcurrentDictionary<Type, XmlSerializer>();

        /// <summary>
        /// Serialises the specified DTO.
        /// </summary>
        /// <param name="dto">The DTO.</param>
        /// <param name="namespaces">The namespaces.</param>
        /// <returns>The serialised stream.</returns>
        public static Stream Serialise(object dto, XmlSerializerNamespaces namespaces = null)
        {
            var dtoType = dto.GetType();

            var serialiser = GetXmlSerializer(dtoType);

            var memoryStream = new MemoryStream();

            if (namespaces == null)
            {
                serialiser.Serialize(memoryStream, dto);
            }
            else
            {
                serialiser.Serialize(memoryStream, dto, namespaces);
            }
            
            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream;
        }

        /// <summary>
        /// Serialises to XML document.
        /// </summary>
        /// <param name="dto">The DTO.</param>
        /// <param name="namespaces">The namespaces.</param>
        /// <returns>the XmlDocument.</returns>
        public static XmlDocument SerialiseToXmlDoc(object dto, XmlSerializerNamespaces namespaces = null)
        {
            var xmlStream = Serialise(dto, namespaces);
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlStream);

            return xmlDoc;
        }

        /// <summary>
        /// Deserialises the specified source string.
        /// </summary>
        /// <typeparam name="T">The type</typeparam>
        /// <param name="sourceXml">The source string XML</param>
        /// <returns>The deserialised object</returns>
        public static T Deserialise<T>(string sourceXml)
        {
            var objType = typeof(T);

            var serialiser = GetXmlSerializer(objType);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sourceXml));

            var result = (T)serialiser.Deserialize(stream);

            return result;
        }

        /// <summary>
        /// Deserialises the stream.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO.</typeparam>
        /// <param name="sourceStream">The source stream.</param>
        /// <returns>
        /// Instance of <see cref="TDto"/>.
        /// </returns>
        public static TDto DeserialiseDtoStream<TDto>(Stream sourceStream)
            where TDto : DtoBase
        {
            var dtoType = typeof(TDto);

            var serialiser = GetXmlSerializer(dtoType);

            var dto = (TDto)serialiser.Deserialize(sourceStream);

            return dto;
        }

        /// <summary>
        /// Deserialise stream into array of DTOs.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <typeparam name="TDto">The DTO data type.</typeparam>
        /// <returns>
        /// The DTO array.
        /// </returns>
        public static TDto[] DeserialiseArrayStream<TDto>(Stream sourceStream)
        {
            var arrayType = typeof(TDto[]);

            var serialiser = GetXmlSerializer(arrayType);

            var array = (TDto[])serialiser.Deserialize(sourceStream);

            return array;
        }

        /// <summary>
        /// De-serialise the specified stream.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="dtoType">The DTO type.</param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public static object DeserialiseDtoStream(Stream sourceStream, Type dtoType)
        {
            var serialiser = GetXmlSerializer(dtoType);

            var dto = serialiser.Deserialize(sourceStream);

            return dto;
        }

        /// <summary>
        /// De-serialise the specified stream to an array of DTOs.
        /// </summary>
        /// <param name="sourceStream">The source stream.</param>
        /// <param name="dtoType">The DTO type.</param>
        /// <returns>
        /// The <see cref="object"/> array.
        /// </returns>
        public static object[] DeserialiseArrayStream(Stream sourceStream, Type dtoType)
        {
            var arrayType = dtoType.MakeArrayType();

            var serialiser = GetXmlSerializer(arrayType);

            var array = serialiser.Deserialize(sourceStream);

            return (object[])array;
        }

        /// <summary>
        /// Creates an xml document from the specified content stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="contentType">The content type.</param>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        public static XmlDocument ContentStreamToXmlDocument(Stream stream, string contentType)
        {
            var isXmlContent = Regex.IsMatch(contentType, @"application\/xml|text\/xml|application\/vnd\.[^;]+\+xml");
            var isJsonContent = Regex.IsMatch(contentType, @"application\/json|text\/json|application\/vnd\.[^;]+\+json");

            if (!isXmlContent && !isJsonContent)
            {
                throw new NotSupportedException($"The specified content type '{contentType}' is not currently supported.");
            }

            var xmlDoc = isXmlContent ? LoadXmlStreamContent(stream) : LoadJsonStreamContent(stream);

            return xmlDoc;
        }

        /// <summary>
        /// Gets the XML serialiser for the specified DTO type.
        /// </summary>
        /// <param name="dtoType">The DTO type.</param>
        /// <returns>
        /// The <see cref="XmlSerializer"/>.
        /// </returns>
        private static XmlSerializer GetXmlSerializer(Type dtoType)
        {
            XmlSerializer serialiser;

            if (!XmlSerialisers.TryGetValue(dtoType, out serialiser))
            {
                serialiser = new XmlSerializer(dtoType);
                XmlSerialisers.TryAdd(dtoType, serialiser);
            }

            return serialiser;
        }

        /// <summary>
        /// Loads the XML stream content into an XML document object.
        /// </summary>
        /// <param name="xmlDataStream">The xml data stream.</param>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        private static XmlDocument LoadXmlStreamContent(Stream xmlDataStream)
        {
            var xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.Load(xmlDataStream);

                return xmlDoc;
            }
            catch (XmlException xmlEx)
            {
                throw new InvalidDataException("XML data stream is not valid. " + xmlEx.Message, xmlEx);
            }
        }

        /// <summary>
        /// Loads the JSON stream content into an XML document object.
        /// </summary>
        /// <param name="jsonDataStream">The JSON data stream.</param>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        private static XmlDocument LoadJsonStreamContent(Stream jsonDataStream)
        {
            // Note leave stream open for use outside this scope.
            var json = new StreamReader(jsonDataStream).ReadToEnd();

            if (json.Contains("\"@xsi:") && !json.Contains("\"@xmlns:xsi\":"))
            {
                var rootMatch = Regex.Match(json, "\"\\w+\": \\{");

                if (rootMatch.Success)
                {
                    json = json.Replace(
                        rootMatch.Value,
                        rootMatch.Value + "\"@xmlns:xsi\": \"http://www.w3.org/2001/XMLSchema-instance\",");
                }
            }

            try
            {
                var xmlDoc = JsonConvert.DeserializeXmlNode(json);

                return xmlDoc;
            }
            catch (JsonReaderException jsonEx)
            {
                throw new InvalidDataException("Json data stream is not valid. " + jsonEx.Message, jsonEx);
            }
        }
    }
}