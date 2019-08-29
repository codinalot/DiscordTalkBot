using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using RedditSharp;

//These are non-audio commands.
public class InfoModule : ModuleBase<SocketCommandContext>
{
    // This is just a text echo
    [Command("say")]
    [Summary("Echos a message.")]
    public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
    {
        await ReplyAsync(echo);
    }

    // This command grabs posts from reddit and drops them in chat.
    // Just list a subreddit, hot/new/rising, and the number of posts to grab
    [Command("reddit")]
    [Summary("grabs reddit posts")]
    public async Task RedditAsync(string sub, string type, int count)//[Remainder, Summary("sub")] string sub)
    {

        var reddit = new Reddit();
        var subreddit = reddit.GetSubreddit("/r/" + sub);

        if(sub == "all" || sub == "All")
        {

        }else if (type == "Hot" || type == "hot")
        {
            foreach (var page in subreddit.Hot.Take(count))
            {
                string reply = page.Title.ToString() + ": " + page.Url.ToString();
                await ReplyAsync(reply);
            }
        }else if(type == "new" || type == "New")
        {
            foreach (var page in subreddit.New.Take(count))
            {
                string reply = page.Title.ToString() + ": " + page.Url.ToString();
                await ReplyAsync(reply);
            }
        }else if(type == "Rising" || type == "rising")
        {
            foreach (var page in subreddit.Rising.Take(count))
            {
                string reply = page.Title.ToString() + ": " + page.Url.ToString();
                await ReplyAsync(reply);
            }
        }
    }

    // Displays usage
    [Command("help", RunMode = RunMode.Async)]
    [Summary("displays help")]
    public async Task HelpAsync()
    {
        string s = "Remove the parentheses:\n(!)reddit subreddit hot/new/rising #posts\n(!)say message\n(!)speak message for bot to say\n(!)clips\n";
        await ReplyAsync(s);
    }

    // This lists all of the available wavs for the bot to play via audio streaming
    [Command("clips")]
    [Summary("display all available clips")]
    public async Task ClipsAsync()
    {
        string[] files = Directory.GetFiles("C:\\music\\");
        string s = "";
        foreach (string tmp in files)
        {
            s += tmp +"\n";
        }
        await ReplyAsync(s);
    }

    // ~sample userinfo --> foxbot#0282
    // ~sample userinfo @Khionu --> Khionu#8708
    // ~sample userinfo Khionu#8708 --> Khionu#8708
    // ~sample userinfo Khionu --> Khionu#8708
    // ~sample userinfo 96642168176807936 --> Khionu#8708
    // ~sample whois 96642168176807936 --> Khionu#8708
    [Command("userinfo")]
    [Summary("Returns info about the current user, or the user parameter, if one passed.")]
    [Alias("user", "whois")]
    public async Task UserInfoAsync([Summary("The (optional) user to get info for")] SocketUser user = null)
    {
        var userInfo = user ?? Context.Client.CurrentUser;
        await ReplyAsync($"{userInfo.Username}#{userInfo.Discriminator}");
    }
}
