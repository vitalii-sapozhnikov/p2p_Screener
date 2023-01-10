namespace p2p_Screener
{
    enum OrderType { Buy, Sell};
    enum CryptoCurrency { USDT };
    enum FiatCurrency { UAH };
    enum PaymentMethod { ABank = 1, Mono = 43, Privat = 60, PUMB = 61, Sport = 72, Izi, None = -1};
    internal class P2POrder
    {
        public OrderType Type { get; set;}
        public CryptoCurrency CryptoCurrency { get; set;}
        public FiatCurrency FiatCurrency { get; set;} 
        public decimal Price { get; set; }
        public decimal Available { get; set; }
        public (decimal Min, decimal Max) Limits { get; set; }
        public ICollection<PaymentMethod> PaymentMethods { get; set; }
        public string NickName { get; set; }
        public string Comment { get; set; }

        public P2POrder(OrderType type, CryptoCurrency cryptoCurrency, FiatCurrency fiatCurrency, decimal price)
        {
            Type = type;
            CryptoCurrency = cryptoCurrency;
            FiatCurrency = fiatCurrency;
            Price = price;
            PaymentMethods = new List<PaymentMethod>();
            NickName = string.Empty;
            Comment = string.Empty;
        }

        public P2POrder()
        {
        }
        public string GetBanksString()
        {
            return string.Join(", ", PaymentMethods.Select(m => m.ToString()).ToArray());
        }

        public string ToPrint()
        {
            return $"Nickname: {NickName}\r\n{Type.ToString()} price: {Price}\r\nRange: {Limits.Min} - {Limits.Max}\r\n" +
                $"Payment methods: {string.Join(", ", PaymentMethods.Select(p => p.ToString()).ToArray())}\r\n" +
                $"Comment: {Comment}\r\n\r\n";
        }
    }
}
