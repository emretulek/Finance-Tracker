using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS8625, CS8600, CS8604, CS8602, CS8618, CA2101
namespace Finance_Tracker
{
    public partial class TLSSession
    {
        [DllImport("tls-client-windows-64-1.7.9.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr request(byte[] requestPayload, string sessionID);

        [DllImport("tls-client-windows-64-1.7.9.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void freeMemory(string sessionID);

        private readonly string sessionID;
        private readonly RequestPayload sessionPayload;

        public TLSSession(Dictionary<string, string> headers = null, string TlsClientIdentifier = "chrome_124", int TimeoutSeconds = 30, bool FollowRedirects = true, string proxy = null)
        {
            this.sessionID = Guid.NewGuid().ToString();

            this.sessionPayload = new RequestPayload
            {
                TlsClientIdentifier = TlsClientIdentifier,
                FollowRedirects = FollowRedirects,
                InsecureSkipVerify = false,
                IsByteRequest = false,
                ForceHttp1 = false,
                WithDebug = false,
                CatchPanics = false,
                WithRandomTLSExtensionOrder = true,
                sessionId = this.sessionID,
                TimeoutSeconds = TimeoutSeconds,
                TimeoutMilliseconds = 0,
                CertificatePinningHosts = [],
                ProxyUrl = "",
                IsRotatingProxy = false,
                Headers = headers ?? [],
                HeaderOrder = headers != null ? new List<string>(headers.Keys) : [],
                RequestUrl = "",
                RequestMethod = "",
                RequestBody = "",
            };
            if (proxy != null)
            {
                Console.WriteLine(proxy);
                this.sessionPayload.ProxyUrl = proxy;
            }
        }

        public async Task<RequestResult> Get(string url, Dictionary<string, string> additionalHeaders = null)
        {
            return await Task.Run(() => this.MakeRequest("GET", url, MergeHeaders(this.sessionPayload.Headers, additionalHeaders)));
        }

        public async Task<RequestResult> Post(string url, Dictionary<string, string> additionalHeaders = null, string body = "")
        {
            return await Task.Run(() => this.MakeRequest("POST", url, MergeHeaders(this.sessionPayload.Headers, additionalHeaders), body));
        }
        public async Task<RequestResult> Patch(string url, Dictionary<string, string> additionalHeaders = null, string body = "")
        {
            return await Task.Run(() => this.MakeRequest("PATCH", url, MergeHeaders(this.sessionPayload.Headers, additionalHeaders), body));
        }
        public async Task<RequestResult> Put(string url, Dictionary<string, string> additionalHeaders = null, string body = "")
        {
            return await Task.Run(() => this.MakeRequest("PUT", url, MergeHeaders(this.sessionPayload.Headers, additionalHeaders), body));
        }
        public async Task<RequestResult> Delete(string url, Dictionary<string, string> additionalHeaders = null)
        {
            return await Task.Run(() => this.MakeRequest("DELETE", url, MergeHeaders(this.sessionPayload.Headers, additionalHeaders)));
        }
        private RequestResult MakeRequest(string method, string url, Dictionary<string, string> headers, string body = "")
        {
            this.sessionPayload.RequestMethod = method;
            this.sessionPayload.RequestUrl = url;
            this.sessionPayload.Headers = headers;
            this.sessionPayload.RequestBody = body;

            string requestJson = JsonConvert.SerializeObject(this.sessionPayload);
            byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);

            IntPtr responsePtr = request(requestBytes, this.sessionID);
            string responseJson = Marshal.PtrToStringAnsi(responsePtr);

            RequestResult result = JsonConvert.DeserializeObject<RequestResult>(responseJson);
            freeMemory(result.Id);

            return result;
        }

        private static Dictionary<string, string> MergeHeaders(Dictionary<string, string> originalHeaders, Dictionary<string, string> additionalHeaders)
        {
            if (additionalHeaders == null) return originalHeaders;

            foreach (var header in additionalHeaders)
            {
                originalHeaders[header.Key] = header.Value;
            }

            return originalHeaders;
        }
    }


    public class RequestResult
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public object Cookies { get; set; }
        public Dictionary<string, List<string>> Headers { get; set; }
        public int Status { get; set; }
        public string Target { get; set; }
        public string UsedProtocol { get; set; }
    }

    public class RequestPayload
    {
        public string TlsClientIdentifier { get; set; } = "chrome_105";
        public bool FollowRedirects { get; set; } = true;
        public bool InsecureSkipVerify { get; set; } = false;
        public bool WithoutCookieJar { get; set; } = false;
        public bool WithDefaultCookieJar { get; set; } = false;
        public bool IsByteRequest { get; set; } = false;
        public bool ForceHttp1 { get; set; } = false;
        public bool WithDebug { get; set; } = false;
        public bool CatchPanics { get; set; } = false;
        public bool WithRandomTLSExtensionOrder { get; set; } = false;
        public string sessionId { get; set; } = "Nada";
        public int TimeoutSeconds { get; set; } = 30;
        public int TimeoutMilliseconds { get; set; } = 0;
        public Dictionary<string, string> CertificatePinningHosts { get; set; } = [];
        public string ProxyUrl { get; set; } = "";
        public bool IsRotatingProxy { get; set; } = false;
        public Dictionary<string, string> Headers { get; set; } = [];
        public List<string> HeaderOrder { get; set; }
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public string RequestBody { get; set; }
    }

}
#pragma warning restore CS8625, CS8600, CS8604, CS8602, CS8618, CA2101
