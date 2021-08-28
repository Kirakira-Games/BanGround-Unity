using UnityEngine;
using Zenject;

public class SelectInstaller : MonoInstaller
{
    public GameObject cellPrefab;
    public Transform cellParent;

    public SelectManager selectManager;
    public SettingPanel settingPanel;
    public FancyBackground fancyBackground;

    public override void InstallBindings()
    {
        Container.BindFactory<KiraSongCell, KiraSongCell.Factory>().FromComponentInNewPrefab(cellPrefab).UnderTransform(cellParent);

        Container.Bind<SelectManager>().FromInstance(selectManager);
        Container.Bind<SettingPanel>().FromInstance(settingPanel);
        Container.Bind<FancyBackground>().FromInstance(fancyBackground);
    }
}
