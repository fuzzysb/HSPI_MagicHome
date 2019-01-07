using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
// ReSharper disable IdentifierTypo

namespace HSPI_MagicHome
{
    /// <summary>
    /// </summary>
    internal class SendPluginData
    {
        /// <summary>
        /// </summary>
        /// <param name="pluginentry"></param>
        /// <returns></returns>
        internal static bool SendHS3Data(Classes.HS3PluginDataEntry pluginentry)
        {
            try
            {
                const string url = @"https://www.broadbandtap.co.uk/hs3api/HS3PluginData.svc/data";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json; charset=utf-8";
                httpWebRequest.Method = "POST";
                httpWebRequest.Accept = "application/json; charset=utf-8";
                string returnstring;
                var json = JsonConvert.SerializeObject(pluginentry);
                byte[] postBytes = Encoding.UTF8.GetBytes(json);
                httpWebRequest.ContentLength = postBytes.Length;
                var requestStream = httpWebRequest.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader =
                    new StreamReader(httpResponse.GetResponseStream() ?? throw new InvalidOperationException()))
                {
                    returnstring = streamReader.ReadToEnd();
                }
                var result = JsonConvert.DeserializeObject<Classes.CADataSendResult>(returnstring, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, StringEscapeHandling = StringEscapeHandling.EscapeNonAscii });
                return result.Result == "Ok";
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}