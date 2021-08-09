using BanGround.Community;
using BanGround.Web;
using UnityEngine;
using Zenject;

public class CommunityInstaller : MonoInstaller
{
    public StoreController storeController;

    public override void InstallBindings()
    {
        // Store
        Container.Bind<IStoreProvider>().WithId("BanGround").To<BanGroundStoreProvider>().AsSingle().NonLazy();
        Container.Bind<IStoreController>().FromInstance(storeController);

        // Cache
        Container.Bind<IResourceDownloadCache<Texture2D>>().To<TextureDownloadCache>().AsSingle().NonLazy();
    }
}
