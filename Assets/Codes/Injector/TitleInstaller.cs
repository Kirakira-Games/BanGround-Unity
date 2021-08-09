using BanGround.Database.Migrations;
using UnityEngine;
using Zenject;

public class TitleInstaller : MonoInstaller
{
    public TitleLoader titleLoader;

    public override void InstallBindings()
    {
        // Migration manager
        Container.Bind<IMigrationManager>().To<MigrationManager>().AsSingle().NonLazy();

        // Title Loader
        Container.Bind<TitleLoader>().FromInstance(titleLoader);
    }
}
