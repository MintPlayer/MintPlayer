namespace MintPlayer.Data.Options
{
    public class SmtpOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool UseTLS { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
