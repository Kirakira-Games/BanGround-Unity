﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace BGEditor
{
    public class ObjectPool
    {
        private Dictionary<Type, LinkedList<GameObject>> pool;
        private Dictionary<GameObject, Type> typeLookup;
        private HashSet<GameObject> recycledThisFrame;

        private ChartCore Core;

        public ObjectPool()
        {
            pool = new Dictionary<Type, LinkedList<GameObject>>();
            Core = ChartCore.Instance;
            typeLookup = new Dictionary<GameObject, Type>();
            recycledThisFrame = new HashSet<GameObject>();
        }

        public GameObject Create<T>() where T: MonoBehaviour
        {
            var type = typeof(T);
            if (!pool.ContainsKey(type))
                pool.Add(type, new LinkedList<GameObject>());
            var list = pool[type];
            if (list.Count == 0)
            {
                GameObject obj;
                if (type.Equals(typeof(EditorSingleNote)))
                {
                    obj = GameObject.Instantiate(Core.SingleNote, Core.grid.transform);
                }
                else if (type.Equals(typeof(EditorFlickNote)))
                {
                    obj = GameObject.Instantiate(Core.FlickNote, Core.grid.transform);
                }
                else if (type.Equals(typeof(EditorSlideNote)))
                {
                    obj = GameObject.Instantiate(Core.SlideNote, Core.grid.transform);
                }
                else
                {
                    obj = new GameObject(type.Name);
                    obj.AddComponent<T>();
                }
                typeLookup[obj] = typeof(T);
                list.AddLast(obj);
            }
            var ret = list.Last.Value;
            list.RemoveLast();
            if (recycledThisFrame.Contains(ret))
                recycledThisFrame.Remove(ret);
            if (!ret.activeSelf)
                ret.SetActive(true);
            return ret;
        }

        public void Destroy(GameObject obj)
        {
            Destroy(typeLookup[obj], obj);
        }

        public void Destroy(Type type, GameObject obj)
        {
            if (!pool.ContainsKey(type))
                pool.Add(type, new LinkedList<GameObject>());
            var list = pool[type];
            list.AddLast(obj.gameObject);

            recycledThisFrame.Add(obj);
        }

        public void PostUpdate()
        {
            foreach (var obj in recycledThisFrame)
                obj.SetActive(false);
            recycledThisFrame.Clear();
        }
    }
}
