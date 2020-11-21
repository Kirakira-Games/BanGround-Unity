namespace BanGround.Web
{
    public interface IKiraWebRequest
    {
        string AccessToken { get; set; }
        string Language { get; }
        string RefreshToken { get; set; }
        string ServerAddr { get; }
        string ServerSite { get; }
        string UA { get; }

        KiraWebRequest.Builder<ResponseType> New<ResponseType>();
        KiraWebRequest.Builder<object> New();
    }
}