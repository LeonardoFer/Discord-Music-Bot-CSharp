# Bot de Música para Discord:

<p align="center">
  <img width="389" height="296" src="https://i.gyazo.com/ccaa70f3de5e2d039653f9759b6d3daf.png">
</p>

 ## Usando o bot(Portugues):
 Para utilizar, abra o Lavalink e depois abra o app do Bot. Use o comando !entrar nome_canal e depois !tocar nome_musica. Você precisa estar em um canal de voz! Você precisa ter a Role com nome de "Server Admin" ou "Moderador" para dar comandos ao Bot! Em breve eu vou implementar uma forma de utilizar roles configuráveis no arquivo config.json.
 Bot de música para Discord utilizando [Lavalink](https://github.com/Frederikam/Lavalink), a [YouTube API](https://developers.google.com/youtube/v3), [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) e [C#](https://dotnet.microsoft.com). 
 ## Using the Bot(English):
 To use it first you have to open Lavalink and then the Bot app. Use the command !entrar channel_name and then !tocar song_name. You need to be in a voice channel! You have to add "Server Admin" or "Moderador" roles into your server to give commands to the Bot! Soon I will implement a way to use configurable roles in the config.json file.
  Music bot for Discord using [Lavalink](https://github.com/Frederikam/Lavalink), the [YouTube API](https://developers.google.com/youtube/v3), [DSharpPlus](https://github.com/DSharpPlus/DSharpPlus) and [C#](https://dotnet.microsoft.com).

 ## Funcionalidades(Portugues):
  1. Implementei um recurso de pesquisa personalizado para procurar vídeos no YouTube usando a API oficial para que o Bot possa obter apenas vídeos marcados como MusicVideo (a pesquisa do Lavalink não funciona muito bem para pesquisa automatizada).
  2. O bot procura automaticamente, usando uma lista de palavras, você pode editar ambos os arquivos nas pastas de Recursos para canções/palavras personalizadas, basta manter 1 por linha, por novas músicas quando sua lista de reprodução terminar para que possa sempre continuar tocando algo.
  3. Concede automaticamente uma role a novos usuários.
  4. Comando para excluir mensagens de 0 até 50.
  5. Comandos: !ping para pingar o bot, !deletar_mensagens 50 para deletar mensagens, !tocar song_name para reproduzir uma música, !proxima para pular uma música, !volume 0-100 para alterar o volume, !pause, !play para retomar, !entrar channel_name para fazê-lo entrar em uma sala de voz, !sair para sair da sala de voz atual, !lista para listar todas as músicas na playlist, !tocando para ver a música que está tocando.
  
  
 ## Features(English):
  1. I've implemented a custom search feature to look for videos on YouTube using the official API so I can get only videos tagged as MusicVideo(the Lavalink search doesn't really work for automated search).
  2. The bot automatically searches(using a list of words, you could edit both files in the Resource folders for custom songs/words, just keep it 1 per line) for new songs when its playlist ends so it can always keep playing something.
  3. Automatically grants a role to new users.
  4. Command to delete messages up to 50.
  5. Commands: !ping to ping the bot, !deletar_mensagens 50 to delete messages, !tocar song_name to play a song, !proxima to skip a song, !volume 0-100 to change the volume, !pause, !play to resume, !entrar channel_name to make it enter a voice room, !sair to leave the current voice room, !lista to list all songs in the playlist, !tocando to see the current song playing.
  
<p align="center">
  <img width="500" height="222" src="https://i.gyazo.com/bf0c93a556ce22804215688d05272fdc.gif">
</p>
  
# Manual de Configuração | Configuration Manual:
 [Configuration Guide (English/Portuguese)](https://github.com/LeonardoFer/Discord-Music-Bot-CSharp/commit/0e6613bba36fec7e42851cb89f3edb3e1568579c)
 
 
 
## Problemas Conhecidos(Portugues):
 1. Lavalink crashando quando você pede para ele tocar um vídeo do tipo Broadcast.
 2. Bot para de tocar quando algum erro acontece com o Lavalink.
## Known Issues(English):
 1. Lavalink crashing when you ask it to play a Broadcast video.
 2. Bot stops playing when Lavalink encounters a error.
