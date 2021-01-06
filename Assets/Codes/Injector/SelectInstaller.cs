using UnityEngine;
using Zenject;

public class SelectInstaller : MonoInstaller
{
    public GameObject cellPrefab;
    public Transform cellParent;

    public override void InstallBindings()
    {
        Container.BindFactory<KiraSongCell, KiraSongCell.Factory>().FromComponentInNewPrefab(cellPrefab).UnderTransform(cellParent);
    }
}
