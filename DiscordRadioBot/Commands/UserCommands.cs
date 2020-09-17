using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;

namespace DiscordRadioBot.Commands
{
    public class UserCommands : BaseCommandModule
    {
        #region Comando !virar_membro
        [Command("virar_membro")]
        [Description("Comando utilizado para receber a Tag de membro!")]
        [RequireRoles(RoleCheckMode.None, "Server Admin", "Moderador", "BOTs", "Membro Antigo")]
        public async Task Virar_Membro(CommandContext context)
        {
            if(context.Channel.Id == BotClient.configJson.VirarMembroCanalID)
            {
                DiscordEmbedBuilder.EmbedThumbnail thumbnail = new DiscordEmbedBuilder.EmbedThumbnail();
                thumbnail.Url = context.Client.CurrentUser.AvatarUrl;

                DiscordEmbedBuilder joinEmbed = new DiscordEmbedBuilder
                {
                    Title = $"Você gostaria de se tornar membro do servidor: \"{context.Guild.Name}\"?",
                    Thumbnail = thumbnail,
                    Color = DiscordColor.Purple
                };

                DiscordMessage joinMessage = await context.Channel.SendMessageAsync(embed: joinEmbed).ConfigureAwait(false);

                DiscordEmoji reactionEmoji = DiscordEmoji.FromName(context.Client, BotClient.configJson.ReactionEmoji);
                await joinMessage.CreateReactionAsync(reactionEmoji).ConfigureAwait(true);

                InteractivityExtension interactivity = context.Client.GetInteractivity();

                InteractivityResult<MessageReactionAddEventArgs> reactionResult = await interactivity.WaitForReactionAsync(
                    x => x.Message == joinMessage &&
                    x.User == context.User &&
                    (x.Emoji == reactionEmoji)).ConfigureAwait(false);

                if(reactionResult.Result.Emoji == reactionEmoji)
                {
                    await context.Member.GrantRoleAsync(context.Guild.GetRole(BotClient.configJson.TagMembroId)).ConfigureAwait(false);
                }

                IReadOnlyList<DiscordMessage> channelMessages = await context.Channel.GetMessagesAsync(20).ConfigureAwait(false);

                List<DiscordMessage> messagesToDelete = new List<DiscordMessage>();
                foreach(DiscordMessage message in channelMessages)
                {
                    if(message.Id != BotClient.configJson.VirarMembroMensagemId)
                    {
                        messagesToDelete.Add(message);
                    }
                }
                await context.Channel.DeleteMessagesAsync(messagesToDelete).ConfigureAwait(false);
            }
            else
            {
                await context.Channel.SendMessageAsync($"ERR0! Este comando não pode ser usado fora do canal {context.Guild.GetChannel(BotClient.configJson.VirarMembroCanalID).Name}!").ConfigureAwait(false);
            }
        }
        #endregion
    }
}