using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sh4
{
    public class Graph<T> where T : class, new()
    {
#nullable enable
        private readonly Dictionary<int, List<int>> _adjIdxListDict = new();

        private readonly WeakKeyDictionary<T, int> _itemIdxDict = new();
        private readonly Dictionary<int, WeakKey<T>> _reverseDict = new();

        private readonly PriorityQueue<int, int> _emptyIdx = new();
        private int _edgeCount = 0;

        // 생성자
        public Graph()
        {
        }

        // 프로퍼티
        public List<T> Vertices
        {
            get
            {
                if (!_itemIdxDict.TryGetAllOriginalKeys(out List<T> vertices, out List<int>? missingIdx))
                {
                    foreach (int idx in missingIdx)
                    {
                        Debug.Log($"[Verify] : {idx}가 key를 잃었습니다.");
                        _adjIdxListDict.Remove(idx);

                        _reverseDict.Remove(idx);

                        _emptyIdx.Enqueue(idx, idx);
                    }
                }

                return vertices;
            }
        }

        public int VertexCount { get => _itemIdxDict.Count; }

        public int EdgeCount { get => _edgeCount; }

        // public 메서드
        public void Print()
        {
            string dictListStr = "[Dict]\n";
            string adjMatStr = $"[Adj Matrix] Edge의 수 : {_edgeCount}\n";

            foreach(var keyValue in _itemIdxDict)
            {
                dictListStr += "{[" + keyValue.Value + "], [" + keyValue.Key + "]}\n";
            }

            List<int> idxList = _adjIdxListDict.Keys.ToList();
            idxList.Sort();
            adjMatStr += $"{"0",-6}";
            idxList.ForEach(idx => adjMatStr += $"{idx,-6}");
            adjMatStr += '\n';

            for(int i = 0; i < idxList.Count; i++)
            {
                adjMatStr += $"{idxList[i], -6}";
                for (int j = 0; j < idxList.Count; j++)
                {
                    if (_adjIdxListDict[idxList[i]].Contains(j))
                    {
                        adjMatStr += $"{"1",-6}";
                    }
                    else
                    {
                        adjMatStr += $"{"0",-6}";
                    }
                }
                adjMatStr += '\n';
            }

            Debug.Log($"Dict: {dictListStr}");
            Debug.Log(adjMatStr);
        }

        public void Clear()
        {
            _adjIdxListDict.Clear();

            _itemIdxDict.Clear();
            _reverseDict.Clear();

            _emptyIdx.Clear();
            _edgeCount = 0;
        }

        public bool AddVertex(T item)
        {
            if (_itemIdxDict.ContainsKey(item))
            {
                Debug.Log($"[AddVertex] : {item} 정점이 {_itemIdxDict[item]}에 이미 있습니다.");
                return false;
            }

            int idx = GetEmptyIdx();

            _itemIdxDict.Add(item, idx);
            _reverseDict.Add(idx, item);

            _adjIdxListDict.Add(idx, new());

            Debug.Log($"[AddVertex] : {item}를 {_itemIdxDict[item]}로 정점 추가");

            return true;
        }

        public bool RemoveVertex(T item)
        {
            Verify();

            int targetIdx = 0;
            if (!_itemIdxDict.TryGetValue(item, out targetIdx))
            {
                Debug.Log($"[RemoveVertex] : {item} 정점이 없습니다.");
                return false;
            }

            var adjIdxList = _adjIdxListDict[targetIdx];
            foreach(int idx in adjIdxList)
            {
                _adjIdxListDict[idx].Remove(targetIdx);
            }
            _adjIdxListDict.Remove(targetIdx);

            _itemIdxDict.Remove(item);
            _reverseDict.Remove(targetIdx);

            _emptyIdx.Enqueue(targetIdx, targetIdx);

            Debug.Log($"[RemoveVertex] : {targetIdx}의 {item} 정점 삭제");

            return true;
        }

        public bool AddEdge(T from, T to)
        {
            AddVertex(from);
            AddVertex(to);

            int fromIdx = _itemIdxDict[from];
            int toIdx = _itemIdxDict[to];

            if (_adjIdxListDict[fromIdx].Contains(toIdx))
            {
                Debug.Log($"[AddEdge] : {{{_itemIdxDict[from]} - {_itemIdxDict[to]}}} 이미 존재하는 간선입니다.");
                return false;
            }

            _adjIdxListDict[fromIdx].Add(toIdx);
            _adjIdxListDict[toIdx].Add(fromIdx);

            _edgeCount++;
            Debug.Log($"[AddEdge] : {{{_itemIdxDict[from]} - {_itemIdxDict[to]}}} 간선 추가");

            return true;
        }

        public bool RemoveEdge(T from, T to)
        {
            int fromIdx = 0;
            int toIdx = 0;

            if (!_itemIdxDict.TryGetValue(from, out fromIdx) || !_itemIdxDict.TryGetValue(to, out toIdx) ||
                !_adjIdxListDict[fromIdx].Remove(toIdx) || !_adjIdxListDict[toIdx].Remove(fromIdx))
            {
                Debug.Log($"[RemoveEdge] : {{{fromIdx} - {toIdx}}} 존재하지 않는 간선입니다.");
                return false;
            }

            _edgeCount--;

            Debug.Log($"[RemoveEdge] : {{{fromIdx} - {toIdx}}} 간선 삭제");

            return true;
        }

        public bool ContainsVertex(T item) =>
            _itemIdxDict.ContainsKey(item);

        public bool ContainsEdge(T from, T to) =>
            ContainsVertex(from) && ContainsVertex(to) ? _adjIdxListDict[_itemIdxDict[from]].Contains(_itemIdxDict[to]) : false;

        public List<T> GetAdjVertices(T item) => 
            GetAdjVertices(item, 1);

        public List<T> GetAdjVertices(T item, int depth)
        {
            Verify();

            List<T> adjVertices = new();
            int targetIdx = _itemIdxDict[item];
            var visited = CreateVisited();

            PriorityQueue<(int idx, int depth), int> bfsQ = new();
            bfsQ.Enqueue((targetIdx, 0), 0);
            visited[targetIdx] = true;

            while (bfsQ.Count > 0)
            {
                (int idx, int depth) cur = bfsQ.Dequeue();
                int nextDepth = cur.depth + 1;

                if (nextDepth <= depth)
                {
                    foreach (int idx in _adjIdxListDict[cur.idx])
                    {
                        if (!visited[idx])
                        {
                            _reverseDict[idx].TryGetTarget(out T adjVertex);
                            adjVertices.Add(adjVertex);

                            bfsQ.Enqueue((idx, nextDepth), nextDepth);
                            visited[idx] = true;
                        }
                    }
                }
            }
            
            return adjVertices;
        }

        // 인접 간선 탐색

        // 그래프 분리
        // 그래프 합치기

        // private 메서드
        private int GetEmptyIdx()
        {
            if(_emptyIdx.Count > 0)
            {
                return _emptyIdx.Dequeue();
            }

            return _itemIdxDict.Count;
        }

        private bool[] CreateVisited()
        {
            int visitedSize = _itemIdxDict.Count + _emptyIdx.Count;
            bool[] visited = new bool[visitedSize];

            foreach((int, int) idx in _emptyIdx.UnorderedItems)
                visited[idx.Item1] = true;

            return visited;
        }

        public void Verify()
        {
            if (_itemIdxDict.CullMissingKey(out var missingIdx))
            {
                foreach(int idx in missingIdx)
                {
                    Debug.Log($"[Verify] : {idx}가 key를 잃었습니다.");

                    _adjIdxListDict.Remove(idx);

                    _reverseDict.Remove(idx);

                    _emptyIdx.Enqueue(idx, idx);
                }
            }
        }


        // 전체 순회(enumerable)


        // 내부 클래스
        private readonly struct Vertex
        {
            private readonly int _idx;
            private readonly T? _instance;

            public Vertex(int idx, in T? instance)
            {
                _idx = idx;
                _instance = instance;
            }

            public int Idx { get => _idx; }
            public T? Instance { get => _instance; }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                return _idx == ((Vertex)obj).Idx;
            }

            public override int GetHashCode()
            {
                return _idx.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format($"{_idx}: {_instance?.ToString()}");
            }
        }

        private readonly struct Edge
        {
            private readonly int _idx1;
            private readonly int _idx2;

            public Edge(int idx1, int idx2)
            {
                if(idx1 < idx2)
                {
                    _idx1 = idx1;
                    _idx2 = idx2;
                }
                else
                {
                    _idx1 = idx2;
                    _idx2 = idx1;
                }
            }

            public int Idx1 {  get => _idx1; }
            public int Idx2 { get => _idx2; }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                Edge temp = (Edge)obj;
                return (_idx1, _idx2) == (temp.Idx1, temp.Idx2);
            }

            public override int GetHashCode()
            {
                return (_idx1, _idx2).GetHashCode();
            }

            public override string ToString()
            {
                return string.Format($"({_idx1}, {_idx2})");
            }
        }
    }

}