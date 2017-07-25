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
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
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
                    var innerException = webEx.InnerException as SocketException;

                    if (innerException == null || !innerException.Message.Contains("No connection could be made because the target machine actively refused it"))
                    {
                        throw;
                    }

                    if (++retryCount >= maxReties)
                    {
                        throw new WebException($"Get web response from end point '{request.RequestUri.AbsolutePath}' failed. The target machine actively refused the connection {retryCount} time(s)", webEx, webEx.Status, webEx.Response);
                    }

                    Thread.Sleep(100);
                }
            }
        }
    }
}