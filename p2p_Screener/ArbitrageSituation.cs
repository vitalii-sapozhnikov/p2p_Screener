namespace p2p_Screener
{
    internal class ArbitrageSituation
    {
        public string Title { get; set; }
        public decimal BankComission { get; set; }
        public decimal ExchangeComission { get; set; }
        public decimal AmountUAH { get; set; }
        public decimal BuyPrice { get; set; }
        public P2POrder SellOrder { get; set; }
        public decimal GetProfitUAH()
        {
            decimal profitPerUSDT = (SellOrder.Price * (1 - BankComission / 100)) - (BuyPrice * (1 + ExchangeComission / 100));
            return profitPerUSDT * 0.999m * (AmountUAH / SellOrder.Price);
        }
        public string GetDescription()
        {
            return $"<b>{Title}</b>\r\nSell:           {SellOrder.GetBanksString()} -- {SellOrder.Price:F2}\r\n" +
                $"Range:      {SellOrder.Limits.Min:F0} - {SellOrder.Limits.Max:F0} ₴\r\n" +
                $"Name:      {SellOrder.NickName}\r\n" +
                $"Buy:          {BuyPrice:F2}\r\n" +
                $"<b>Profit:      {GetProfitUAH():F2}</b>";
        }

        public ArbitrageSituation()
        {
            SellOrder = new P2POrder();
            Title = string.Empty;
        }
    }
}
