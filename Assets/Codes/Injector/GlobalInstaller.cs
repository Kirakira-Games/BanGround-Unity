using UnityEngine;
using Zenject;

public class GlobalInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Data Loader
        Container.Bind<IDataLoader>().To<DataLoader>().AsSingle().OnInstantiated((context, obj) =>
        {
            if (obj is ValidationMarker) return;
            var dataLoader = obj as IDataLoader;
            dataLoader.InitFileSystem();
            DataLoader.Instance = dataLoader; // TODO: Remove
            new GameObject("AppPreloader").AddComponent<AppPreLoader>();
        }).NonLazy();
        // Chart version
        Container.Bind<IChartVersion>().To<ChartVersion>().AsSingle().OnInstantiated((contxet, obj) =>
        {
            if (obj is ValidationMarker) return;
            ChartVersion.Instance = obj as IChartVersion; // TODO: Remove
        }).NonLazy();
        // Audio Manager
        Container.Bind<IAudioManager>().To<AudioManager>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
    }
}