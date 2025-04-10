﻿using FeedFetcher.API;
using FeedFetcher.Interfaces;
using FeedFetcher.IOCAndServices;
using FeedFetcher.Models;
using FeedFetcher.Response;
using InstagramApiSharp;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using ZstdNet;

namespace FeedFetcher.Utilities
{
    public class HttpHelper
    {
        private static HttpHelper instance;
        private InstaAPI api { get; set; }
        private ILogger logger;
        public static HttpHelper Instance => instance ?? (instance = new HttpHelper());
        private InstagramUser InstagramUser { get; set; } = new InstagramUser();
        public HttpHelper()
        {
            logger = InstanceProvider.GetInstance<ILogger>();
        }
        private static JsonJArrayHandler handler => JsonJArrayHandler.GetInstance;
        public string SessionString { get; set; }
        public void SetSession(SessionModel session)
        {
            this.SessionString = session.CookieString;
            InstagramUser.JsonCookies = session.CookieString;
            InstagramUser.Username = session.Username;
            InstagramUser.Password = session.Password;
            api = InstaAPI.Instance(new InstagramAccount { Cookies = session.CookieString, Username = InstagramUser.Username, Password = InstagramUser.Password });
        }
        public async Task<bool> IsAuthenticated(SessionModel session)
        {
            try
            {
                api = await api.Build();
                if (api.instaApi.IsUserAuthenticated)
                {
                    var str = await api.instaApi.GetStateDataAsStringAsync();
                    session.CookieString = str;
                }
                return api.instaApi.IsUserAuthenticated;
            }
            catch { return false; }
        }
        public SessionRequestParam WEBLogin(string profileUrl = "")
        {
            var finalResponse = string.Empty;
            var reqParam = GetSessionParam();
            try
            {
                var url = string.IsNullOrEmpty(profileUrl) ? "https://www.instagram.com" : profileUrl;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                SetProxy(ref request);
                request.Method = "GET";
                request.Headers.Add("Upgrade-Insecure-Requests", "1");
                request.UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.6802.75 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7";
                request.Headers.Add("Sec-Fetch-Site", "same-site");
                request.Headers.Add("Sec-Fetch-Mode", "navigate");
                request.Headers.Add("Sec-Fetch-User", "?1");
                request.Headers.Add("Sec-Fetch-Dest", "document");
                request.Headers.Add("dpr", "1");
                request.Headers.Add("viewport-width", "1920");
                request.Headers.Add("sec-ch-ua", "\"(Not(A:Brand\";v=\"99\", \"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\"");
                request.Headers.Add("sec-ch-ua-mobile", "?0");
                request.Headers.Add("sec-ch-ua-platform", "\"Linux\"");
                request.Headers.Add("sec-ch-ua-model", "");
                request.Headers.Add("sec-ch-ua-full-version-list", "\"(Not(A:Brand\";v=\"99.0.0.0\", \"Google Chrome\";v=\"131\", \"Chromium\";v=\"131\"");
                request.Headers.Add("sec-ch-prefers-color-scheme", "light");
                request.Referer = "https://www.instagram.com/";
                request.Headers.Add("Accept-Encoding", "gzip,deflate,br");
                request.Headers.Add("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                request.CookieContainer = reqParam.Cookies;
                // Send the request and get the response
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    // Handle the response here (example: reading response stream)
                    var encodingType = response?.ContentEncoding?.ToString()?.ToLower();
                    finalResponse = GetDecodedResponse(response.GetResponseStream(), encodingType);
                    if (finalResponse.Contains("\"is_logged_out_user\":false"))
                    {
                        reqParam.Response = finalResponse;
                        var data = Regex.Split(finalResponse, "</script>");
                        var json = data.FirstOrDefault(x => x.Contains("\"is_logged_out_user\":false"));
                        json = Regex.Replace(json, "<.*?>", "")?.Replace("\n", "");
                        reqParam.Hmac = GetBetween(json, "\"claim\":\"", "\"");
                        reqParam.OtherProfileID = GetBetween(reqParam.Response, "\"profile_id\":\"", "\"");
                    }
                }
            }
            catch { }
            return reqParam;
        }

        private void SetProxy(ref HttpWebRequest request)
        {
            try
            {
                if (!string.IsNullOrEmpty(InstagramUser.Proxy.Ip)
                    && InstagramUser.Proxy.Port > 0)
                {
                    var webProxy = new WebProxy(InstagramUser.Proxy.Ip, InstagramUser.Proxy.Port)
                    {
                        BypassProxyOnLocal = true
                    };

                    if (!string.IsNullOrEmpty(InstagramUser.Proxy.Username)
                        && !string.IsNullOrEmpty(InstagramUser.Proxy.Password))
                    {
                        webProxy.Credentials = new NetworkCredential(InstagramUser.Proxy.Username, InstagramUser.Proxy.Password);
                    }
                    request.Proxy = webProxy;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private string GetDecodedResponse(Stream responseStream, string EncodingType = "gzip")
        {
            try
            {
                if (responseStream == null)
                    return string.Empty;
                #region Decoding Response
                Stream decompressedStream = responseStream;
                using (responseStream)
                {
                    try
                    {
                        var ContentEncoding = EncodingType;
                        var isGzip = false;
                        var isDeflate = false;
                        var isBr = false;
                        var isZstd = false;
                        var IsEncoded = !string.IsNullOrEmpty(ContentEncoding) &&
                            ((isGzip = ContentEncoding.Contains("gzip"))
                            || (isDeflate = ContentEncoding.Contains("deflate"))
                            || (isBr = ContentEncoding.Contains("br"))
                            || (isZstd = ContentEncoding.Contains("zstd")));
                        if (IsEncoded)
                        {
                            using (Stream stream = isGzip ? new GZipStream(responseStream, CompressionMode.Decompress) :
                                isDeflate ? new DeflateStream(responseStream, CompressionMode.Decompress) :
                                isBr ? new BrotliStream(responseStream, CompressionMode.Decompress) :
                                isZstd ? new DecompressionStream(responseStream) :
                                responseStream)
                            {
                                using (var streamReader = new StreamReader(stream))
                                {
                                    return streamReader.ReadToEnd();
                                }
                            }
                        }
                        else
                        {
                            using (var streamReader = new StreamReader(responseStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        using (var streamReader = new StreamReader(responseStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
                #endregion
            }
            catch { }
            return new StreamReader(responseStream).ReadToEnd();
        }
        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            try
            {
                int Start, End;
                if (strSource.Contains(strStart) && strSource.Contains(strEnd))
                {
                    Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                    End = strSource.IndexOf(strEnd, Start);
                    return strSource.Substring(Start, End - Start);
                }
                else
                {
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }
        public static string GetGuid(bool isDashesNeed = true)
        {
            // Generate the GUID 
            var getGuid = Guid.NewGuid().ToString();
            // return the GUID without dashes if isDashesNeed is true 
            return !isDashesNeed ? getGuid.Replace('-', char.MinValue) : getGuid;
        }
        public static string GetRowClientTime()
        {
            string Rawclienttime = ((long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
            string initialtime = Rawclienttime.Substring(0, Rawclienttime.Length - 3);
            string endtime = Rawclienttime.Substring(Rawclienttime.Length - 3);
            Rawclienttime = initialtime + "." + endtime;
            return Rawclienttime;
        }
        public static string GetHexFromString(string inputString)
        {
            //Validate the input whether is null or not 
            if (inputString == null)
                throw new ArgumentNullException(nameof(inputString));

            // Convert the input values to hexa
            using (var md5 = MD5.Create())
            {
                // Read the bytes values form the input string
                var bytes = Encoding.UTF8.GetBytes(inputString);

                //Compute the hash values of bytes with the help of MD5 then convert those to base datatype(here string),
                //finally convert the string to lower
                return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", string.Empty).ToLower();
            }
        }
        public static string GetMobileDeviceId(string Guid = "")
        {
            // Collect the random inputString with five character, convert those character to byte array with help of the MD5
            return "android-" + (string.IsNullOrEmpty(Guid) ? GetHexFromString(RandomUtilties.GetRandomString(5)).Substring(0, 16) : Guid);
        }
        public InstagramUser MOBILELogin()
        {
            var FinalResponse = string.Empty;
            try
            {
                var reqParam = WEBLogin();
                var url = "https://i.instagram.com/api/v1/feed/timeline/";
                // Set up HttpClient
                var deviceID = GetGuid() ?? "b7c5fb84-c663-4879-a8aa-59d665992112";
                var request = (HttpWebRequest)WebRequest.Create(url);
                SetProxy(ref request);
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                request.Accept = "*/*";
                request.UserAgent = "Instagram 283.0.0.20.105 Android (31/12; 320dpi; 720x1470; vivo; V2029; 2027; qcom; en_US; 475221264)";
                request.Headers.Add("X-Ads-Opt-Out", "0");
                request.Headers.Add("X-Google-AD-ID", "ccb53d12-326c-46c5-b334-260cf6b447df");
                request.Headers.Add("X-DEVICE-ID", deviceID);
                request.Headers.Add("X-Pigeon-Rawclienttime", GetRowClientTime());
                request.Headers.Add("X-WhatsApp", "1");
                request.Headers.Add("X-Pigeon-Session-Id", $"UFS-{GetGuid()}-1");
                request.Headers.Add("IG-U-RUR", reqParam.RUR);
                request.Headers.Add("X-CM-Bandwidth-KBPS", "419.597");
                request.Headers.Add("X-CM-Latency", "163.051");
                request.Headers.Add("X-IG-Bandwidth-Speed-KBPS", "419.000");
                request.Headers.Add("X-IG-Bandwidth-TotalBytes-B", "0");
                request.Headers.Add("X-IG-Bandwidth-TotalTime-MS", "0");
                request.Headers.Add("X-Bloks-Version-Id", "f5fbf62cc3c51dc0e6f4ffd3a79e0c5929ae0b8af58c54acd1e186871a92fb27");
                if (!string.IsNullOrEmpty(reqParam?.Hmac))
                    request.Headers.Add("X-IG-WWW-Claim", reqParam.Hmac);
                request.Headers.Add("X-IG-Transfer-Encoding", "chunked");
                request.Headers.Add("X-Bloks-Is-Layout-RTL", "false");
                request.Headers.Add("X-IG-Device-ID", deviceID);
                request.Headers.Add("X-IG-Family-Device-ID", GetGuid() ?? "3b45623e-d661-4343-9c9a-0dbdc7fbca21");
                request.Headers.Add("X-IG-Android-ID", GetMobileDeviceId() ?? "android-b10069c5ba7bbd58");
                request.Headers.Add("X-IG-Timezone-Offset", "19800");
                request.Headers.Add("X-IG-CLIENT-ENDPOINT", "unknown");
                request.Headers.Add("X-FB-Connection-Type", "WIFI");
                request.Headers.Add("X-IG-Connection-Type", "WIFI");
                request.Headers.Add("X-IG-Capabilities", "3brTv10=");
                request.Headers.Add("X-IG-App-ID", "567067343352427");
                request.Headers.Add("Priority", "u=0");
                request.Headers.Add("Accept-Language", "en-US");
                request.Headers.Add("Accept-Encoding", "gzip,deflate,br");
                request.Headers.Add("Authorization", reqParam.AuthToken);
                request.Headers.Add("X-MID", reqParam.MID);
                request.Headers.Add("IG-U-DS-USER-ID", reqParam.UserID);
                request.Headers.Add("IG-INTENDED-USER-ID", reqParam.UserID);
                // Prepare the form data
                var data = new Dictionary<string, string>
                {
                    { "has_camera_permission", "0" },
                    { "feed_view_info", "[]" },
                    { "phone_id",GetGuid() },
                    { "reason", "cold_start_fetch" },
                    { "battery_level", InstagramUser?.BatteryLevel },
                    { "timezone_offset", "19800" },
                    { "device_id", deviceID },
                    { "request_id",GetGuid() },
                    { "is_pull_to_refresh", "0" },
                    { "_uuid", deviceID },
                    { "is_charging", "0" },
                    { "is_dark_mode", "0" },
                    { "will_sound_on", "0" },
                    { "session_id",GetGuid() },
                    { "bloks_versioning_id", "f5fbf62cc3c51dc0e6f4ffd3a79e0c5929ae0b8af58c54acd1e186871a92fb27" }
                };

                // Encode the data as form-urlencoded content
                var postData = new StringBuilder();
                foreach (var pair in data)
                {
                    if (postData.Length > 0)
                        postData.Append("&");
                    postData.Append(HttpUtility.UrlEncode(pair.Key));
                    postData.Append("=");
                    postData.Append(HttpUtility.UrlEncode(pair.Value));
                }

                // Convert the post data to a byte array
                byte[] byteArray = Encoding.UTF8.GetBytes(postData.ToString());

                // Write data to request stream
                request.ContentLength = byteArray.Length;
                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }

                // Get response
                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            InstagramUser.IsMobileLoggedIn = true;
                            FinalResponse = GetDecodedResponse(response.GetResponseStream(), response?.ContentEncoding?.ToLower());
                        }
                    }
                }
                catch (WebException ex)
                {
                    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        FinalResponse = reader.ReadToEnd();
                    }
                }
                finally
                {
                    InstagramUser.UpdateMIDAndAuthToken(reqParam.MID, reqParam.AuthToken);
                }
            }
            catch { }
            return InstagramUser;
        }
        private SessionRequestParam GetSessionParam()
        {
            var param = new SessionRequestParam();
            var array = handler.GetJArrayElement(SessionString);
            foreach (var cookie in array)
            {
                var name = handler.GetJTokenValue(cookie, "name");
                var value = handler.GetJTokenValue(cookie, "value");
                var domain = handler.GetJTokenValue(cookie, "domain");
                if (name == "mid")
                    param.MID = value;
                if (name == "ds_user_id")
                    param.UserID = value;
                if (name == "sessionid")
                    param.AuthToken = value;
                if (name == "rur")
                {
                    param.RUR = value?.Replace("\\054", ",")?.Replace("\"", "");
                    if (!string.IsNullOrWhiteSpace(param.RUR))
                        param.IGURUR = GetBetween("#" + param.RUR, "#", ",");
                }
                param.Cookies.Add(new Cookie { Name = name, Value = value, Domain = ".instagram.com" });
            }
            if (!string.IsNullOrEmpty(param.AuthToken))
            {
                var json = $"{{\"ds_user_id\":\"{param.UserID}\",\"sessionid\":\"{param.AuthToken}\"}}";
                var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
                param.AuthToken = $"Bearer IGT:2:{token}";
            }
            return param;
        }
        //private string GetProfileUrl(string text)
        //{
        //    try
        //    {
        //        if (!string.IsNullOrEmpty(text) && !text.Contains(".instagram.com"))
        //            return $"https://www.instagram.com/{text}/";
        //        return text;
        //    }
        //    catch { return text; }
        //}
        public async Task<FeedResponseHandler> GetFeedResponse(string profileUrl, bool IsMobile = false)
        {
            var response = new FeedResponseHandler();
            try
            {
                if (!IsMobile)
                {
                    var url = $"https://www.instagram.com/api/v1/users/web_profile_info/?username={WebUtility.UrlEncode(profileUrl)}";

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    request.Method = "GET";
                    request.Headers["Accept-Encoding"] = "gzip, deflate, br, zstd";
                    request.Headers["Accept-Language"] = "en-GB,en;q=0.9";
                    request.Referer = $"https://www.instagram.com/{profileUrl}/";
                    request.Headers["sec-ch-ua"] = "\"Google Chrome\";v=\"135\", \"Not-A.Brand\";v=\"8\", \"Chromium\";v=\"135\"";
                    request.Headers["sec-ch-ua-full-version-list"] = "\"Google Chrome\";v=\"135.0.7049.84\", \"Not-A.Brand\";v=\"8.0.0.0\", \"Chromium\";v=\"135.0.7049.84\"";
                    request.Headers["sec-ch-ua-mobile"] = "?0";
                    request.Headers["sec-ch-ua-platform"] = "\"Windows\"";
                    request.Headers["sec-fetch-dest"] = "empty";
                    request.Headers["sec-fetch-mode"] = "cors";
                    request.Headers["sec-fetch-site"] = "same-origin";
                    request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36";
                    request.Headers["x-ig-app-id"] = "936619743392459";
                    try
                    {
                        var jsons = this.SessionString.Deserialize<List<cookies>>();
                        var collections = new CookieContainer();
                        foreach (cookies cookie in jsons)
                        {
                            collections.Add(new Cookie { Name = cookie.name, Value = cookie.value, Domain = cookie.domain });
                        }
                        request.CookieContainer = collections;
                    }
                    catch (Exception)
                    {
                    }
                    var res = await request.GetResponseAsync() as HttpWebResponse;
                    return new FeedResponseHandler(GetDecodedResponse(res.GetResponseStream(), res?.ContentEncoding));
                    #region Mobile Login
                    //profileUrl = GetProfileUrl(profileUrl);
                    //var reqParam = WEBLogin(profileUrl);
                    //var url = $"https://i.instagram.com/api/v1/feed/user_stream/{reqParam.OtherProfileID}/";
                    //var deviceID = GetGuid();
                    //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                    //request.Method = "POST";
                    //request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                    //// Add headers
                    //request.Headers.Add("User-Agent", "Instagram 283.0.0.20.105 Android (31/12; 320dpi; 720x1470; vivo; V2029; 2027; qcom; en_US; 475221264)");
                    //request.Headers.Add("X-IG-App-Locale", "en_US");
                    //request.Headers.Add("X-IG-Device-Locale", "en_US");
                    //request.Headers.Add("X-IG-Mapped-Locale", "en_US");
                    //request.Headers.Add("X-Pigeon-Session-Id", $"UFS-{GetGuid()}-0");
                    //request.Headers.Add("X-Pigeon-Rawclienttime", GetRowClientTime());
                    ////request.Headers.Add("X-IG-Bandwidth-Speed-KBPS", "787.000");
                    ////request.Headers.Add("X-IG-Bandwidth-TotalBytes-B", "4522550");
                    ////request.Headers.Add("X-IG-Bandwidth-TotalTime-MS", "6464");
                    //request.Headers.Add("X-Bloks-Version-Id", "f5fbf62cc3c51dc0e6f4ffd3a79e0c5929ae0b8af58c54acd1e186871a92fb27");
                    //if (!string.IsNullOrEmpty(reqParam.Hmac))
                    //    request.Headers.Add("X-IG-WWW-Claim", reqParam.Hmac);
                    //request.Headers.Add("X-IG-Transfer-Encoding", "chunked");
                    //request.Headers.Add("X-Bloks-Is-Layout-RTL", "false");
                    //request.Headers.Add("X-IG-Device-ID", deviceID);
                    //request.Headers.Add("X-IG-Family-Device-ID", GetGuid());
                    //request.Headers.Add("X-IG-Android-ID", GetMobileDeviceId());
                    //request.Headers.Add("X-IG-Timezone-Offset", "19800");
                    ////request.Headers.Add("X-IG-Nav-Chain", "MainFeedFragment:feed_timeline:1:cold_start:1737709686.720:10#230#301:3552032383224948675,UserDetailFragment:profile:3:media_owner:1737709705.641::,ClipsProfileTabFragment:clips_profile:4:button:1737709707.208::,ProfileMediaTabFragment:profile:5:button:1737709712.452::");
                    //request.Headers.Add("X-IG-CLIENT-ENDPOINT", "ProfileMediaTabFragment:profile");
                    ////request.Headers.Add("X-IG-SALT-LOGGER-IDS", "566762171,974456048,31784979,25624577,25101347,42991645,25952257,42991646,61669378,974462634");
                    //request.Headers.Add("X-FB-Connection-Type", "WIFI");
                    //request.Headers.Add("X-IG-Connection-Type", "WIFI");
                    //request.Headers.Add("X-IG-Capabilities", "3brTv10=");
                    //request.Headers.Add("X-IG-App-ID", "567067343352427");
                    //request.Headers.Add("Authorization", reqParam.AuthToken);
                    //request.Headers.Add("X-MID", reqParam.MID);
                    ////request.Headers.Add("IG-U-IG-DIRECT-REGION-HINT", "CLN,68839847230,1768988104:01f7d4e2c91e88bec95468fa64733a74668ac168f62b3d1377a9f8e0f94bcc986097a2b2");
                    //request.Headers.Add("IG-U-DS-USER-ID", reqParam.UserID);
                    //request.Headers.Add("IG-U-RUR", reqParam.RUR);
                    //request.Headers.Add("IG-INTENDED-USER-ID", reqParam.UserID);

                    //// Prepare the request body
                    //string postData = $"_uuid={deviceID}";
                    //byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                    //// Write the request body
                    //using (Stream dataStream = request.GetRequestStream())
                    //{
                    //    dataStream.Write(byteArray, 0, byteArray.Length);
                    //}

                    //try
                    //{
                    //    // Get the response from the server
                    //    HttpWebResponse response1 = (HttpWebResponse)request.GetResponse();

                    //    // Read the response
                    //    using (StreamReader reader = new StreamReader(response1.GetResponseStream()))
                    //    {
                    //        string responseText = reader.ReadToEnd();
                    //        Console.WriteLine("Response Code: " + response1.StatusCode);
                    //        Console.WriteLine("Response Body: " + responseText);
                    //    }
                    //}
                    //catch (WebException ex)
                    //{
                    //    using (StreamReader reader = new StreamReader(ex.Response.GetResponseStream()))
                    //    {
                    //        string errorResponse = reader.ReadToEnd();
                    //        Console.WriteLine("Error: " + ex.Message);
                    //        Console.WriteLine("Error Response: " + errorResponse);
                    //    }
                    //}

                    #endregion
                }
                else
                {
                    api = await api.Build();
                    if (api.instaApi.IsUserAuthenticated)
                    {
                        var feedresponse = await api.instaApi.UserProcessor.GetUserMediaAsync(profileUrl, paginationParameters:
                            PaginationParameters.MaxPagesToLoad(2));
                        return new FeedResponseHandler(feedresponse.Value.Serialize(), true);
                    }
                }

            }
            catch (Exception e)
            {
                response.NotFound = true;
                logger.Log($"{profileUrl} Error ==> {e?.Message}");
            }
            return response;
        }

        public async Task<string> GetRequestAsync(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                var response = await request.GetResponseAsync();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch { return string.Empty; }
        }

        public async Task<string> PostAsync(string postAPI, string? jsonResponse)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postAPI);
                request.Method = "POST";
                request.ContentType = "application/json";
                var buffer = Encoding.UTF8.GetBytes(jsonResponse);
                request.ContentLength = buffer.Length;
                var requestStream = await request.GetRequestStreamAsync();
                requestStream.Write(buffer, 0, buffer.Length);
                var response = await request.GetResponseAsync();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch { return string.Empty; }
        }
    }
    public class SessionRequestParam
    {
        public string UserID { get; set; }
        public string MID { get; set; }
        public string AuthToken { get; set; }
        public string RUR { get; set; }
        public string Hmac { get; set; }
        public string Response { get; set; }
        public string IGURUR { get; set; }
        public string OtherProfileID { get; set; }
        public CookieContainer Cookies { get; set; } = new CookieContainer();
    }
    public class InstagramUser : ICloneable
    {
        public bool SwaggerAPIInitialized { get; set; }
        public bool WebInitialized { get; set; }
        public string JsonCookies { get; set; }
        public bool MobileInitialized { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public int AccountId { get; set; }

        public Proxy Proxy { get; set; } = new Proxy();

        public string Email { get; set; }
        public string BatteryLevel { get; set; } = RandomUtilties.GetRandomNumber(100, 8).ToString();
        public string EmailPass { get; set; }

        public string UserId { get; set; }
        public string CloneWithUser { get; set; }

        public string PostsDone { get; set; }

        public string AutoLikedDone { get; set; }

        public string UserAgentMobile { get; set; }

        public string VarificationCode { get; set; } = "";

        public string RolloutHash { get; set; } = "";

        public string XIGWWWClaim { get; set; } = "";

        public Dictionary<string, Dictionary<string, string>> Experiments { get; set; } = new Dictionary<string, Dictionary<string, string>>();

        public CookieCollection WebCookies { get; set; }

        public CookieCollection MobileCookies { get; set; }
        public CookieCollection OutlookSession { get; set; }
        public object Clone()
        {
            return MemberwiseClone();
        }

        public string Name { get; set; }
        public string Phone { get; set; }
        public string Gender { get; set; }
        public string Biography { get; set; }
        public string ExternalUrls { get; set; }
        public bool IsPrivate { get; set; }
        public string ProfilePicFileUrl { get; set; }
        public bool IsWebLoggedIn { get; set; }
        public bool IsMobileLoggedIn { get; set; }

        public int LastLoginAt { get; set; }

        public string publicKey { get; set; }
        public string publicId { get; set; }
        public string EncPwd { get; set; }

        public string UserAgentMobileWeb { get; set; }
        public string AccountVerifyType { get; set; }
        public string ChallengeContext { get; set; }
        public string PerfLoggingId { get; set; }
        public string CountryName { get; set; }
        public AccountModel accountModel { get; set; } = new AccountModel();
        public bool UpdateMIDAndAuthToken(string MID, string AuthToken)
        {
            try
            {
                if (!string.IsNullOrEmpty(MID))
                    accountModel.MidHeader = MID;
                if (!string.IsNullOrEmpty(AuthToken))
                    accountModel.AuthorizationHeader = AuthToken;
                return true;
            }
            catch { return false; }
        }
    }
    public class AccountModel
    {
        public string AuthorizationHeader { get; set; }
        public string MidHeader { get; set; }

        public string CommentSessionId { get; set; }
    }
}
