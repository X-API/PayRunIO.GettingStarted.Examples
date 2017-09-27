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
    using System.Globalization;
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

            var millisecondsTimeout = 500;

            decimal waitTime = 0;

            const int MaxSleepTime = 10000;

            while (true)
            {
                try
                {
                    return (HttpWebResponse)request.GetResponse();
                }
                catch (WebException webEx)
                {
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        throw;
                    }

                    if (++retryCount >= maxReties)
                    {
                        throw new WebException($"Get web response from end point '{request.RequestUri.AbsolutePath}' failed. The target was inaccessible after {retryCount} attempt(s) and {Math.Round(waitTime / 1000, 3)} seconds.", webEx, webEx.Status, webEx.Response);
                    }

                    Thread.Sleep(millisecondsTimeout);

                    waitTime += millisecondsTimeout;

                    request = ((HttpWebRequest)request).Copy();

                    millisecondsTimeout = Math.Min(millisecondsTimeout * 2, MaxSleepTime);
                }
            }
        }

        /// <summary>
        /// Copy the request.
        /// </summary>
        /// <param name="requestToCopy">The request to copy.</param>
        /// <returns>
        /// The <see cref="HttpWebRequest"/>.
        /// </returns>
        private static HttpWebRequest Copy(this HttpWebRequest requestToCopy)
        {
            var clone = (HttpWebRequest)WebRequest.Create(requestToCopy.RequestUri);

            clone.AuthenticationLevel = requestToCopy.AuthenticationLevel;
            clone.CachePolicy = requestToCopy.CachePolicy;
            clone.ConnectionGroupName = requestToCopy.ConnectionGroupName;
            clone.ContentType = requestToCopy.ContentType;
            clone.Credentials = requestToCopy.Credentials;
            clone.ImpersonationLevel = requestToCopy.ImpersonationLevel;
            clone.Method = requestToCopy.Method;
            clone.PreAuthenticate = requestToCopy.PreAuthenticate;
            clone.Proxy = requestToCopy.Proxy;
            clone.Timeout = requestToCopy.Timeout;
            clone.UseDefaultCredentials = requestToCopy.UseDefaultCredentials;

            if (requestToCopy.ContentLength > 0)
            {
                clone.ContentLength = requestToCopy.ContentLength;
            }

            clone.Accept = requestToCopy.Accept;
            clone.AllowAutoRedirect = requestToCopy.AllowAutoRedirect;
            clone.AllowWriteStreamBuffering = requestToCopy.AllowWriteStreamBuffering;
            clone.AutomaticDecompression = requestToCopy.AutomaticDecompression;
            clone.ClientCertificates = requestToCopy.ClientCertificates;
            clone.SendChunked = requestToCopy.SendChunked;
            clone.TransferEncoding = requestToCopy.TransferEncoding;
            clone.Connection = requestToCopy.Connection;
            clone.ContentType = requestToCopy.ContentType;
            clone.ContinueDelegate = requestToCopy.ContinueDelegate;
            clone.CookieContainer = requestToCopy.CookieContainer;
            clone.Date = requestToCopy.Date;
            clone.Expect = requestToCopy.Expect;
            clone.Host = requestToCopy.Host;
            clone.IfModifiedSince = requestToCopy.IfModifiedSince;
            clone.KeepAlive = requestToCopy.KeepAlive;
            clone.MaximumAutomaticRedirections = requestToCopy.MaximumAutomaticRedirections;
            clone.MaximumResponseHeadersLength = requestToCopy.MaximumResponseHeadersLength;
            clone.MediaType = requestToCopy.MediaType;
            clone.Pipelined = requestToCopy.Pipelined;
            clone.ProtocolVersion = requestToCopy.ProtocolVersion;
            clone.ReadWriteTimeout = requestToCopy.ReadWriteTimeout;
            clone.Referer = requestToCopy.Referer;
            clone.Timeout = requestToCopy.Timeout;
            clone.UnsafeAuthenticatedConnectionSharing = requestToCopy.UnsafeAuthenticatedConnectionSharing;
            clone.UserAgent = requestToCopy.UserAgent;

            var allKeys = requestToCopy.Headers.AllKeys;
            foreach (var key in allKeys)
            {
                switch (key.ToLower(CultureInfo.InvariantCulture))
                {
                    case "accept":
                    case "connection":
                    case "content-length":
                    case "content-type":
                    case "date":
                    case "expect":
                    case "host":
                    case "if-modified-since":
                    case "range":
                    case "referer":
                    case "transfer-encoding":
                    case "user-agent":
                    case "proxy-connection":
                        break;
                    default:
                        clone.Headers[key] = requestToCopy.Headers[key];
                        break;
                }
            }

            return clone;
        }
    }
}