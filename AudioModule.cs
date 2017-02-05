using Discord.Commands;

public class AudioModule : ModuleBase<ICommandContext>
{
    private readonly AudioService _service;

    public AudioModule(AudioService service)
    {
        _service = service;
    }

    // Remember to add more preconditions to your commands yourself.
    // This is merely the minimal amount necessary.
    [Command("join", RunMode = RunMode.Async)]
    public Task JoinCmd()
    {
        return _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
    }

    [Command("leave", RunMode = RunMode.Async)]
    public Task LeaveCmd()
    {
        return _service.LeaveAudio(Context.Guild);
    }
    
    [Command("play", RunMode = RunMode.Async)]
    public Task PlayCmd([Remainder] string song)
    {
        return _service.SendAudioAsync(Context.Guild, Context.Channel, song);
    }
}