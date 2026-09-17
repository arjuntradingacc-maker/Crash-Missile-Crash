using System.Collections.Generic;
using UnityEngine;

namespace CrashMissileCrash.Core
{
    /// <summary>
    /// Generic component pool. Avoids per-unit Instantiate/Destroy calls so the crowd system
    /// can support hundreds/thousands of on-screen units without GC spikes or hitching.
    /// </summary>
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _inactive = new Stack<T>();
        private readonly HashSet<T> _active = new HashSet<T>();

        public int ActiveCount => _active.Count;
        public int TotalCount => _active.Count + _inactive.Count;

        public ObjectPool(T prefab, Transform parent, int prewarmCount = 0)
        {
            _prefab = prefab;
            _parent = parent;
            for (int i = 0; i < prewarmCount; i++)
            {
                var instance = CreateNew();
                instance.gameObject.SetActive(false);
                _inactive.Push(instance);
            }
        }

        private T CreateNew()
        {
            var instance = Object.Instantiate(_prefab, _parent);
            return instance;
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T instance = _inactive.Count > 0 ? _inactive.Pop() : CreateNew();
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            _active.Add(instance);
            return instance;
        }

        public void Release(T instance)
        {
            if (!_active.Remove(instance)) return;
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(_parent, false);
            _inactive.Push(instance);
        }

        public void ReleaseAll()
        {
            foreach (var instance in _active)
            {
                instance.gameObject.SetActive(false);
                _inactive.Push(instance);
            }
            _active.Clear();
        }

        public IEnumerable<T> ActiveItems => _active;
    }
}
