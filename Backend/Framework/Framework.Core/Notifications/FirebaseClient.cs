// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FirebaseClient.cs" company="Usama Nada">
//   No Copyright .. Copy, Share, and Evolve.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Framework.Core.Notifications
{
    #region usings

    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    #endregion usings

    /// <summary>
    ///     The firebase client.
    /// </summary>
    public class FirebaseClient
    {
        /// <summary>
        ///     The sender id.
        /// </summary>
        public readonly string SenderId;

        /// <summary>
        ///     The server key.
        /// </summary>
        public readonly string ServerKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseClient"/> class.
        /// </summary>
        /// <param name="fireBaseServerUrl">
        /// The fire base server url.
        /// </param>
        /// <param name="serverKey">
        /// The server key.
        /// </param>
        /// <param name="senderId">
        /// The sender id.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public FirebaseClient(string fireBaseServerUrl, string serverKey, string senderId)
        {
            if (string.IsNullOrEmpty(fireBaseServerUrl))
            {
                throw new ArgumentNullException(nameof(fireBaseServerUrl));
            }

            if (string.IsNullOrEmpty(serverKey))
            {
                throw new ArgumentNullException(nameof(serverKey));
            }

            if (string.IsNullOrEmpty(senderId))
            {
                throw new ArgumentNullException(nameof(senderId));
            }

            this.FirebaseServerUrl = fireBaseServerUrl;
            this.ServerKey = serverKey;
            this.SenderId = senderId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseClient"/> class.
        /// </summary>
        /// <param name="serverKey">
        /// The server key.
        /// </param>
        /// <param name="senderId">
        /// The sender id.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public FirebaseClient(string serverKey, string senderId)
        {
            if (string.IsNullOrEmpty(serverKey))
            {
                throw new ArgumentNullException(nameof(serverKey));
            }

            if (string.IsNullOrEmpty(senderId))
            {
                throw new ArgumentNullException(nameof(senderId));
            }

            this.ServerKey = serverKey;
            this.SenderId = senderId;
        }

        /// <summary>
        ///     Gets the firebase server url.
        /// </summary>
        public string FirebaseServerUrl { get; } = "https://fcm.googleapis.com/fcm/send";

        /// <summary>
        /// The send notification.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// The <see cref="ReturnResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public ReturnResult SendNotification(MobileNotification message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var result = new ReturnResult();

            var webRequest = WebRequest.Create(this.FirebaseServerUrl);
            webRequest.Method = "post";
            webRequest.ContentType = "application/json";
            webRequest.Headers.Add($"Authorization: key={this.ServerKey}");
            webRequest.Headers.Add($"Sender: id={this.SenderId}");

            var payload = new
            {
                to = message.ToMobileToken,
                priority = message.Priority ?? "high",
                content_available = true,
                notification =
                                      new { body = message.Body, title = message.Title, badge = message.Badge },
                data = message.Data
            };

            var requestJson = JsonConvert.SerializeObject(payload);
            var byteArray = Encoding.UTF8.GetBytes(requestJson);
            webRequest.ContentLength = byteArray.Length;

            using (var dataStream = webRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);

                using (var webResponse = webRequest.GetResponse())
                using (var dataStreamResponse = webResponse.GetResponseStream())
                using (var reader = new StreamReader(dataStreamResponse ?? throw new InvalidOperationException()))
                {
                    var responseJson = reader.ReadToEnd();

                    var response = JsonConvert.DeserializeObject<FirebaseResponse>(responseJson);

                    if (response == null)
                    {
                        result.AddErrorItem(
                            string.Empty,
                            $"Firebase Returned Null Response Request JSON: {requestJson} ResponseJson: {responseJson}");
                        return result;
                    }

                    if (response.failure == 1)
                    {
                        result.AddErrorItem(
                            string.Empty,
                            $"Firebase Returned Failure Response Request JSON: {requestJson} ResponseJson: {responseJson}");
                        return result;
                    }

                    if (response.success == 1)
                    {
                        return result;
                    }
                }
            }

            return result;
        }
    }
}