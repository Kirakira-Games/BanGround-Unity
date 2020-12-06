using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;
using BanGround.Database.Models;

namespace BanGround.Database
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void SetupMessagePackResolver()
        {
            StaticCompositeResolver.Instance.Register(new[]{
                MasterMemoryResolver.Instance, // set MasterMemory generated resolver
                GeneratedResolver.Instance,    // set MessagePack generated resolver
                StandardResolver.Instance      // set default MessagePack resolver
            });

            var options = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = options;
        }
    }
}