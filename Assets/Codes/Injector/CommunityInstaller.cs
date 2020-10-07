using BanGround.Community;
using UnityEngine;
using Zenject;

public class CommunityInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IStoreProvider>().WithId("BanGround").To<BanGroundStoreProvider>().AsSingle().NonLazy();
    }
}
