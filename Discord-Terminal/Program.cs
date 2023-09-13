using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;

class Program
{
    private static ulong channelId = 0; // Initialize channelId to 0
    private static ulong voiceChannelId = 0; // Initialize voiceChannelId to 0
    private static IVoiceChannel voiceChannel = null;

    private static async Task Main(string[] args)
    {
        // Ask the user for the bot token
        Console.Title = "Made with ❤️ by apfelteesaft";
        Console.Write("Enter your bot's token: ");
        string token = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(token))
        {
            Console.WriteLine("Invalid token. Please provide a valid bot token.");
            return;
        }

        // Create a new DiscordSocketClient
        var client = new DiscordSocketClient();

        // Log messages to the console
        client.Log += LogAsync;

        // Log in as the bot using the provided token
        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // Wait for the bot to be ready before sending messages
        client.Ready += async () =>
        {
            Console.WriteLine("Bot is connected and ready!");

            // Ask the user for the initial channel ID
            while (true)
            {
                Console.Write("Enter the initial text channel ID where you want to send messages: ");
                if (ulong.TryParse(Console.ReadLine(), out ulong inputChannelId))
                {
                    channelId = inputChannelId;
                    break;
                }
                else
                {
                    Console.WriteLine("Invalid channel ID. Please enter a valid ID.");
                }
            }

            var textChannel = client.GetChannel(channelId) as ITextChannel;

            if (textChannel == null)
            {
                Console.WriteLine($"Text channel with ID {channelId} not found.");
                return;
            }

            // Ensure that the bot has fetched the member list before proceeding

            Console.WriteLine($"\n\nType 'help' to display all the commands\n\nEnter messages to send to #{textChannel.Name} (type 'exit' to quit):");

            while (true)
            {
                string message = Console.ReadLine();

                if (message.ToLower() == "exit")
                {
                    break;
                }
                else if (message.ToLower() == "help")
                {
                    Console.WriteLine("commands:\n\nchannelid: Change the current channel where the bot is sending messages\n\nvcid: Select a voice channel for the bot to join\n\ndisc: Let the bot leave the VC"); // Respond with "hello"
                }
                else if (message.ToLower() == "channelid")
                {
                    // Ask the user for a new text channel ID
                    Console.Write("Enter the new text channel ID: ");
                    while (true)
                    {
                        if (ulong.TryParse(Console.ReadLine(), out ulong newChannelId))
                        {
                            channelId = newChannelId;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Invalid channel ID. Please enter a valid ID.");
                        }
                    }
                    textChannel = client.GetChannel(channelId) as ITextChannel;
                    if (textChannel == null)
                    {
                        Console.WriteLine($"Text channel with ID {channelId} not found.");
                        return;
                    }
                    Console.WriteLine($"Now sending messages to #{textChannel.Name}.");
                }
                else if (message.ToLower() == "vcid")
                {
                    // Ask the user for a voice channel ID
                    Console.Write("Enter the voice channel ID to join: ");
                    while (true)
                    {
                        if (ulong.TryParse(Console.ReadLine(), out ulong newVoiceChannelId))
                        {
                            voiceChannelId = newVoiceChannelId;
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Invalid voice channel ID. Please enter a valid ID.");
                        }
                    }

                    var guild = textChannel.Guild as SocketGuild;

                    if (guild == null)
                    {
                        Console.WriteLine("Failed to retrieve guild information.");
                        return;
                    }

                    voiceChannel = guild.GetVoiceChannel(voiceChannelId);

                    if (voiceChannel == null)
                    {
                        Console.WriteLine($"Voice channel with ID {voiceChannelId} not found.");
                    }
                    else
                    {
                        // Join the voice channel
                        await JoinVoiceChannelAsync(client, voiceChannel);
                        Console.WriteLine($"Joined voice channel: {voiceChannel.Name}");
                    }
                }
                else if (message.ToLower() == "disc")
                {
                    // Check if the bot is in a voice channel and leave if so
                    if (voiceChannel != null)
                    {
                        await LeaveVoiceChannelAsync(client, voiceChannel);
                        Console.WriteLine($"Left voice channel: {voiceChannel.Name}");
                        voiceChannel = null; // Reset voiceChannel
                    }
                    else
                    {
                        Console.WriteLine("Bot is not in a voice channel.");
                    }
                }
                else
                {
                    await textChannel.SendMessageAsync(message); // Send the message directly to the text channel
                    Console.WriteLine($"Sent: {message}");
                }
            }
        };

        await Task.Delay(-1); // Keep the application running indefinitely
    }

    private static async Task JoinVoiceChannelAsync(BaseSocketClient client, IVoiceChannel voiceChannel)
    {
        if (voiceChannel != null)
        {
            await (voiceChannel as IAudioChannel).ConnectAsync();
        }
    }

    private static async Task LeaveVoiceChannelAsync(BaseSocketClient client, IVoiceChannel voiceChannel)
    {
        if (voiceChannel != null)
        {
            await (voiceChannel as IAudioChannel).DisconnectAsync();
        }
    }

    private static Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }
}
