using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace p2p_Screener
{
    internal static class ByBit
    {
        public class CurrencyConfig
        {
            public int scale { get; set; }
            public int amountScale { get; set; }
        }
        public class Item
        {
            public string id { get; set; }
            public string accountId { get; set; }
            public string userId { get; set; }
            public string nickName { get; set; }
            public string tokenId { get; set; }
            public string tokenName { get; set; }
            public string currencyId { get; set; }
            public int side { get; set; }
            public int priceType { get; set; }
            public string price { get; set; }
            public string premium { get; set; }
            public string lastQuantity { get; set; }
            public string quantity { get; set; }
            public string frozenQuantity { get; set; }
            public string executedQuantity { get; set; }
            public string minAmount { get; set; }
            public string maxAmount { get; set; }
            public string remark { get; set; }
            public int status { get; set; }
            public string createDate { get; set; }
            public List<int> payments { get; set; }
            public int orderNum { get; set; }
            public int finishNum { get; set; }
            public int recentOrderNum { get; set; }
            public int recentExecuteRate { get; set; }
            public string fee { get; set; }
            public TokenConfig tokenConfig { get; set; }
            public CurrencyConfig currencyConfig { get; set; }
            public bool isOnline { get; set; }
            public string lastLogoutTime { get; set; }
            public TradingPreferenceSet tradingPreferenceSet { get; set; }
            public string blocked { get; set; }
            public bool makerContact { get; set; }
            public int authStatus { get; set; }
        }
        public class Result
        {
            public int count { get; set; }
            public List<Item> items { get; set; }
        }
        public class BybitP2PTicker
        {
            public int ret_code { get; set; }
            public string ret_msg { get; set; }
            public Result result { get; set; }
            public object token { get; set; }
        }
        public class TokenConfig
        {
            public string minQuote { get; set; }
            public string maxQuote { get; set; }
            public int scale { get; set; }
            public string upRange { get; set; }
            public string downRange { get; set; }
        }
        public class TradingPreferenceSet
        {
            public int hasUnPostAd { get; set; }
            public int isKyc { get; set; }
            public int isEmail { get; set; }
            public int isMobile { get; set; }
            public int hasRegisterTime { get; set; }
            public int registerTimeThreshold { get; set; }
        }


        public static async Task<List<P2POrder>> GetOrdersAsync(OrderType orderType = OrderType.Sell, 
                                                                List<PaymentMethod> payments = null)
        {
            string operation = orderType == OrderType.Buy ? "1" : "0";
            if (payments == null)
                return await Curl(operation, "");

            List<P2POrder> orders = new List<P2POrder>();
            foreach (var p in payments)
                orders.AddRange(await Curl(operation, ((int)p).ToString()));

            if (orderType == OrderType.Buy)
                orders.OrderBy(o => o.Price);
            else
                orders.OrderByDescending(o => o.Price);

            orders.RemoveRange(10, orders.Count - 10);

            return orders;
        }

        private static async Task<List<P2POrder>> Curl(string op, string payment)
        {
            var client = new RestClient("https://api2.bybit.com/spot/api/otc/item/list");
            var request = new RestRequest();

            request.Method = Method.Post;
            request.AddHeader("authority", "api2.bybit.com");
            request.AddHeader("accept", "application/json");
            request.AddHeader("accept-language", "uk");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddHeader("lang", "uk");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");

            request.AddParameter("userId", "");
            request.AddParameter("tokenId", "USDT");
            request.AddParameter("currencyId", "UAH");
            request.AddParameter("payment", payment);
            request.AddParameter("side", op); 
            request.AddParameter("size", "10");
            request.AddParameter("page", "1");
            request.AddParameter("amount", "");

            var response = await client.ExecuteAsync(request);
            var json = response.Content;

            BybitP2PTicker ticker = JsonConvert.DeserializeObject<BybitP2PTicker>(json);

            var orders = ticker.result.items.Select(i =>
            {
                return
                new P2POrder(OrderType.Buy, CryptoCurrency.USDT, FiatCurrency.UAH, decimal.Parse(i.price, CultureInfo.InvariantCulture))
                {
                    Limits = (decimal.Parse(i.minAmount, CultureInfo.InvariantCulture), decimal.Parse(i.maxAmount, CultureInfo.InvariantCulture)),
                    Available = decimal.Parse(i.quantity, CultureInfo.InvariantCulture),
                    NickName = i.nickName,
                    Comment = i.remark,
                    PaymentMethods = i.payments.Select(m => (PaymentMethod)m).ToList(),
                };
            }).ToList();
            return orders;
        }
    }
}
