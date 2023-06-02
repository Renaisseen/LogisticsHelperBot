using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Google.Maps.Direction;
using Google.Maps;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TelegramGoogleDirectionsBot
{
    class LogisticsHelper
    {
        private static TelegramBotClient botClient;
        public static string _startPoint = string.Empty;
        public static string _endPoint = string.Empty;
        static string _keyGoogleApi = "GOOGLE_API_KEY";
        static void Main(string[] args)
        {
            botClient = new TelegramBotClient("TELEGRAM_BOT_TOKEN");
            GoogleSigned.AssignAllServices(new GoogleSigned(_keyGoogleApi));
            botClient.OnMessage += BotClient_OnMessage;
            botClient.StartReceiving();

            Console.WriteLine("Bot started. Press any key to exit.");
            Console.ReadKey();

            // Stop the bot
            botClient.StopReceiving();
        }

        private static async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Text)
            {
                string messageText = e.Message.Text;

                if (messageText.StartsWith("/analyze"))
                {
                    // Handle the start command
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id, "Enter the starting point (Point A).");
                }
                else if (messageText.StartsWith("/details"))
                {
                    // Handle the help command
                    await botClient.SendTextMessageAsync(e.Message.Chat.Id,
                        "This bot helps you find the fastest route between two points with possible cities to go through. To get started, use analyse button.\nYou can see manual on https://github.com/Renaisseen/LogicticHelperBot");
                }
                else
                {
                    // Handle the user input
                    await HandleUserInput(e.Message);
                }
            }
        }

        private static async Task HandleUserInput(Message message)
        {
            if (message.ReplyToMessage != null && message.ReplyToMessage.Text == "Enter the starting point (Point A).")
            {
                // Point A
                _startPoint = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter the destination (Point B).", replyToMessageId: message.MessageId);
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text == "Enter the destination (Point B).")
            {
                // Point B
                _endPoint = message.Text;
                await botClient.SendTextMessageAsync(message.Chat.Id, "Enter possible cities for staying the night (separated by commas).", replyToMessageId: message.MessageId);
            }
            else if (message.ReplyToMessage != null && message.ReplyToMessage.Text == "Enter possible cities for staying the night (separated by commas).")
            {
                string[] cities = message.Text.Split(',');

                var fastestRoute = new List<FastestRouteDetails>();
                try
                {
                    Debug.WriteLine("HandleUserInput startPoint:\n" + _startPoint);
                    Debug.WriteLine("HandleUserInput destination:\n" + _endPoint);
                    Debug.WriteLine("HandleUserInput cities:\n" + string.Join(",", cities));
                    fastestRoute = GetFastestRoute(message.Chat.Id, _startPoint, _endPoint, cities).Result;

                }
                catch (AggregateException err)
                {
                    foreach (var errInner in err.InnerExceptions)
                    {
                        Debug.WriteLine(errInner);
                    }
                }

                if (fastestRoute != null)
                {
                    string responseMessage = "Fastest route is:\n" + string.Join("\n", fastestRoute.Select(x => "Route: " + x.CitySequence + " Dur: " + x.Duration));

                    await botClient.SendTextMessageAsync(message.Chat.Id, responseMessage, replyToMessageId: message.MessageId);
                }
                else
                {
                    await botClient.SendTextMessageAsync(message.Chat.Id, "No route found. Please try again with different inputs.", replyToMessageId: message.MessageId);
                }
            }
        }

        private static async Task<List<FastestRouteDetails>> GetFastestRoute(long chatId, string startPoint, string endPoint, string[] cities)
        {
            Debug.WriteLine("GetFastestRoute startPoint:\n" + startPoint);
            Debug.WriteLine("GetFastestRoute endPoint:\n" + endPoint);

            var fastestDuration = double.MaxValue;
            var fastestRoute = new List<FastestRouteDetails>();

            foreach (var posCity in cities)
            {
                var durAtoC = GetDirectionsDuration(startPoint, posCity, _keyGoogleApi).Result;
                var durCtoB = GetDirectionsDuration(posCity, endPoint, _keyGoogleApi).Result;
                var duration = durAtoC + durCtoB;
                if (duration < fastestDuration)
                {
                    fastestDuration = duration;
                    var fastestRouteList = new List<FastestRouteDetails>()
                    {
                        new FastestRouteDetails(durAtoC, startPoint + " - " + posCity),
                        new FastestRouteDetails(durCtoB, posCity + " - " + endPoint)
                    };
                    fastestRoute = fastestRouteList;
                }
            }

            return fastestRoute;
        }
        public static async Task<long> GetDirectionsDuration(string origin, string destination, string apiKey)
        {
            using (var httpClient = new HttpClient())
            {
                var apiUrl = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin}&destination={destination}&key={apiKey}";

                var response = await httpClient.GetAsync(apiUrl);
                var result = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("result:\n" + result);

                var resp = JsonConvert.DeserializeObject<DirectionResponse>(result);
                Debug.WriteLine("resp duration:\n" + resp.Routes[0].Legs[0].Duration.Value);

                var duration = resp.Routes[0].Legs[0].Duration.Value;

                return duration;
            }
        }
    }
    public class FastestRouteDetails
    {
        public FastestRouteDetails(long dur, string citySequence)
        {
            var timeSpan = TimeSpan.FromSeconds(dur);
            var timeInNormalTimeFormat = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                       timeSpan.Hours,
                       timeSpan.Minutes,
                       timeSpan.Seconds);
            Duration = timeInNormalTimeFormat;
            CitySequence = citySequence;
        }
        public string Duration { get; set; }
        public string CitySequence { get; set; }
    }
}
