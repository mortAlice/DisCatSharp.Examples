using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlusNextGen;
using DSharpPlusNextGen.Entities;
using DSharpPlusNextGen.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace SlashCommands.Commands
{
    public class RollRandom : SlashCommandModule
    {
        // By using an enum with the ChoiceName attribute, we can allow users to pick from a list without having to deal with arbiturary user input.
        public enum RandomChoice
        {
            [ChoiceName("Number")]
            Number,
            [ChoiceName("Role")]
            DiscordRole,
            [ChoiceName("User")]
            DiscordUser
        }

        [SlashCommand("roll_random", "Gets a random person, role or number.")]
        public async Task Command(InteractionContext context, [Option("random_choice", "Should a random number, role or user be picked?")] RandomChoice randomChoice = RandomChoice.Number)
        {
            // This is how you use dependency injection. We registered a Random instance over in Program.cs, now we're getting the same instance here.
            using IServiceScope scope = context.Services.CreateScope();
            Random random = scope.ServiceProvider.GetService<Random>();

            DiscordInteractionResponseBuilder discordInteractionResponseBuilder = new();
            if (randomChoice != RandomChoice.Number && context.Guild == null)
            {
                discordInteractionResponseBuilder.Content = $"Error: {randomChoice} cannot be used outside of a guild!";
                discordInteractionResponseBuilder.IsEphemeral = true;
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordInteractionResponseBuilder);
            }

            switch (randomChoice)
            {
                case RandomChoice.Number:
                    discordInteractionResponseBuilder.Content = random.Next(1, 101).ToString();
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordInteractionResponseBuilder);
                    break;
                case RandomChoice.DiscordRole:
                    int rolePosition = random.Next(context.Guild.Roles.Count + 1);
                    discordInteractionResponseBuilder.Content = context.Guild.Roles.Values.ElementAt(rolePosition).Mention;
                    // CHALLENGE: Make the role not be pinged when mentioned using the DiscordInteractionResponseBuilder.Mentions property
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordInteractionResponseBuilder);
                    break;
                case RandomChoice.DiscordUser:
                    // CHALLENGE: Make a guild member cache to prevent API abuse.
                    IReadOnlyCollection<DiscordMember> guildMembers = await context.Guild.GetAllMembersAsync();
                    int userPosition = random.Next(guildMembers.Count);
                    discordInteractionResponseBuilder.Content = guildMembers.ElementAt(userPosition).Mention;
                    // CHALLENGE: Make the user not be pinged when mentioned using the DiscordInteractionResponseBuilder.Mentions property
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordInteractionResponseBuilder);
                    break;
                // This shouldn't be reached, but it's here for error safety.
                default:
                    discordInteractionResponseBuilder.Content = "Error: Choice options are Number, Role or User! Please pick one of those.";
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, discordInteractionResponseBuilder);
                    break;
            }
        }
    }
}