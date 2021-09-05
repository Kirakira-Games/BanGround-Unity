namespace BanGround.Web.Profile
{
    public class RemoteKVarData
    {
        public int rm_data_version;
        public string rm_ver_stable;
        public string rm_ver_min;
    }

    public static class Extension
    {
        public static KiraWebRequest.Builder<RemoteKVarData> GetRemoteKVars(this IKiraWebRequest web)
        {
            return web.New<RemoteKVarData>().Get("profile/unity_vars");
        }
    }
}
