# DiscordTalkBot

This project is a discord bot that uses Jampal speech to text (http://jampal.sourceforge.net/ptts.html) to speak in your audio channel!

It will also play music (or any mp3 / wav file) that you've placed in C:\music. 

On top of this, it can also automatically retrieve posts from reddit (for the memes).

In order to use, you'll have to do a few things (but only a few).

(1) Follow the above link to download jampal text to speech (ptts.vbs file). 
    -Place this in your C:\bin
    
(2) Download libsodium.dll & libopus.dll and place both in the DiscordBot directory.

(3) Install ffmpeg (also place in C:\bin)

(4) Place your discord bot token in DiscordToken.txt.

(5) Download nuget.exe and run it on the packages.config to generate packages

Have fun!

-This is based on some boilerplate discord audio code from @Joe4evr
