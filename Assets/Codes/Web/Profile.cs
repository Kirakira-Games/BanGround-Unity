using System.Collections.Generic;

namespace BanGround.Web.Profile
{
    using RemoteKVarData = Dictionary<string, object>;

    public static class Extension
    {
        public static KiraWebRequest.Builder<RemoteKVarData> GetRemoteKVars(this IKiraWebRequest web)
        {
            return web.New<RemoteKVarData>().Get("profile/unity_vars");
        }
    }
}
