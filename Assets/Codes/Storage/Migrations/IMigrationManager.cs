using Cysharp.Threading.Tasks;

namespace BanGround.Database.Migrations
{
    public interface IMigrationManager: ITaskWithProgress
    {
        int CurrentMigrationIndex { get; }
        int TotalMigrations { get; }
        string Description { get; }

        bool Init();
        UniTask<bool> Migrate();
    }
}
