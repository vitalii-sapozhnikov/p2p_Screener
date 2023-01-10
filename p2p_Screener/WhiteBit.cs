using Newtonsoft.Json;
using RestSharp;

namespace p2p_Screener
{
    internal static class WhiteBit
    {
        public static async Task<decimal> GetMarketPriceUSDT_UAH_Aync()
        {
            var client = new RestClient("https://whitebit.com");
            var request = new RestRequest("/api/v4/public/ticker");

            var response = await client.GetAsync(request);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);
            return obj.USDT_UAH.last_price;
        }
    }
}
