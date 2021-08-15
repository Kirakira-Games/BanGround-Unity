using UnityEngine;
using Zenject;

public class SelectInstaller : MonoInstaller
{
    public GameObject cellPrefab;
    public Transform cellParent;

    public SelectManager selectManager;
    public SettingAndMod settingAndMod;
    public FancyBackground fancyBackground;
    public RankTable rankTable;

    public override void InstallBindings()
    {
        Container.BindFactory<KiraSongCell, KiraSongCell.Factory>().FromComponentInNewPrefab(cellPrefab).UnderTransform(cellParent);

        Container.Bind<SelectManager>().FromInstance(selectManager);
        Container.Bind<SettingAndMod>().FromInstance(settingAndMod);
        Container.Bind<FancyBackground>().FromInstance(fancyBackground);
        Container.Bind<RankTable>().FromInstance(rankTable);
    }
}
