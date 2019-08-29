using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.IO;


//This file serves as the entry point for the chatbot.
public class Program
{
    private CommandService _commands;
    private DiscordSocketClient _client;
    private IServiceProvider _services;
    private AudioService _audio;

    private static void Main(string[] args)
        => new Program().MainAsync().GetAwaiter().GetResult();

    public async Task MainAsync()
    {

        _client = new DiscordSocketClient();
        _commands = new CommandService();
        _audio = new AudioService();

        //Put your discord token in a file to be loaded here.
        StreamReader sr = new StreamReader("../../DiscordToken.txt");
        string token = sr.ReadLine();

        _client.Log += Log;

        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .AddSingleton(_audio)
            .BuildServiceProvider();

        await InstallCommandAsync();

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);

    }

    public async Task InstallCommandAsync()
    {
        _client.MessageReceived += HandleCommandAsync;

        await _commands.AddModulesAsync(Assembly.GetEntryAssembly());

    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a System Message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        // Create a number to track where the prefix ends and the command begins
        // Then, determine if the message is a command, based on if it starts with '!' or a mention prefix
        int argPos = 0;
        if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
        
        // Create a Command Context
        var context = new SocketCommandContext(_client, message);

        // Execute the command. (result does not indicate a return value, 
        // rather an object stating if the command executed successfully)
        var result = await _commands.ExecuteAsync(context, argPos, _services);
        if (!result.IsSuccess)
            await context.Channel.SendMessageAsync(result.ErrorReason);
    }

    private Task Log(LogMessage message)
    {
        Console.WriteLine(message.ToString());
        return Task.CompletedTask;
    }

}
