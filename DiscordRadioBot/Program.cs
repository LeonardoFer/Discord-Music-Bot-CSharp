namespace DiscordRadioBot
{
    class Program
    {
        static void Main(string[] args)
        {
            BotClient discordCommandBot = new BotClient();
            discordCommandBot.RunAsync().GetAwaiter().GetResult();
        }
    }
}