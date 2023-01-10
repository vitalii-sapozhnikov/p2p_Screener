namespace p2p_Screener
{
    internal class ArbitrageConfig
    {
        public decimal PrivatComission { get; set; }
        public decimal OtherComission { get; set; }
        public decimal WhiteBitDepositComission { get; set; }
        public decimal BinanceDepositComission { get; set; }
        public decimal WhiteBitMarketComission { get; set; }
        public decimal BinanceMarketComission { get; set; }
        public decimal TransactionAmount { get; set; }

        public ArbitrageConfig()
        {
            PrivatComission = 0.5m;
            OtherComission = 0.0m;
            WhiteBitDepositComission = 0.0m;
            BinanceDepositComission = 1.5m;
            WhiteBitMarketComission = 0.1m;
            BinanceMarketComission = 0.1m;
            TransactionAmount = 40000m;
        }
    }
}
