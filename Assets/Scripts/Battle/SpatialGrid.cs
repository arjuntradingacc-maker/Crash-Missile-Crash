using System.Collections.Generic;
using UnityEngine;

namespace CrashMissileCrash.Battle
{
    /// <summary>
    /// Uniform-bucket spatial partition over the battlefield's X/Z plane. Rebuilt once per combat
    /// tick from the currently active units (cheap for the hundreds/low-thousands scale this game
    /// targets) so neighbor queries used for targeting are O(nearby) instead of O(n) per unit.
    /// </summary>
    public class SpatialGrid<T> where T : class
    {
        private readonly float _cellSize;
        private readonly Dictionary<long, List<T>> _cells = new Dictionary<long, List<T>>();
        private readonly List<List<T>> _pool = new List<List<T>>();
        private int _poolCursor;

        public SpatialGrid(float cellSize)
        {
            _cellSize = Mathf.Max(0.5f, cellSize);
        }

        public void Clear()
        {
            _cells.Clear();
            _poolCursor = 0;
        }

        private long Key(float x, float z)
        {
            int cx = Mathf.FloorToInt(x / _cellSize);
            int cz = Mathf.FloorToInt(z / _cellSize);
            return ((long)cx << 32) ^ (uint)cz;
        }

        public void Insert(T item, Vector3 position)
        {
            long key = Key(position.x, position.z);
            if (!_cells.TryGetValue(key, out var list))
            {
                list = RentList();
                _cells[key] = list;
            }
            list.Add(item);
        }

        private List<T> RentList()
        {
            if (_poolCursor < _pool.Count)
            {
                var list = _pool[_poolCursor++];
                list.Clear();
                return list;
            }
            var newList = new List<T>(8);
            _pool.Add(newList);
            _poolCursor++;
            return newList;
        }

        /// <summary>Invokes <paramref name="visit"/> for every item whose cell is within radius of position.</summary>
        public void QueryRadius(Vector3 position, float radius, System.Action<T> visit)
        {
            int cellRadius = Mathf.CeilToInt(radius / _cellSize) + 1;
            int cx = Mathf.FloorToInt(position.x / _cellSize);
            int cz = Mathf.FloorToInt(position.z / _cellSize);

            for (int dx = -cellRadius; dx <= cellRadius; dx++)
            {
                for (int dz = -cellRadius; dz <= cellRadius; dz++)
                {
                    long key = ((long)(cx + dx) << 32) ^ (uint)(cz + dz);
                    if (_cells.TryGetValue(key, out var list))
                    {
                        for (int i = 0; i < list.Count; i++) visit(list[i]);
                    }
                }
            }
        }
    }
}
