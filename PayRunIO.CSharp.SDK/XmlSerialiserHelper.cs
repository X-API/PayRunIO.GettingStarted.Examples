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
    using System.Xml;
    using System.Xml.Serialization;

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
        /// Deserialises the stream.
        /// </summary>
        /// <typeparam name="TDto">The type of the DTO.</typeparam>
        /// <param name="sourceStream">The source stream.</param>
        /// <returns>
        /// Instance of <see cref="TDto"/>.
        /// </returns>
        public static TDto DeserialiseStream<TDto>(Stream sourceStream)
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
        public static object DeserialiseStream(Stream sourceStream, Type dtoType)
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
    }
}