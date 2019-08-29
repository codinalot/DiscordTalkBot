using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using Discord.Audio;

// You *MUST* mark these commands with 'RunMode.Async'
// otherwise the bot will not respond until the Task times out.
public class AudioModule : ModuleBase<ICommandContext>
{
    private readonly AudioService _service;

    // Remember to add an instance of the AudioService
    // to your IServiceCollection when you initialize your bot
    public AudioModule(AudioService service)
    {
        _service = service;
    }

    [Command("joke", RunMode = RunMode.Async)]
    [Summary("Will have the bot enter your channel and vocalize lines from this file.")]
    public async Task JokeAsync()
    {
        //Put your joke file here
        var jokefile = File.ReadAllLines("C:\\bot\\sunny.txt");
        var jokes = new List<string>(jokefile);

        //It would definitely be better to not re-initialize Random() every time
        Random rnd = new Random();
        int r = rnd.Next(jokes.Count);
        string s;

        s = jokes[r];

        await SpeakCmd(s);
    }

    // This command joins whatever channel the user is currently in
    [Command("join", RunMode = RunMode.Async)]
    public async Task JoinCmd()
    {
        await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
    }

    // This command is what makes this bot pretty cool.
    // (1): Creates a text file (tmp) and writes the words to speak into the file
    // (2): Runs a command prompt where it uses Jampal text to speech command line program to create a wav file
    // (3): This wav file is played as audio by the bot; voila, your bot speaks!
    [Command("speak", RunMode = RunMode.Async)]
    public async Task SpeakCmd([Remainder] string words)
    {
        await JoinCmd();
        using (FileStream fs = File.Create("C:\\music\\tmp.txt"))
        {
            fs.Write(new UTF8Encoding(true).GetBytes(words), 0, words.Length);
        }

        var proc = Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c cscript C:\\bin\\ptts.vbs -w C:\\music\\tmp.wav < C:\\music\\tmp.txt",
            UseShellExecute = false,
            RedirectStandardOutput = false
        });

        proc.WaitForExit();
        await _service.SendAudioAsync(Context.Guild, Context.Channel, "tmp");
        await LeaveCmd();
    }

    // Leaves the channel the bot is currently in
    [Command("leave", RunMode = RunMode.Async)]
    public async Task LeaveCmd()
    {
        await _service.LeaveAudio(Context.Guild);
    }

    // This command will play a song from your C:\\music directory (currently, you can change the directory).
    [Command("play", RunMode = RunMode.Async)]
    public async Task PlayCmd([Remainder] string song)
    {
        await JoinCmd();
        await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
        await LeaveCmd();
    }
}

// AudioService handles core processing for doing audio based things within Discord
public class AudioService
{
    private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

    // This code has the bot join an audio channel.
    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        IAudioClient client;
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            return;
        }
        if (target.Guild.Id != guild.Id)
        {
            return;
        }

        var audioClient = await target.ConnectAsync();

        if (ConnectedChannels.TryAdd(guild.Id, audioClient))
        {
            // If you add a method to log happenings from this service,
            // you can uncomment these commented lines to make use of that.
            //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        IAudioClient client;
        if (ConnectedChannels.TryRemove(guild.Id, out client))
        {
            await client.StopAsync();
            //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
        }
    }

    // Check C:\music\ for audio files to play
    //      -(Only checks for mp3 & wav)
    public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
    {
        // Your task: Get a full path to the file if the value of 'path' is only a filename.
        string music_path = "C:\\music\\";
        string total_path = music_path + path + ".wav";
        string mp_path;

        // Found the file?
        if (!File.Exists(total_path))
        {
            mp_path = music_path + path + ".mp3";
            if (!File.Exists(mp_path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            else
            {
                total_path = mp_path;
            }
        }
        
        // Gets the current client channel, creates an audio stream, and copies the audio data to the stream.
        IAudioClient client;
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {

            //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
            using (var output = CreateStream(total_path).StandardOutput.BaseStream)
            using (var stream = client.CreatePCMStream(AudioApplication.Music))
            {
                try {
                    System.Diagnostics.Debug.WriteLine("Trying CopyToAsync on stream...");
                    await output.CopyToAsync(stream);
                }
                finally {
                    System.Diagnostics.Debug.WriteLine("completed CopyToAsync...");
                    await stream.FlushAsync();
                    System.Diagnostics.Debug.WriteLine("Flushing async..");
                }
            }
        }
    }

    // Uses ffmpeg to create an audio data stream
    private Process CreateStream(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            Arguments = $"-hide_banner -loglevel debug -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }
}