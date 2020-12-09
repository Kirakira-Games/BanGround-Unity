using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace BanGround.Community
{
    public class DownloadTaskGroup : DownloadTaskBase, IDownloadTask
    {
        public List<IDownloadTask> Tasks { get; private set; } = new List<IDownloadTask>();
        /// <summary>
        /// Current task index.
        /// </summary>
        public int TIndex { get; private set; } = 0;
        public IDownloadTask currentTask => TIndex >= Tasks.Count ? null : Tasks[TIndex];

        public override float Progress => Mathf.Min(1, (float)(TIndex + (currentTask?.Progress ?? 1f)) / Tasks.Count);
        public override string Description => currentTask?.Description ?? "Done.";
        public override DownloadState State => currentTask?.State ?? DownloadState.Finished;

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
                if (currentTask == null)
                    OnFinish.Invoke();
            });
            Tasks.Add(task);
        }

        public override void Cancel()
        {
            currentTask?.Cancel();
        }

        public override async UniTask Start()
        {
            while (currentTask != null)
            {
                await currentTask.Start();
            }
        }
    }
}