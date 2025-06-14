using System;

namespace Capsa_Connector.Model
{
    public class AppTokenInfoDataWrap
    {
        public AppTokenInfoWrap? data { get; set; }
    }
    public class AppTokenInfoWrap
    {
        public AppTokenInfo? appToken { get; set; }

    }
    public class AppTokenInfo
    {
        public DateTime? appAccessedAt { get; set; }
        public string? token { get; set; }
        public User? user { get; set; }
        public DateTime? webAccessedAt { get; set; }
    }
    public class User
    {
        public string? email { get; set; }
        public string? firstName { get; set; }
        public string? lastName { get; set; }
    }

}
