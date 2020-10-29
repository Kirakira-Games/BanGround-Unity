using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace BanGround.Community
{
    public class DownloadTaskGroup : IDownloadTask
    {
        public List<IDownloadTask> Tasks { get; private set; } = new List<IDownloadTask>();
        /// <summary>
        /// Current task index.
        /// </summary>
        public int TIndex { get; private set; } = 0;
        public IDownloadTask currentTask => TIndex >= Tasks.Count ? null : Tasks[TIndex];

        public string Key { get; private set; }

        public float Progress => (float)(TIndex + (currentTask?.Progress ?? 1f)) / Tasks.Count;

        public string Description => currentTask?.Description ?? "Done.";

        public DownloadState State => currentTask?.State ?? DownloadState.Finished;

        public UnityEvent OnCancel { get; private set; } = new UnityEvent();

        public UnityEvent OnFinish { get; private set; } = new UnityEvent();

        public DownloadTaskGroup(string key)
        {
            Key = key;
        }

        public void AddTask(IDownloadTask task)
        {
            task.OnCancel.AddListener(() =>
            {
                TIndex++;
                if (currentTask == null)
                    OnCancel.Invoke();
                else
                    Cancel();
            });
            task.OnFinish.AddListener(() =>
            {
                TIndex++;
                var curTask = currentTask;
                if (curTask == null)
                    OnFinish.Invoke();
                else
                    curTask.Start();
            });
            Tasks.Add(task);
        }

        public void Cancel()
        {
            currentTask?.Cancel();
        }

        public async UniTask Start()
        {
            while (currentTask != null)
            {
                await currentTask.Start();
            }
        }
    }
}