#region Usings
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using DSharpPlus;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using DSharpPlus.EventArgs;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;

using Newtonsoft.Json;

using DiscordRadioBot.Classes;
using DiscordRadioBot.Commands;
#endregion
namespace DiscordRadioBot
{
    public class BotClient
    {
        public static Random _randomNumber { get; private set; } = new Random();
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public static LavalinkExtension Lavalink { get; private set; }
        public static LavalinkNodeConnection LavalinkNode { get; private set; }
        public static List<LavalinkTrack> Playlist { get; private set; } = new List<LavalinkTrack>();
        public static int _currentTrackIndex { get; set; } = 0;
        public static bool IsSongPlaying { get; set; } = false;
        public static ConfigJson configJson { get; private set; }

        public async Task RunAsync()
        {
            #region Abrindo o arquivo de configuração do bot.
            string projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
            string configFilePath = $"{projectPath}{ConfigurationManager.AppSettings["ClientConfigJson"]}";
 
            string json = string.Empty;            

            FileStream fileStream = null;
            try
            {
                fileStream = File.OpenRead(configFilePath);                
                using(StreamReader streamReader = new StreamReader(fileStream, new UTF8Encoding(false)))
                {
                    fileStream = null;
                    json = await streamReader.ReadToEndAsync().ConfigureAwait(false);
                }                
            }
            finally
            {
                if(fileStream != null)
                {
                    fileStream.Dispose();
                }
            }
            configJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            #endregion
            #region Configurando o bot com o Token do arquivo de configuração.
            DiscordConfiguration config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };
            #endregion
            #region Instanciando o bot(_client) e configurando a extensão de interactividade.
            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2),
            });

            await Client.ConnectAsync();
            #endregion
            #region Inicializando o serviço de audio do _client
            ConnectionEndpoint endpoint = new ConnectionEndpoint
            {
                Hostname = configJson.EndpointHostname,
                Port = configJson.EndpointPort
            };
            LavalinkConfiguration lavalinkConfig = new LavalinkConfiguration
            {
                Password = configJson.LavalinkPassword,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint,
            };
            Lavalink = Client.UseLavalink();
            LavalinkNode = await Lavalink.ConnectAsync(lavalinkConfig);
            #endregion
            #region Configuração e registro dos comandos do bot.
            CommandsNextConfiguration commandsConfiguration = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { configJson.CommandPrefix },
                EnableDms = false,
                EnableMentionPrefix = true,
                DmHelp = false
            };
            Commands = Client.UseCommandsNext(commandsConfiguration);
            Commands.RegisterCommands<ModeratorCommands>();
            Commands.RegisterCommands<UserCommands>();
            #endregion

            LavalinkNode.PlaybackFinished += Lavalink_PlaybackFinished;

            await Task.Delay(-1);
        }
        private Task OnClientReady(ReadyEventArgs eventArgs)
        {        
            return Task.CompletedTask;
        }
        /// <summary>
        /// Executa quando uma música acaba ou quando um usuário utiliza o comando !proxima.
        /// </summary>
        /// <param name="TrackFinishEventArgs"></param>
        /// <returns></returns>
        private static async Task Lavalink_PlaybackFinished(EventArgs TrackFinishEventArgs)
        {
            LavalinkGuildConnection guildConnection = LavalinkNode.GetGuildConnection(await Lavalink.Client.GetGuildAsync(configJson.ServerId).ConfigureAwait(false));
            _currentTrackIndex++;
            if(_currentTrackIndex > Playlist.Count)
            {
                await guildConnection.PlayAsync(Playlist[_currentTrackIndex]).ConfigureAwait(false);    
            }

            if(_currentTrackIndex + 1 > Playlist.Count)
            {
                Playlist.Clear();
                _currentTrackIndex = 0;
                await guildConnection.Guild.GetChannel(configJson.ComandosBotCanalId).SendMessageAsync($"A playlist está vazia! Adicionando novas músicas!");
                await AddSongs(5);
                await guildConnection.PlayAsync(Playlist[_currentTrackIndex]).ConfigureAwait(false);
                IsSongPlaying = true;
            }
        }
        /// <summary>
        /// Adiciona novas músicas à playlist utilizando a API do YouTube para realizar a busca.
        /// </summary>
        /// <param name="numberOfTracksToGrab"> How many songs to search for. </param>
        /// <returns></returns>
        public static async Task AddSongs(short numberOfTracksToGrab)
        {
            LavalinkGuildConnection guildConnection = LavalinkNode.GetGuildConnection(await Lavalink.Client.GetGuildAsync(configJson.ServerId).ConfigureAwait(false));

            for(int i = 0; i < numberOfTracksToGrab; i++)
            {
                WordsGenerator _wordsGenerator = new WordsGenerator(_randomNumber);                
                List<string> wordList = _wordsGenerator.GenerateWordList();

                YoutubeSearchEngine _youtubeSearchEngine = new YoutubeSearchEngine(1);
                Dictionary<string, string> searchResult = _youtubeSearchEngine.SearchVideos(wordList);
                
                foreach(var result in searchResult)
                {
                    Uri videoUri = new Uri(result.Value);   
                    LavalinkLoadResult loadResult = await LavalinkNode.Rest.GetTracksAsync(videoUri).ConfigureAwait(false);

                    if(loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                    {
                        await guildConnection.Guild.GetChannel(configJson.ComandosBotCanalId).SendMessageAsync($"ERR0! Pesquisa por {searchResult} falhou! Tipo do resultado: {result.Key}").ConfigureAwait(false);
                    }
                    else
                    {
                        Playlist.Add(loadResult.Tracks.First());
                        await guildConnection.Guild.GetChannel(configJson.ComandosBotCanalId).SendMessageAsync($"A música: {loadResult.Tracks.First().Title} foi adicionada à playlist de músicas para tocar!").ConfigureAwait(false);
                    }                    
                }                
            }
        }
    }
}