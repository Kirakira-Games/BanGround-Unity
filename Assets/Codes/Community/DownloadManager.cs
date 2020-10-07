using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;

namespace BanGround.Community
{
    public enum DownloadState
    {
        Stopped,
        Preparing,
        Downloading,
        Finished
    }
    public interface IDownloadTask
    {
        /// <summary>
        /// Unique key for each download task to avoid duplicates
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Progress (between 0 and 1)
        /// </summary>
        float Progress { get; }

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
    public class DownloadManager
    {
        public List<IDownloadTask> Tasks { get; private set; } = new List<IDownloadTask>();

        public bool AddTask(IDownloadTask task)
        {
            if (Tasks.Find(t => t.Key == task.Key) != null)
                return false;
            Tasks.Add(task);
            task.OnFinish.AddListener(() => Tasks.Remove(task));
            task.OnCancel.AddListener(() => Tasks.Remove(task));
            return true;
        }

        public void CancelAll()
        {
            Tasks.ForEach(task => task.Cancel());
        }
    }
}