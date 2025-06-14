using System;
using System.Diagnostics;
using Capsa_Connector.Core.Helpers;
using RestSharp;
using static Capsa_Connector.Core.Tools.Log;

namespace Capsa_Connector.Controller.Core.Helpers;
using static Capsa_Connector.Core.Tools.Log;

public static class ErrorReport
{
    public enum Level
    {
         Debug = 7,
         Info = 6,
         Notice = 5,
         Warning = 4,
         Error = 3,
         Critical = 2,
         Alert = 1,
         Emergency = 0
    }
    public static async void report(string message, Level level)
    {
        try
        {
            RestHandler rest = new RestHandler();
            var client = rest.GetRestClient();
            var request = rest.getRestRequest($"report", Method.Post);
            
            
            string mes = "{\"message\":\"" + message + "\",\"level\":\"" + (int) level +"\"}";
            
            request.AddParameter("application/json", mes, ParameterType.RequestBody);
    
            var response = await client.ExecuteAsync(request);
        }
        catch (Exception ex)
        {
            v($"Error AppendFileOnlineEdit -> {ex.Message}");
        }
    }
}