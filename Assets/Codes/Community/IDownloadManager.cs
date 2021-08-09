using System.Collections.Generic;

namespace BanGround.Community
{
    public interface IDownloadManager
    {
        DownloadTaskEvent onAddTask { get; }
        List<IDownloadTask> Tasks { get; }

        bool AddTask(IDownloadTask task);
        void CancelAll();
    }
}
