using System.Collections.Generic;
using System.Net;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Core.Tools
{
    public static class StatusCodeChecker
    {
        public static List<int> SuccessfulStatusCodes { get; set; } = new List<int>
        {
            (int)HttpStatusCode.OK,
            (int)HttpStatusCode.Created,
            (int)HttpStatusCode.Accepted,
            (int)HttpStatusCode.NonAuthoritativeInformation,
            (int)HttpStatusCode.NoContent,
            (int)HttpStatusCode.ResetContent,
            (int)HttpStatusCode.PartialContent,
            (int)HttpStatusCode.MultiStatus,
            (int)HttpStatusCode.AlreadyReported,
            (int)HttpStatusCode.IMUsed
        };
        public static bool isSuccessful(HttpStatusCode? code, HttpStatusCode wanted)
        {
            if (code != wanted) v($"Code check was expecting {wanted} status code and obtained {code}", true);
            if (code == null) return false;
            if (SuccessfulStatusCodes.Contains((int)code)) return true;
            return false;
        }
    }
}
