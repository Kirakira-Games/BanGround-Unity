using System;
using System.Collections.Generic;
using UnityEngine;

namespace BGEditor
{
    public class ObjectPool
    {
        private Dictionary<Type, LinkedList<GameObject>> pool;
        private Dictionary<GameObject, Type> typeLookup;

        public ObjectPool()
        {
            pool = new Dictionary<Type, LinkedList<GameObject>>();
            typeLookup = new Dictionary<GameObject, Type>();
        }

        public GameObject Create<T>() where T: MonoBehaviour
        {
            var type = typeof(T);
            if (!pool.ContainsKey(type))
                pool.Add(type, new LinkedList<GameObject>());
            var list = pool[type];
            if (list.Count == 0)
            {
                var obj = new GameObject(type.Name);
                obj.AddComponent<T>();
                typeLookup[obj] = obj.GetType();
                list.AddLast(obj);
            }
            var ret = list.Last.Value;
            list.RemoveLast();

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

            obj.gameObject.SetActive(false);
        }

        public void Destroy<T>(T obj) where T: MonoBehaviour
        {
            Destroy(typeof(T), obj.gameObject);
        }
    }
}
