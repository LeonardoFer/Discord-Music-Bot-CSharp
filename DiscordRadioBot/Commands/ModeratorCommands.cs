#region Usings
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using DiscordRadioBot.Classes;
#endregion
namespace DiscordRadioBot.Commands
{   
    public class ModeratorCommands : BaseCommandModule
    {        
        public static LavalinkGuildConnection guildConnection { get; private set; }        
        #region Comando !ping
        [Command("ping")]
        [Description("Comando para verificar se o BOT está online.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Ping(CommandContext context)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                await context.Channel.SendMessageAsync($"Beep Boop! Não se preocupe {context.User.Username}! Eu estou online!").ConfigureAwait(false);
            }
        }
        #endregion
        #region Comando !deletar_mensagens
        [Command("deletar_mensagens")]
        [Description("Deleta o número especificado de mensagens no canal!")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Deletar_Mensagens(CommandContext context, [Description("Número de mensagens a deletar.")] int numberOfMessagesToClear)
        {
            if(context.Channel.Id != BotClient.configJson.VirarMembroCanalID)
            {
                if(numberOfMessagesToClear >= 0)
                {
                    if(numberOfMessagesToClear > 50)
                    {
                        await context.Guild.GetChannel(BotClient.configJson.ComandosBotCanalId).SendMessageAsync($"ERR0! {context.User.Username} o seu comando é inválido! Número precisa ser entre 1 e 50 mensagens!").ConfigureAwait(false);
                    }
                    else
                    {
                        IReadOnlyList<DiscordMessage> messagesToDelete = await context.Channel.GetMessagesAsync(numberOfMessagesToClear + 1).ConfigureAwait(false);
                        await context.Channel.DeleteMessagesAsync(messagesToDelete).ConfigureAwait(false);
                        await context.Guild.GetChannel(BotClient.configJson.ComandosBotCanalId).SendMessageAsync($"{context.User.Username} o seu desejo é uma ordem! Eu deletei as ultimas {messagesToDelete.Count} mensagens do canal {context.Channel.Name}").ConfigureAwait(false);
                    }
                }
                else
                {
                    await context.Message.RespondAsync($"ERR0! {context.User.Username} o seu comando é inválido! Número precisa maior que 0!").ConfigureAwait(false);
                }
            }
        }
        #endregion
        #region Comando !tocar
        [Command("tocar")]
        [Description("Comando para adicionar uma música à playlist.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Tocar(CommandContext context, [Description("Pesquisa que o bot fará no site externo.")][RemainingText] string searchQuery)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                guildConnection = BotClient.LavalinkNode.GetGuildConnection(await BotClient.Lavalink.Client.GetGuildAsync(BotClient.configJson.ServerId).ConfigureAwait(false));

                if(context.Member.VoiceState == null || context.Member.VoiceState.Channel == null)
                {
                    await context.RespondAsync($"{context.User.Username} você precisa estar conectado a um canal de voz!").ConfigureAwait(false);
                    return;
                }
                if(guildConnection == null)
                {
                    await context.RespondAsync($"ERR0! Lavalink não está conectado!").ConfigureAwait(false);
                    return;
                }
         
                YoutubeSearchEngine youtubeSearchEngine = new YoutubeSearchEngine(1);
                Dictionary<string, string> searchResult = youtubeSearchEngine.SearchVideos(searchQuery);
                if(searchResult.Count == 0)
                {
                    await context.RespondAsync($"ERR0! Pesquisa por {searchQuery} falhou! Não consegui encontrar nenhum vídeo no YouTube!").ConfigureAwait(false);
                    return;
                }
                string videoTitle = searchResult.ElementAt(0).Key.ToString();

                LavalinkLoadResult loadResult = await BotClient.LavalinkNode.Rest.GetTracksAsync(videoTitle).ConfigureAwait(false);
                if(loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
                {
                    await context.RespondAsync($"ERR0! Pesquisa por {searchQuery} falhou! Tipo do resultado: {loadResult.LoadResultType}").ConfigureAwait(false);
                }

                LavalinkTrack track = loadResult.Tracks.First();
                if(track.Length >= new TimeSpan(0, BotClient.configJson.TrackMaxLengthInMinutes, 0))
                {
                    await context.RespondAsync($"Duração da Música {track.Title} é maior que o maximo permitido(valor configurável)! Ignorando esta música...");
                    await context.RespondAsync($"Duração: {track.Length.Hours}:{track.Length.Minutes}:{track.Length.Seconds}.");
                    return;
                }

                if(BotClient.Playlist.Count == 0)
                {
                    BotClient.Playlist.Add(track);
                    await guildConnection.SetVolumeAsync(50).ConfigureAwait(false);
                    await guildConnection.PlayAsync(BotClient.Playlist[0]).ConfigureAwait(false);
                    await context.RespondAsync($"Tocando: {track.Title}! Volume: {50}%").ConfigureAwait(false);
                    BotClient.IsSongPlaying = true;
                }
                else
                {
                    BotClient.Playlist.Add(track);
                    await context.RespondAsync($"A música: {track.Title} foi adicionada à playlist de músicas para tocar!").ConfigureAwait(false);
                }
            }
        }
        #endregion
        #region Comando !proxima
        [Command("proxima")]
        [Description("Comando para pular para a proxima música da playlist.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Proxima(CommandContext context)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                guildConnection = BotClient.LavalinkNode.GetGuildConnection(await BotClient.Lavalink.Client.GetGuildAsync(BotClient.configJson.ServerId).ConfigureAwait(false));

                if(context.Member.VoiceState == null || context.Member.VoiceState.Channel == null)
                {
                    await context.RespondAsync($"{context.User.Username} você precisa estar conectado a um canal de voz!").ConfigureAwait(false);
                    return;
                }
                if(guildConnection == null)
                {
                    await context.RespondAsync($"{context.User.Username}, eu não estou conectado a nenhum canal de voz!").ConfigureAwait(false);
                    return;
                }

                if(BotClient.Playlist.Count == BotClient._currentTrackIndex + 1)
                {
                    await context.RespondAsync($"Não posso pular para a próxima música! Pois estou tocando a música de número {BotClient._currentTrackIndex + 1} e a Playlist só tem {BotClient.Playlist.Count}");
                }
                else
                {
                    await guildConnection.PlayAsync(BotClient.Playlist[BotClient._currentTrackIndex + 1]).ConfigureAwait(false);
                    await context.RespondAsync($"Ok {context.User.Username}! Vou pular para a próxima música!").ConfigureAwait(false);
                }
            }
        }
        #endregion
        #region Comando !volume
        [Command("volume")]
        [Description("Comando para mudar o volume do BOT.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Volume(CommandContext context, [Description("Volume que você quer que o BOT toque. Deve ser entre 0 e 100")] int volume)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                guildConnection = BotClient.LavalinkNode.GetGuildConnection(await BotClient.Lavalink.Client.GetGuildAsync(BotClient.configJson.ServerId).ConfigureAwait(false));

                if(volume > 100)
                {
                    volume = 100;
                }
                if(volume < 0)
                {
                    volume = 0;
                }

                await guildConnection.SetVolumeAsync(volume).ConfigureAwait(false);
                await context.RespondAsync($"Ok {context.User.Username}! Volume alterado para: {volume}!").ConfigureAwait(false);
            }
        }
        #endregion
        #region Comando !pause
        [Command("pause")]
        [Description("Comando para pausar a música.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Pause(CommandContext context)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                guildConnection = BotClient.LavalinkNode.GetGuildConnection(await BotClient.Lavalink.Client.GetGuildAsync(BotClient.configJson.ServerId).ConfigureAwait(false));

                if(BotClient.IsSongPlaying && guildConnection != null)
                {
                    await guildConnection.PauseAsync().ConfigureAwait(false);
                    await context.RespondAsync($"Ok {context.User.Username}! Música pausada!").ConfigureAwait(false);
                    BotClient.IsSongPlaying = false;
                }
                else
                {
                    await context.RespondAsync($"{context.User.Username} a música ja está pausada!").ConfigureAwait(false);
                }
            }
        }
        #endregion
        #region Comando !play
        [Command("play")]
        [Description("Comando para dar play na música.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Play(CommandContext context)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                guildConnection = BotClient.LavalinkNode.GetGuildConnection(await BotClient.Lavalink.Client.GetGuildAsync(BotClient.configJson.ServerId).ConfigureAwait(false));

                if(!BotClient.IsSongPlaying && guildConnection != null)
                {
                    await guildConnection.ResumeAsync().ConfigureAwait(false);
                    await context.RespondAsync($"Ok {context.User.Username}! Musica resumida!").ConfigureAwait(false);
                    BotClient.IsSongPlaying = true;
                }
                else
                {
                    await context.RespondAsync($"{context.User.Username} a música ja está tocando!").ConfigureAwait(false);
                }
            }
        }
        #endregion
        #region Comando !entrar
        [Command("entrar")]
        [Description("Comando para fazer o BOT entrar no canal de voz.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Entrar(CommandContext context, [Description("Canal que você quer que o BOT se conecte.")] DiscordChannel channel)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                if(channel.Type != ChannelType.Voice)
                {
                    await context.RespondAsync($"{context.User.Username} você precisa estar conectado a um canal de voz!").ConfigureAwait(false);
                    return;
                }

                await BotClient.LavalinkNode.ConnectAsync(channel);
                await context.RespondAsync($"Ok {context.User.Username}! Eu me conectei ao canal: #{channel.Name}!").ConfigureAwait(false);
            }
        }
        #endregion
        #region Comando !sair
        [Command("sair")]
        [Description("Comando para fazer com que o BOT saia da sala de voz.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Sair(CommandContext context, [Description("Canal que você quer que o BOT se desconecte.")] DiscordChannel channel)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                guildConnection = BotClient.LavalinkNode.GetGuildConnection(await BotClient.Lavalink.Client.GetGuildAsync(BotClient.configJson.ServerId).ConfigureAwait(false));
                BotClient.IsSongPlaying = false;

                if(guildConnection == null)
                {
                    await context.RespondAsync($"{context.User.Username} eu não estou conectado a nenhum canal de voz!").ConfigureAwait(false);
                    return;
                }

                await guildConnection.DisconnectAsync();
                await context.RespondAsync($"Ok {context.User.Username}! Eu me desconectei do canal: #{channel.Name}!").ConfigureAwait(false);
            }
        }
        #endregion
        #region Comando !lista
        [Command("lista")]
        [Description("Comando para fazer com que o BOT liste todas as músicas na playlist.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Lista(CommandContext context)
        {
            if(context.Channel.Id == BotClient.configJson.ComandosBotCanalId)
            {
                if(BotClient.Playlist.Count == 0)
                {
                    await context.RespondAsync($"A playlist está vazia! Use !tocar <nome da música> para adicionar uma música à playlist.");
                    return;
                }

                StringBuilder stringBuilder = new StringBuilder();
                for(int i = 0; i < BotClient.Playlist.Count; i++)
                {
                    stringBuilder.Append($"Música: {BotClient.Playlist[i].Title}. Posição: {i + 1}.\n");
                }

                DiscordEmbedBuilder listEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Playlist:",
                    Description = stringBuilder.ToString(),
                    Color = DiscordColor.SpringGreen
                };

                await context.Channel.SendMessageAsync(embed: listEmbed).ConfigureAwait(false);
            }
        }
        #endregion
        #region Comando !tocando
        [Command("tocando")]
        [Description("Comando para ver qual música o BOT está tocando.")]
        [RequireRoles(RoleCheckMode.Any, "Server Admin", "Moderador")]
        public async Task Tocando(CommandContext context)
        {
            if(BotClient.Playlist.Count > 0)
            {
                await context.RespondAsync($"A música que eu estou tocando agora é: {BotClient.Playlist[BotClient._currentTrackIndex].Title}.").ConfigureAwait(false);
            }
            else
            {
                await context.RespondAsync($"Eu não estou tocando nenhuma música no momento!").ConfigureAwait(false);
            }
        }
        #endregion
    }
}