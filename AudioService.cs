using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

public class AudioService
{
    private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

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
            //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.").ConfigureAwait(false);
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        IAudioClient client;
        if (ConnectedChannels.TryRemove(guild.Id, out client))
        {
            await client.DisconnectAsync();
            //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.").ConfigureAwait(false);
        }
    }
    
    public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
    {
        if (!File.Exists(path))
        {
            await channel.SendMessageAsync("File does not exist.");
            return;
        }
        IAudioClient client;
        if (ConnectedChannels.TryGetValue(guild.Id, out client))
        {
            //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}").ConfigureAwait(false);
            var output = CreateStream(path).StandardOutput.BaseStream;
            var stream = client.CreatePCMStream(1920);
            await output.CopyToAsync(stream);
            await stream.FlushAsync().ConfigureAwait(false);
        }
    }

    private Process CreateStream(string path)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg.exe",
            Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true
        });
    }
}