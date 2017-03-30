// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebResponseRetryHelper.cs" company="PayRun.IO">
//   PayRun.IO 2017
// </copyright>
// <summary>
//   Defines the WebResponseRetryHelper type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace PayRunIO.CSharp.SDK
{
    using System.IO;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// The web response retry helper.
    /// </summary>
    public static class WebResponseRetryHelper
    {
        /// <summary>
        /// Gets the web response. Includes retry logic if target machine refuses connection.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="maxReties">The maximum number of retries.</param>
        /// <returns>
        /// The <see cref="HttpWebResponse"/>.
        /// </returns>
        public static HttpWebResponse GetResponse(WebRequest request, int maxReties)
        {
            var retryCount = 0;

            while (true)
            {
                try
                {
                    return (HttpWebResponse)request.GetResponse();
                }
                catch (WebException webEx)
                {
                    var exceptionResponse = webEx.Response as HttpWebResponse;

                    if (!RequestWasActivelyRefused(exceptionResponse) || ++retryCount >= maxReties)
                    {
                        throw;
                    }

                    Thread.Sleep(100);
                }
            }
        }

        /// <summary>
        /// Determines if the request was actively refused by the remote server.
        /// </summary>
        /// <param name="webResponse">The web response.</param>
        /// <returns>
        /// <c>True</c> if the request was actively refused; otherwise <c>false</c>.
        /// </returns>
        private static bool RequestWasActivelyRefused(WebResponse webResponse)
        {
            var responseStream = webResponse?.GetResponseStream();

            if (responseStream == null)
            {
                return false;
            }

            var streamReader = new StreamReader(responseStream);
            var responseMessage = streamReader.ReadToEnd();
            responseStream.Seek(0, SeekOrigin.Begin);

            return responseMessage.Contains("No connection could be made because the target machine actively refused it");
        }
    }
}