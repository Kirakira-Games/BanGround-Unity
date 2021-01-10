using Cysharp.Threading.Tasks;

namespace BanGround.Database.Migrations
{
    public interface IMigrationManager
    {
        int CurrentMigrationIndex { get; }
        float CurrentMigrationProgress { get; }
        int TotalMigrations { get; }

        bool Init();
        UniTask<bool> Migrate();
    }
}