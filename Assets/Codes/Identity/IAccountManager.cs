using BanGround.Web.Auth;
using Cysharp.Threading.Tasks;

namespace BanGround.Identity
{
    public interface IAccountManager
    {
        UserLite ActiveUser { get; }
        UserFull ActiveUserInfo { get; }
        bool isAuthing { get; }
        bool isOfflineMode { get; }
        bool isTokenSaved { get; }
        int LoginAttemptCount { get; }

        UniTask<bool> DoLogin();
        UniTask<bool> TryLogin();
        void GoOffline();
    }
}
