using Newtonsoft.Json;

namespace DiscordRadioBot
{
    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]

        public string CommandPrefix { get; private set; }
        [JsonProperty("YoutubeApiKey")]
        public string YoutubeApiKey { get; private set; }
        [JsonProperty("EndpointHostname")]
        public string EndpointHostname { get; private set; }
        [JsonProperty("EndpointPort")]
        public int EndpointPort { get; private set; }
        [JsonProperty("LavalinkPassword")]
        public string LavalinkPassword { get; private set; }

        [JsonProperty("VirarMembroCanalId")]
        public ulong VirarMembroCanalID { get; private set; }
        [JsonProperty("ComandosBotCanalId")]
        public ulong ComandosBotCanalId { get; private set; }
        [JsonProperty("VirarMembroMensagemId")]
        public ulong VirarMembroMensagemId { get; private set; }
        [JsonProperty("TagMembroId")]
        public ulong TagMembroId { get; private set; }
        [JsonProperty("ServerId")]
        public ulong ServerId { get; private set; }

        [JsonProperty("ReactionEmoji")]
        public string ReactionEmoji { get; private set; }

        [JsonProperty("TrackMaxLengthInMinutes")]
        public int TrackMaxLengthInMinutes { get; private set; }
    }
}