// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RestApiHelper.cs" company="PayRun.IO">
//   PayRun.IO 2017
// </copyright>
// <summary>
//   Defines the RestApiHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PayRunIO.CSharp.SDK
{
    using System;
    using System.IO;
    using System.Net;
    using System.Xml;

    using PayRunIO.DataAccess.Helpers;
    using PayRunIO.Models;
    using PayRunIO.OAuth1;
    using PayRunIO.OAuth1.Helpers;

    /// <summary>
    /// The host API helper.
    /// </summary>
    public class RestApiHelper
    {
        /// <summary>
        /// The signature generator.
        /// </summary>
        private readonly IOAuthSignatureGenerator signatureGenerator;

        /// <summary>
        /// The consumer key.
        /// </summary>
        private readonly string consumerKey;

        /// <summary>
        /// The consumer secret.
        /// </summary>
        private readonly string consumerSecret;

        /// <summary>
        /// The host endpoint
        /// </summary>
        private readonly string hostEndpoint;

        /// <summary>
        /// The content type.
        /// </summary>
        private readonly string contentTypeHeader;

        /// <summary>
        /// Gets the accept header.
        /// </summary>
        private readonly string acceptHeader;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestApiHelper" /> class.
        /// </summary>
        /// <param name="signatureGenerator">The signature generator.</param>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="hostEndpoint">The host endpoint.</param>
        /// <param name="contentTypeHeader">The content-type header to be used (either <c>application/json</c> or <c>application/xml</c>).</param>
        /// <param name="acceptHeader">The accept header to be used (either <c>application/json</c> or <c>application/xml</c>).</param>
        public RestApiHelper(IOAuthSignatureGenerator signatureGenerator, string consumerKey, string consumerSecret, string hostEndpoint, string contentTypeHeader, string acceptHeader)
        {      
            this.signatureGenerator = signatureGenerator;
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.hostEndpoint = hostEndpoint;
            this.contentTypeHeader = contentTypeHeader;
            this.acceptHeader = acceptHeader;
        }

        /// <summary>
        /// Gets or sets the http status code.
        /// </summary>
        public HttpStatusCode HttpStatusCode { get; set; }

        /// <summary>
        /// [PATCH] the specified URL with the given stream.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <param name="patchStream">The patch stream.</param>
        /// <typeparam name="TDto">The DTO data type.</typeparam>
        /// <returns>
        /// The <see cref="TDto"/>.
        /// </returns>
        public TDto Patch<TDto>(string urlPath, Stream patchStream)
            where TDto : DtoBase
        {
            var request = this.CreateAuthorisedRequest(urlPath, "PATCH", patchStream);
            
            using (var response = this.GetResponse(request))
            {
                var xmlDoc = XmlSerialiserHelper.ContentStreamToXmlDocument(response.GetResponseStream(), response.ContentType);

                var output = XmlSerialiserHelper.Deserialise<TDto>(xmlDoc.InnerXml);

                return output;
            }
        }

        /// <summary>
        /// [POST] the specified DTO to the url.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <param name="dtoToPost">The DTO to post.</param>
        /// <returns>
        /// The <see cref="Link"/>.
        /// </returns>
        public Link Post(string urlPath, DtoBase dtoToPost)
        {
            var request = this.CreateAuthorisedRequest(urlPath, "POST", dtoToPost);

            using (var response = this.GetResponse(request))
            {
                var xmlDoc = XmlSerialiserHelper.ContentStreamToXmlDocument(response.GetResponseStream(), response.ContentType);

                var output = XmlSerialiserHelper.Deserialise<Link>(xmlDoc.InnerXml);

                return output;
            }
        }

        /// <summary>
        /// [POST] the specified DTO array to the url.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <param name="dtoToPost">The DTO to post.</param>
        /// <typeparam name="TDto">The DTO data type.</typeparam>
        /// <returns>
        /// The <see cref="LinkCollection"/>.
        /// </returns>
        public LinkCollection Post<TDto>(string urlPath, TDto[] dtoToPost)
        {
            var request = this.CreateAuthorisedRequest(urlPath, "POST", dtoToPost);

            using (var response = this.GetResponse(request))
            {
                var xmlDoc = XmlSerialiserHelper.ContentStreamToXmlDocument(response.GetResponseStream(), response.ContentType);

                var output = XmlSerialiserHelper.Deserialise<LinkCollection>(xmlDoc.InnerXml);

                return output;
            }
        }

        /// <summary>
        /// [PUT] the specified DTO into the url.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <param name="dtoToPut">The DTO to put.</param>
        /// <typeparam name="TDto">THe DTO data type.</typeparam>
        /// <returns>
        /// The <see cref="TDto"/>.
        /// </returns>
        public TDto Put<TDto>(string urlPath, TDto dtoToPut)
            where TDto : DtoBase
        {
            var request = this.CreateAuthorisedRequest(urlPath, "PUT", dtoToPut);

            using (var response = this.GetResponse(request))
            {
                var xmlDoc = XmlSerialiserHelper.ContentStreamToXmlDocument(response.GetResponseStream(), response.ContentType);

                var output = XmlSerialiserHelper.Deserialise<TDto>(xmlDoc.InnerXml);

                return output;
            }
        }

        /// <summary>
        /// [DELETE] the DTO indicated by the specified url.
        /// </summary>
        /// <param name="urlPath">
        /// The url path.
        /// </param>
        public void Delete(string urlPath)
        {
            var request = this.CreateAuthorisedRequest(urlPath, "DELETE");

            using (var response = this.GetResponse(request))
            {
                response.Close();
            }
        }

        /// <summary>
        /// [GET] DTO from the specified url.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <typeparam name="TDto">The DTO data type.</typeparam>
        /// <returns>
        /// The <see cref="TDto"/>.
        /// </returns>
        public TDto Get<TDto>(string urlPath)
            where TDto : DtoBase
        {
            var request = this.CreateAuthorisedRequest(urlPath, "GET");

            using (var response = this.GetResponse(request))
            {
                var xmlDoc = XmlSerialiserHelper.ContentStreamToXmlDocument(response.GetResponseStream(), response.ContentType);

                var output = XmlSerialiserHelper.Deserialise<TDto>(xmlDoc.InnerXml);

                return output;
            }
        }

        /// <summary>
        /// [GET] DTO revision for specified date from the url.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <param name="effectiveDate">The revision effective date.</param>
        /// <typeparam name="TDto">The DTO data type.</typeparam>
        /// <returns>
        /// The <see cref="TDto"/>.
        /// </returns>
        public TDto GetRevision<TDto>(string urlPath, DateTime effectiveDate)
            where TDto : DtoBase
        {
            if (!urlPath.EndsWith("/"))
            {
                urlPath += "/";
            }

            urlPath += effectiveDate.ToString("yyyy-MM-dd");

            return this.Get<TDto>(urlPath);
        }

        /// <summary>
        /// [GET] the links collection from the specified url.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <returns>
        /// The <see cref="LinkCollection"/>.
        /// </returns>
        public LinkCollection GetLinks(string urlPath)
        {
            return this.Get<LinkCollection>(urlPath);
        }

        /// <summary>
        /// [GET] raw xml from specified url path.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <returns>
        /// The <see cref="XmlDocument"/>.
        /// </returns>
        public XmlDocument GetRawXml(string urlPath)
        {
            var request = this.CreateAuthorisedRequest(urlPath, "GET");
            request.Accept = "application/xml";

            using (var response = this.GetResponse(request))
            {
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    throw new ArgumentException($"No response stream received: {urlPath}", nameof(urlPath));
                }

                var xmlDoc = new XmlDocument();
                xmlDoc.Load(responseStream);

                return xmlDoc;
            }
        }

        /// <summary>
        /// [GET] raw JSON from specified url path.
        /// </summary>
        /// <param name="urlPath">The url path.</param>
        /// <returns>
        /// The JSON string />.
        /// </returns>
        public string GetRawJson(string urlPath)
        {
            var request = this.CreateAuthorisedRequest(urlPath, "GET");
            request.Accept = "application/json";

            using (var response = this.GetResponse(request))
            {
                var responseStream = response.GetResponseStream();

                if (responseStream == null)
                {
                    throw new ArgumentException($"No response stream received: {urlPath}", nameof(urlPath));
                }

                var streamReader = new StreamReader(responseStream);
                var jsonResponse = streamReader.ReadToEnd();

                return jsonResponse;
            }
        }

        /// <summary>
        /// Get the response and manage any web exceptions.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// The <see cref="HttpWebResponse"/>.
        /// </returns>
        public HttpWebResponse GetResponse(WebRequest request)
        {
            try
            {
                var response = WebResponseRetryHelper.GetResponse(request, 5);

                this.HttpStatusCode = response.StatusCode;

                return response;
            }
            catch (WebException webException)
            {
                var exceptionResponse = webException.Response as HttpWebResponse;

                if (exceptionResponse == null)
                {
                    throw;
                }

                if (exceptionResponse.StatusCode != HttpStatusCode.InternalServerError && exceptionResponse.StatusCode != HttpStatusCode.BadRequest)
                {
                    throw;
                }

                var responseStream = exceptionResponse.GetResponseStream();

                if (responseStream == null)
                {
                    throw;
                }

                var streamReader = new StreamReader(responseStream);

                var htmlErrorMessage = streamReader.ReadToEnd();

                responseStream.Seek(0, SeekOrigin.Begin);

                var errorMessageStart = htmlErrorMessage.IndexOf("<div id=\"errorDetails\">", StringComparison.Ordinal);

                if (errorMessageStart > -1)
                {
                    htmlErrorMessage = htmlErrorMessage.Substring(errorMessageStart);
                }

                throw new Exception(htmlErrorMessage, webException);
            }
        }

        /// <summary>
        /// Creates an authorised http web request.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="method">The method.</param>
        /// <returns>
        /// The <see cref="HttpWebRequest"/>.
        /// </returns>
        private HttpWebRequest CreateAuthorisedRequest(string path, string method)
        {
            var nonce = Nonce.New();
            var timeStamp = TimeStampHelper.ConvertToTimeStamp(DateTime.Now);

            var request = (HttpWebRequest)WebRequest.Create(this.hostEndpoint + path);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Method = method;

            var signature = this.signatureGenerator.GenerateSignature(
                this.consumerKey,
                this.consumerSecret,
                timeStamp,
                nonce,
                request.RequestUri.AbsoluteUri,
                request.Method);

            var authHeader = this.signatureGenerator.BuildAuthHeader(
                this.consumerKey,
                timeStamp,
                nonce,
                signature);

            request.Headers.Add("Authorization", authHeader);
            request.Accept = this.acceptHeader;
            request.ContentType = this.contentTypeHeader;

            request.Timeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

            return request;
        }

        /// <summary>
        /// Creates an authorised request including the specified payload object.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="method">The method.</param>
        /// <param name="payloadToAdd">The payload to add.</param>
        /// <typeparam name="TDto">The DTO data type.</typeparam>
        /// <returns>
        /// The <see cref="HttpWebRequest"/>.
        /// </returns>
        private HttpWebRequest CreateAuthorisedRequest<TDto>(string path, string method, TDto payloadToAdd)
        {
            var request = this.CreateAuthorisedRequest(path, method);

            Func<TDto, Stream> serialiser;
            if (this.contentTypeHeader.Equals("application/json"))
            {
                serialiser = dto => JsonSerialiserHelper.Serialise(dto);
            }
            else
            {
                serialiser = dto => XmlSerialiserHelper.Serialise(dto);
            }

            using (var payloadStream = serialiser(payloadToAdd))
            {
                request.ContentLength = payloadStream.Length;

                payloadStream.CopyTo(request.GetRequestStream());

                payloadStream.Flush();
            }
            
            return request;
        }

        /// <summary>
        /// Creates an authorised request for the given stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="method">The method.</param>
        /// <param name="payloadToAdd">The payload to add.</param>
        /// <returns>
        /// The <see cref="HttpWebRequest"/>.
        /// </returns>
        private HttpWebRequest CreateAuthorisedRequest(string path, string method, Stream payloadToAdd)
        {
            var request = this.CreateAuthorisedRequest(path, method);

            request.ContentLength = payloadToAdd.Length;
            payloadToAdd.CopyTo(request.GetRequestStream());

            return request;
        }
    }
}
