using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TTTHttpClient
{
    /// <summary>
    /// 
    /// </summary>
    public static class SmartClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="situation"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static async Task<dynamic> GetMyRecommendation(string situation, string player)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://stujo-tic-tac-toe-stujo-v1.p.rapidapi.com");
                client.DefaultRequestHeaders.Add(
                    "X-RapidAPI-Host",
                    "stujo-tic-tac-toe-stujo-v1.p.rapidapi.com");
                client.DefaultRequestHeaders.Add(
                    "X-RapidAPI-Key",
                    "b7fdb0b11bmsh7d55e56332a5030p191336jsn479b0dff677f");

                var response = await client.GetAsync($"/{situation}/{player}");
                var stringResult = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject(stringResult);
            }
        }
    }
}
