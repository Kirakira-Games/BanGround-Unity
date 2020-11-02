using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System;

namespace BanGround.Community
{
    public abstract class DownloadTaskBase : IDownloadTask
    {
        public Texture2D Image { get; protected set; }
        public string Key { get; protected set; }
        public string Name { get; protected set; } = "Task";
        public virtual float Progress
        {
            get => throw new NotImplementedException();
            protected set => throw new NotImplementedException();
        }
        public virtual string Description
        {
            get => throw new NotImplementedException();
            protected set => throw new NotImplementedException();
        }
        public virtual DownloadState State
        {
            get => throw new NotImplementedException();
            protected set => throw new NotImplementedException();
        }
        public UnityEvent OnCancel { get; protected set; } = new UnityEvent();
        public UnityEvent OnFinish { get; protected set; } = new UnityEvent();

        public abstract UniTask Start();
        public abstract void Cancel();

        public IDownloadTask SetImage(Texture2D texture)
        {
            Image = texture;
            return this;
        }

        public IDownloadTask SetName(string name)
        {
            Name = name;
            return this;
        }
    }
}