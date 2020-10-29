using BanGround.Community;
using UnityEngine;
using Zenject;

public class CommunityInstaller : MonoInstaller
{
    public StoreController storeController;

    public override void InstallBindings()
    {
        Container.Bind<IStoreProvider>().WithId("BanGround").To<BanGroundStoreProvider>().AsSingle().NonLazy();
        Container.Bind<IStoreController>().FromInstance(storeController);
    }
}
