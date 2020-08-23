using System;
using UnityEngine;

namespace BGEditor
{
    public interface IObjectPool
    {
        void Init(GameObject SingleNote, GameObject FlickNote, GameObject SlideNote, GameObject GridInfoText);
        GameObject Create<T>() where T : MonoBehaviour;
        void Destroy(GameObject obj);
        void Destroy(Type type, GameObject obj);
        void PostUpdate();
    }
}