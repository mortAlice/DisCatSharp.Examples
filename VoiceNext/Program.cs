﻿using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.EventArgs;
using DisCatSharp.VoiceNext;

using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;

namespace DisCatSharp.Examples.VoiceNext
{
    /// <summary>
    /// The program.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point. Initializes the bot.
        /// </summary>
        /// <param name="args">The args.</param>
        static async Task Main(string[] args)
        {
            // Logging! Let the user know that the bot started!
            Console.WriteLine("Starting bot...");
            
            // Create logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // CHALLENGE: Try making sure the token is provided! Hint: A Try/Catch block may be needed!
            DiscordConfiguration discordConfiguration = new()
            {
                // The token is recieved from the command line arguments (bad practice in production!)
                // Example: dotnet run <someBotTokenHere>
                // CHALLENGE: Make it read from a file, optionally from a json file using System.Text.Json
                // CHALLENGE #2: Try retriving the token from environment variables
                Token = args[0],
                LoggerFactory = new LoggerFactory().AddSerilog(Log.Logger)
            };

            DiscordClient discordClient = new(discordConfiguration);
            
            // This line is needed to enable support for voice channels in the bot.
            discordClient.UseVoiceNext();
            
            Console.WriteLine("Connecting to Discord...");
            await discordClient.ConnectAsync();
            
            // Use the default logger provided for easy reading
            discordClient.Logger.LogInformation($"Connection success! Logged in as {discordClient.CurrentUser.Username}#{discordClient.CurrentUser.Discriminator} ({discordClient.CurrentUser.Id})");
            
            // Let the user know that we're registering the commands.
            discordClient.Logger.LogInformation("Registering application commands...");

            // In order not to list all the commands when adding, you can create a list of all commands with this.
            Type appCommandModule = typeof(ApplicationCommandsModule);
            var commands = Assembly.GetExecutingAssembly().GetTypes().Where(t => appCommandModule.IsAssignableFrom(t) && !t.IsNested).ToList();

            var appCommandExt = discordClient.UseApplicationCommands();

            // Register event handlers
            appCommandExt.SlashCommandExecuted += Slash_SlashCommandExecuted;
            appCommandExt.SlashCommandErrored += Slash_SlashCommandErrored;

            foreach (var command in commands)
            {
                appCommandExt.RegisterCommands(command, 885510395295584289);
            }
            
            discordClient.Logger.LogInformation("Application commands registered successfully");

            // Listen for commands by putting this method to sleep and relying off of DiscordClient's event listeners
            await Task.Delay(-1);
        }
        
        /// <summary>
        /// Fires when the user uses the slash command.
        /// </summary>
        /// <param name="sender">Application commands ext.</param>
        /// <param name="e">Event arguments.</param>
        private static Task Slash_SlashCommandExecuted(ApplicationCommandsExtension sender, SlashCommandExecutedEventArgs e)
        {
            Log.Logger.Information($"Slash: {e.Context.CommandName}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Fires when an exception is thrown in the slash command.
        /// </summary>
        /// <param name="sender">Application commands ext.</param>
        /// <param name="e">Event arguments.</param>
        private static Task Slash_SlashCommandErrored(ApplicationCommandsExtension sender, SlashCommandErrorEventArgs e)
        {
            Log.Logger.Error($"Slash: {e.Exception.Message} | CN: {e.Context.CommandName} | IID: {e.Context.InteractionId}");
            return Task.CompletedTask;
        }
    }
}