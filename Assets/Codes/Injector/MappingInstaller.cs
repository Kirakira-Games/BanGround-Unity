using BGEditor;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MappingInstaller : MonoInstaller
{
    public ChartCore chartCore;
    public EditNoteController editNoteController;
    public AudioProgressController audioProgress;
    public Button blocker;
    public GridController gridController;

    [Inject]
    private IFactory<IEditorInfo> editorInfoFactory;

    public override void InstallBindings()
    {
        Container.Bind<IChartCore>().FromInstance(chartCore);
        Container.Bind<IEditNoteController>().FromInstance(editNoteController);
        Container.Bind<IAudioProgressController>().FromInstance(audioProgress);
        Container.Bind<IGridController>().FromInstance(gridController);
        Container.Bind<Button>().WithId("Blocker").FromInstance(blocker);
        Container.Bind<IEditorInfo>().FromInstance(editorInfoFactory.Create());
        Container.Bind<IObjectPool>().To<ObjectPool>().AsSingle().NonLazy();
    }
}
