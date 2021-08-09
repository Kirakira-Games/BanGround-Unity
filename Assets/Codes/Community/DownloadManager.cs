using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace BanGround.Community
{
    public class DownloadTaskEvent : UnityEvent<IDownloadTask> { }
    public enum DownloadState
    {
        Stopped,
        Preparing,
        Downloading,
        Finished
    }
    public interface IDownloadTask : ITaskWithProgress
    {
        /// <summary>
        /// Image of the task. Nullable.
        /// </summary>
        Texture2D Image { get; }
        IDownloadTask SetImage(Texture2D texture);

        /// <summary>
        /// Unique key for each download task to avoid duplicates
        /// </summary>
        string Key { get; }

        /// <summary>
        /// User readable name of the download task.
        /// </summary>
        string Name { get; }
        IDownloadTask SetName(string name);

        /// <summary>
        /// Description for displaying to impatient user
        /// </summary>
        string Description { get; }

        DownloadState State { get; }

        UniTask Start();
        void Cancel();
        UnityEvent OnCancel { get; }
        UnityEvent OnFinish { get; }
    }
    public class DownloadManager : IDownloadManager
    {
        public List<IDownloadTask> Tasks { get; private set; } = new List<IDownloadTask>();
        public DownloadTaskEvent onAddTask { get; private set; } = new DownloadTaskEvent();

        public bool AddTask(IDownloadTask task)
        {
            if (Tasks.Find(t => t.Key == task.Key) != null)
                return false;
            Tasks.Add(task);
            task.OnFinish.AddListener(() => Tasks.Remove(task));
            task.OnCancel.AddListener(() => Tasks.Remove(task));
            task.Start();
            onAddTask.Invoke(task);
            return true;
        }

        public void CancelAll()
        {
            Tasks.ForEach(task => task.Cancel());
        }
    }
}
