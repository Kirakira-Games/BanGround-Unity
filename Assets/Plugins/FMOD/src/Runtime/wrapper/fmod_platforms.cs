namespace FMOD
{
    public partial class VERSION
    {
#if DEVELOPMENT_BUILD
        public const string dllSuffix = "L";
#else
        public const string dllSuffix = "";
#endif
    }
}

namespace FMOD.Studio
{
    public partial class STUDIO_VERSION
    {
#if DEVELOPMENT_BUILD
        public const string dllSuffix = "L";
#else
        public const string dllSuffix = "";
#endif
    }
}

#if UNITY_ANDROID && !UNITY_EDITOR
namespace FMOD
{
    public partial class VERSION
    {
        public const string dll = "fmod" + dllSuffix;
    }
}

namespace FMOD.Studio
{
    public partial class STUDIO_VERSION
    {
        public const string dll = "fmodstudio" + dllSuffix;
    }
}
#endif

#if UNITY_IPHONE && !UNITY_EDITOR
namespace FMOD
{
    public partial class VERSION
    {
        public const string dll = "__Internal";
    }
}

namespace FMOD.Studio
{
    public partial class STUDIO_VERSION
    {
        public const string dll = "__Internal";
    }
}
#endif

#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
namespace FMOD
{
    public partial class VERSION
    {
        public const string dll = "fmodstudio" + dllSuffix;
    }
}

namespace FMOD.Studio
{
    public partial class STUDIO_VERSION
    {
        public const string dll = "fmodstudio" + dllSuffix;
    }
}
#endif

#if !UNITY_EDITOR
namespace FMOD
{
    public partial class VERSION
    {
#if UNITY_STANDALONE_WIN
        public const string dll = "fmodstudio" + dllSuffix;
#elif UNITY_WSA
        public const string dll = "fmod" + dllSuffix;
#endif
    }
}

namespace FMOD.Studio
{
    public partial class STUDIO_VERSION
    {
#if UNITY_STANDALONE_WIN || UNITY_WSA
        public const string dll = "fmodstudio" + dllSuffix;
#endif
    }
}
#endif

#if UNITY_EDITOR
namespace FMOD
{
    public partial class VERSION
    {
        public const string dll = "fmodstudioL";
    }
}

namespace FMOD.Studio
{
    public partial class STUDIO_VERSION
    {
        public const string dll = "fmodstudioL";
    }
}
#endif