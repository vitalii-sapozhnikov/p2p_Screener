using p2p_Screener;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

internal class Program
{
    private static string Token { get; set; } = "5756002034:AAEgi1BjAhT747qZH8xkYcZ-dkRMsCKv-pM";
    private static TelegramBotClient Client { get; set; }
    private static ArbitrageConfig Config { get; set; }
    public static void Log(string message)
    {
        System.IO.File.AppendAllText("log.txt", $"{DateTime.Now}:\t{message}");
    }
    private static async Task Main(string[] args)
    {
        Client = new TelegramBotClient(Token);
        Config = new ArbitrageConfig();
        using CancellationTokenSource cts = new();

        ReceiverOptions receiverOptions = new()
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        Client.StartReceiving(
            updateHandler: HandleUpdateAsync,
            pollingErrorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions
        );
        var commandList = new List<BotCommand>()
        {
            new BotCommand() 
            { 
                Command = "/rates", 
                Description = "Returns market values for USDT" 
            },
            new BotCommand() 
            { 
                Command = "/p2p_binance", 
                Description = "Returns P2P orders on Binance" 
            },
            new BotCommand() 
            { 
                Command = "/p2p_bybit", 
                Description = "Returns P2P orders on ByBit" 
            },
            new BotCommand()
            {
                Command = "/get_binance_arbitrage",
                Description = "Returns arbitrage situations on Binance"
            },
        };

        await Client.SetMyCommandsAsync(commandList, new BotCommandScopeAllPrivateChats());

        var me = await Client.GetMeAsync();

        Log($"Start listening for @{me.Username}");
        Console.ReadLine();

        // Send cancellation request to stop bot
        cts.Cancel();

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            Log($"Received a '{messageText}' message in chat {chatId}.");

            if (messageText == "/rates")
            {
                var binanceRate = Binance.GetMarketPriceUSDT_UAH_Async();
                var whiteBitRate = WhiteBit.GetMarketPriceUSDT_UAH_Aync();
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: $"Binance:      {await binanceRate:F2} ₴    ({await binanceRate * 1.015m:F2} ₴)\r\n" +
                    $"WhiteBit:     {await whiteBitRate:F2} ₴");
            }
            if (messageText == "/p2p_binance")
            {
                var binanceList = await Binance.GetOrdersAsync();
                string response = String.Join("\r\n", 
                       binanceList.Select(r => $"Price: {r.Price,7:F2}\r\n" +
                                               $"Range: {r.Limits.Min} - {r.Limits.Max} ₴\r\n" +
                                               $"Name:  {r.NickName}\r\n" +
                                               $"Banks:  {r.GetBanksString()}\r\n").ToList());                
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: response);
            }
            if (messageText == "/p2p_bybit")
            {
                var binanceList = await ByBit.GetOrdersAsync();
                string response = String.Join("\r\n",
                       binanceList.Select(r => $"Price: {r.Price,7:F2}\r\n" +
                                               $"Range: {r.Limits.Min} - {r.Limits.Max} ₴\r\n" +
                                               $"Name:  {r.NickName}\r\n" +
                                               $"Banks:  {r.GetBanksString()}\r\n").ToList());
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: response);
            }
            if (messageText == "/get_binance_arbitrage")
            {
                var binanceP2P_Privat = Binance.GetOrdersAsync(Config.TransactionAmount, OrderType.Sell,
                    new List<PaymentMethod>() { PaymentMethod.Privat });

                var binanceP2P_other = Binance.GetOrdersAsync(Config.TransactionAmount, OrderType.Sell,
                    new List<PaymentMethod>() {PaymentMethod.ABank, PaymentMethod.Mono,
                        PaymentMethod.PUMB, PaymentMethod.Sport, PaymentMethod.Izi });

                var binanceMarket = Binance.GetMarketPriceUSDT_UAH_Async();
                var whiteBitMarket = WhiteBit.GetMarketPriceUSDT_UAH_Aync();

                var arbitrageList = new List<ArbitrageSituation>()
                {
                    new ArbitrageSituation()
                    {
                        Title = "BinanceP2P → Privat → Mono → Whitebit",
                        BankComission = Config.PrivatComission,
                        ExchangeComission = Config.WhiteBitDepositComission,
                        SellOrder = (await binanceP2P_Privat).First(),
                        BuyPrice = await whiteBitMarket,
                        AmountUAH = Config.TransactionAmount,
                    },
                    new ArbitrageSituation()
                    {
                        Title = "BinanceP2P → OtherBank → Mono → Whitebit",
                        BankComission = Config.OtherComission,
                        ExchangeComission = Config.WhiteBitDepositComission,
                        SellOrder = (await binanceP2P_other).First(),
                        BuyPrice = await whiteBitMarket,
                        AmountUAH = Config.TransactionAmount,
                    },
                    new ArbitrageSituation()
                    {
                        Title = "BinanceP2P → Privat → Binance",
                        BankComission = Config.PrivatComission,
                        ExchangeComission = Config.BinanceDepositComission,
                        SellOrder = (await binanceP2P_Privat).First(),
                        BuyPrice = await binanceMarket,
                        AmountUAH = Config.TransactionAmount,
                    },
                    new ArbitrageSituation()
                    {
                        Title = "BinanceP2P → OtherBank → Binance",
                        BankComission = Config.OtherComission,
                        ExchangeComission = Config.BinanceDepositComission,
                        SellOrder = (await binanceP2P_Privat).First(),
                        BuyPrice = await binanceMarket,
                        AmountUAH = Config.TransactionAmount,
                    },
                };
                arbitrageList = arbitrageList.OrderByDescending(s => s.GetProfitUAH()).ToList();

                string response = string.Join("\r\n\r\n", arbitrageList.Select(n => n.GetDescription()));
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: response, parseMode: ParseMode.Html);
            }
        }

        Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Log(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}