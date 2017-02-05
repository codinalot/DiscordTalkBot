using Discord.Commands;

public class AudioModule : ModuleBase<ICommandContext>
{
    private readonly AudioService _service;

    public AudioModule(AudioService service)
    {
        _service = service;
    }

    [Command("join", RunMode = RunMode.Async)
    public Task ConnectCmd()
    {
        return _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
    }

    [Command("leave", RunMode = RunMode.Async)]
    public Task LeaveCmd()
    {
        return _service.LeaveAudio(Context.Guild);
    }
}