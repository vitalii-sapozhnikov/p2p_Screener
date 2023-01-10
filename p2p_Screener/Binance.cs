using RestSharp;
using Newtonsoft.Json;
using System.Globalization;

namespace p2p_Screener
{
    internal static class Binance
    {
        public class Adv
        {
            public string advNo { get; set; }
            public string classify { get; set; }
            public string tradeType { get; set; }
            public string asset { get; set; }
            public string fiatUnit { get; set; }
            public object advStatus { get; set; }
            public object priceType { get; set; }
            public object priceFloatingRatio { get; set; }
            public object rateFloatingRatio { get; set; }
            public object currencyRate { get; set; }
            public string price { get; set; }
            public object initAmount { get; set; }
            public string surplusAmount { get; set; }
            public object amountAfterEditing { get; set; }
            public string maxSingleTransAmount { get; set; }
            public string minSingleTransAmount { get; set; }
            public object buyerKycLimit { get; set; }
            public object buyerRegDaysLimit { get; set; }
            public object buyerBtcPositionLimit { get; set; }
            public object remarks { get; set; }
            public string autoReplyMsg { get; set; }
            public object payTimeLimit { get; set; }
            public List<TradeMethod> tradeMethods { get; set; }
            public object userTradeCountFilterTime { get; set; }
            public object userBuyTradeCountMin { get; set; }
            public object userBuyTradeCountMax { get; set; }
            public object userSellTradeCountMin { get; set; }
            public object userSellTradeCountMax { get; set; }
            public object userAllTradeCountMin { get; set; }
            public object userAllTradeCountMax { get; set; }
            public object userTradeCompleteRateFilterTime { get; set; }
            public object userTradeCompleteCountMin { get; set; }
            public object userTradeCompleteRateMin { get; set; }
            public object userTradeVolumeFilterTime { get; set; }
            public object userTradeType { get; set; }
            public object userTradeVolumeMin { get; set; }
            public object userTradeVolumeMax { get; set; }
            public object userTradeVolumeAsset { get; set; }
            public object createTime { get; set; }
            public object advUpdateTime { get; set; }
            public object fiatVo { get; set; }
            public object assetVo { get; set; }
            public object advVisibleRet { get; set; }
            public object assetLogo { get; set; }
            public int assetScale { get; set; }
            public int fiatScale { get; set; }
            public int priceScale { get; set; }
            public string fiatSymbol { get; set; }
            public bool isTradable { get; set; }
            public string dynamicMaxSingleTransAmount { get; set; }
            public string minSingleTransQuantity { get; set; }
            public string maxSingleTransQuantity { get; set; }
            public string dynamicMaxSingleTransQuantity { get; set; }
            public string tradableQuantity { get; set; }
            public string commissionRate { get; set; }
            public List<object> tradeMethodCommissionRates { get; set; }
            public object launchCountry { get; set; }
            public object abnormalStatusList { get; set; }
            public object closeReason { get; set; }
        }
        public class Advertiser
        {
            public string userNo { get; set; }
            public object realName { get; set; }
            public string nickName { get; set; }
            public object margin { get; set; }
            public object marginUnit { get; set; }
            public object orderCount { get; set; }
            public int monthOrderCount { get; set; }
            public double monthFinishRate { get; set; }
            public object advConfirmTime { get; set; }
            public object email { get; set; }
            public object registrationTime { get; set; }
            public object mobile { get; set; }
            public string userType { get; set; }
            public List<object> tagIconUrls { get; set; }
            public int userGrade { get; set; }
            public string userIdentity { get; set; }
            public object proMerchant { get; set; }
            public object isBlocked { get; set; }
        }
        public class Datum
        {
            public Adv adv { get; set; }
            public Advertiser advertiser { get; set; }
        }
        public class BinanceP2PTicker
        {
            public string code { get; set; }
            public object message { get; set; }
            public object messageDetail { get; set; }
            public List<Datum> data { get; set; }
            public int total { get; set; }
            public bool success { get; set; }
        }
        public class TradeMethod
        {
            public object payId { get; set; }
            public string payMethodId { get; set; }
            public object payType { get; set; }
            public object payAccount { get; set; }
            public object payBank { get; set; }
            public object paySubBank { get; set; }
            public string identifier { get; set; }
            public object iconUrlColor { get; set; }
            public string tradeMethodName { get; set; }
            public string tradeMethodShortName { get; set; }
            public string tradeMethodBgColor { get; set; }
        }

        public static async Task<List<P2POrder>> GetOrdersAsync(decimal amount = 40000,
                                                                OrderType orderType = OrderType.Sell, 
                                                                List<PaymentMethod>? payments = null)
        {
            var client = new RestClient("https://p2p.binance.com/bapi/c2c/v2/friendly/c2c/adv/search");
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("authority", "p2p.binance.com");
            request.AddHeader("accept", "application/json");
            request.AddHeader("accept-language", "uk,en;q=0.9,en-GB;q=0.8,en-US;q=0.7,ru;q=0.6");
            request.AddHeader("lang", "uk");
            request.AddHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.107 Safari/537.36");

            string[] paymentsArr;
            if (payments != null)
            {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                paymentsArr = payments.Select(p =>
                {
                    switch (p)
                    {
                        case PaymentMethod.ABank:
                            return "ABank";
                        case PaymentMethod.Mono:
                            return "Monobank";
                        case PaymentMethod.Privat:
                            return "PrivatBank";
                        case PaymentMethod.PUMB:
                            return "PUMBBank";
                        case PaymentMethod.Sport:
                            return "Sportbank";
                        case PaymentMethod.Izi:
                            return "izibank";
                        default:
                            return null;
                    }
                }).Where(p => p != null).ToArray();
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
            }
            else
                paymentsArr = new string[0];
            

            var reqObj = new
            {
                page = 1,
                rows = 10,
                payTypes = paymentsArr,
                countries = new string[0],
                asset = "USDT",
                fiat = "UAH",
                tradeType = orderType == OrderType.Buy ? "BUY" : "SELL",
                transAmount = amount,
            };
            var jsonPOSTobj = JsonConvert.SerializeObject(reqObj);

            var response = await client.ExecuteAsync(request.AddJsonBody(jsonPOSTobj));
            var json = response.Content;

            BinanceP2PTicker ticker = JsonConvert.DeserializeObject<BinanceP2PTicker>(json);

            List<P2POrder> orders = ticker.data.Select(o =>
            {
                return new P2POrder(orderType, CryptoCurrency.USDT, FiatCurrency.UAH, decimal.Parse(o.adv.price, CultureInfo.InvariantCulture))
                {
                    Limits = (decimal.Parse(o.adv.minSingleTransAmount, CultureInfo.InvariantCulture), decimal.Parse(o.adv.dynamicMaxSingleTransAmount, CultureInfo.InvariantCulture)),
                    Available = decimal.Parse(o.adv.surplusAmount, CultureInfo.InvariantCulture),
                    NickName = o.advertiser.nickName,
                    Comment = null,
                    PaymentMethods = o.adv.tradeMethods.Select(m =>
                    {
                        switch (m.identifier)
                        {
                            case "ABank":
                                return PaymentMethod.ABank;
                            case "Monobank":
                                return PaymentMethod.Mono;
                            case "PrivatBank":
                                return PaymentMethod.Privat;
                            case "PUMBBank":
                                return PaymentMethod.PUMB;
                            case "Sportbank":
                                return PaymentMethod.Sport;
                            case "izibank":
                                return PaymentMethod.Izi;
                            default:
                                return PaymentMethod.None;
                        }
                    }).Where(m => m != PaymentMethod.None).ToList(),
                };
            }).ToList();
            
            return orders;
        }
        public static async Task<decimal> GetMarketPriceUSDT_UAH_Async()
        {
            var client = new RestClient("https://api.binance.com/api/v3/ticker/price");
            var request = new RestRequest();
            request.AddParameter("symbol", "USDTUAH");

            var response = await client.GetAsync(request);
            dynamic obj = JsonConvert.DeserializeObject(response.Content);
            return obj?.price;
        }
    }
}
