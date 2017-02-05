using System.Collections.Concurrent;
using Discord.Audio;

public class AudioService
{
    public ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels { get; } = new ConcurrentDictionary<ulong, IAudioClient>();

    public AudioService()
    {
    }

    public async Task JoinAudio(IGuild guild, IVoiceChannel target)
    {
        if (ConnectedChannels.TryGetValue(guild.Id, out var _))
        {
            return;
        }
        if (target.Guild.Id != guild.Id)
        {
            return;
        }

        var audioClient = await target.ConnectAsync().ConfigureAwait(false);

        if (ConnectedChannels.TryAdd(guild.Id, audioClient))
        {
            //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.").ConfigureAwait(false);
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (ConnectedChannels.TryRemove(guild.Id, out var client))
        {
            await client.DisconnectAsync().ConfigureAwait(false);
            //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.").ConfigureAwait(false);
        }
    }
}