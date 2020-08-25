namespace Web
{
    public interface IKiraWebRequest
    {
        string AccessToken { get; set; }
        string Language { get; }
        string RefreshToken { get; set; }
        string ServerAddr { get; }
        string UA { get; }

        KiraWebRequest.Builder New();
    }
}