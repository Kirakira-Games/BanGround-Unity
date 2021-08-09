public interface IMessageBannerController
{
    void ShowMsg(LogLevel level, string content, bool autoClose = true);
}
