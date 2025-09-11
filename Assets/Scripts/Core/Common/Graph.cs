using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Sh4
{
    public class Graph<T> where T : class, new()
    {
#nullable enable
        // Vertex 관리
        private readonly WeakKeyDictionary<T, int> _itemIdxDict = new();
        private readonly Dictionary<int, WeakKey<T>> _reverseDict = new();
        private readonly PriorityQueue<int, int> _emptyIdxMinHeap = new();
        // Edge 관리
        private readonly Dictionary<int, List<int>> _adjIdxListDict = new();
        private readonly Dictionary<Edge, int> _edgeWeightDict = new();
        // Component 관리
        private List<Graph<T>>? _components = null;
        private List<int>? _articulationIdxList = null;
        private List<Edge>? _bridgeList = null;

        // 생성자
        public Graph() { }

        public Graph(List<T> vertices) : this(vertices, new()) { }

        public Graph(List<T> vertices, List<(T, T)> edges)  
        {
            foreach (T item in vertices)
            {
                AddVertex(item);
            }

            foreach((T from, T to) in edges)
            {
                AddEdge(from, to);
            }
        }

        // 프로퍼티
        public List<T> Vertices
        {
            get
            {
                if (!_itemIdxDict.TryGetAllOriginalKeys(out List<T> vertices, out List<int> missingIdxs))
                {
                    InvalidateIndex(missingIdxs);
                }

                return vertices;
            }
        }

        public int VertexCount { get => _itemIdxDict.Count; }

        public int EdgeCount { get => _edgeWeightDict.Count; }

        public List<Graph<T>> Components { get => _components ??= EnsureComponents(); }

        public List<T> Articulations 
        { 
            get
            {
                List<T> articulations = new();

                if (_articulationIdxList is null)
                {
                    SetArticulationAndBridge();
                }

                foreach (int idx in _articulationIdxList!)
                {
                    articulations.Add((T)_reverseDict[idx]);
                }

                return articulations;
            }
        }
        
        public List<(T, T)> Bridges
        {
            get
            {
                List<(T, T)> bridges = new();

                if (_bridgeList is null)
                {
                    SetArticulationAndBridge();
                }

                foreach (Edge edge in _bridgeList!)
                {
                    bridges.Add(((T)_reverseDict[edge.Idx1], (T)_reverseDict[edge.Idx1]));
                }

                return bridges;
            }
        }

        // public 메서드
        public void Print()
        {
            string dictListStr = "[Dict]\n";
            string adjMatStr = $"[Adj Matrix] Edge의 수 : {_edgeWeightDict.Count}\n";

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
            // vertex
            _itemIdxDict.Clear();
            _reverseDict.Clear();
            _emptyIdxMinHeap.Clear();

            // edge
            _adjIdxListDict.Clear();
            _edgeWeightDict.Clear();

            // component
            _components = null;
            _articulationIdxList = null;
            _bridgeList = null;
        }

        public bool AddVertex(T item)
        {
            if (_itemIdxDict.ContainsKey(item))
            {
                Debug.Log($"[AddVertex] : {item} 정점이 {_itemIdxDict[item]}에 이미 있습니다.");
                return false;
            }

            int idx = GetEmptyIdx();

            // vertex
            _itemIdxDict.Add(item, idx);
            _reverseDict.Add(idx, item);

            // edge
            _adjIdxListDict.Add(idx, new());

            Debug.Log($"[AddVertex] : {item}를 {_itemIdxDict[item]}로 정점 추가");

            // component
            if (_components != null)
            {
                _components.Add(new Graph<T>(new List<T>{ item }));
            }

            return true;
        }

        public bool RemoveVertex(T item)
        {
            if (!_itemIdxDict.TryGetValue(item, out int targetIdx))
            {
                Debug.Log($"[RemoveVertex] : {item} 정점이 없습니다.");
                return false;
            }

            // component
            if (_components != null && IsArticulationIdx(targetIdx))
            {
                // TODO
                // 정점을 포함하고 있는 컴포넌트를 분리해서 여러 컴포넌트로 만듦
            }
            _articulationIdxList = null;
            _bridgeList = null;

            // edge
            foreach(int adjIdx in _adjIdxListDict[targetIdx])
            {
                _edgeWeightDict.Remove(new Edge(targetIdx, adjIdx));
                _adjIdxListDict[adjIdx].Remove(targetIdx);
            }
            _adjIdxListDict.Remove(targetIdx);

            // vertex
            _itemIdxDict.Remove(item);
            _reverseDict.Remove(targetIdx);
            _emptyIdxMinHeap.Enqueue(targetIdx, targetIdx);

            Debug.Log($"[RemoveVertex] : {targetIdx}의 {item} 정점 삭제");

            return true;
        }

        public bool AddEdge(T from, T to, int weight = 1)
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

            // edge
            _adjIdxListDict[fromIdx].Add(toIdx);
            _adjIdxListDict[toIdx].Add(fromIdx);
            _edgeWeightDict.Add(new Edge(fromIdx, toIdx), weight);

            Debug.Log($"[AddEdge] : {{{_itemIdxDict[from]} - {_itemIdxDict[to]}}} 간선 추가");

            // component
            if (_components != null)
            {
                // TODO
                // 각각에 간선 추가 후 병합
            }
            _articulationIdxList = null;
            _bridgeList = null;

            return true;
        }

        public bool RemoveEdge(T from, T to)
        {
            int fromIdx = 0;
            int toIdx = 0;

            if (!_itemIdxDict.TryGetValue(from, out fromIdx))
            {
                Debug.Log($"[RemoveVertex] : {from} 정점이 없습니다.");
                return false;
            }

            if (!_itemIdxDict.TryGetValue(to, out toIdx))
            {
                Debug.Log($"[RemoveVertex] : {to} 정점이 없습니다.");
                return false;
            }

            if(!_adjIdxListDict[fromIdx].Remove(toIdx) || !_adjIdxListDict[toIdx].Remove(fromIdx))
            {
                Debug.Log($"[RemoveEdge] : {{{fromIdx} - {toIdx}}} 존재하지 않는 간선입니다.");
                return false;
            }

            // component
            if (_components != null && (IsArticulationIdx(fromIdx) || IsArticulationIdx(toIdx)))
            {
                // TODO
                // 간선을 포함하고 있는 컴포넌트를 분리해서 여러 컴포넌트로 만듦
            }
            _articulationIdxList = null;
            _bridgeList = null;

            // edge
            _edgeWeightDict.Remove(new Edge(fromIdx, toIdx));

            Debug.Log($"[RemoveEdge] : {{{fromIdx} - {toIdx}}} 간선 삭제");

            return true;
        }

        public bool ContainsVertex(T item) =>
            _itemIdxDict.ContainsKey(item);

        public bool ContainsEdge(T from, T to) =>
            ContainsVertex(from) && ContainsVertex(to) ? _adjIdxListDict[_itemIdxDict[from]].Contains(_itemIdxDict[to]) : false;

        public List<T> GetAdjVertices(T item, int depth = 1)
        {
            Verify();

            int targetIdx = _itemIdxDict[item];
            List<int> adjIdxs = BFS(targetIdx, depth);
            List<T> adjVertices = new();
            
            foreach(int idx in adjIdxs)
            {
                if(idx == targetIdx)
                {
                    continue; 
                }

                adjVertices.Add((T)_reverseDict[idx]);
            }

            return adjVertices;
        }

        public bool IsArticulation(T item)
        {
            return IsArticulationIdx(_itemIdxDict[item]);
        }

        public bool IsBridge(T from, T to)
        {
            return IsBridgeIdx(new Edge(_itemIdxDict[from], _itemIdxDict[to]));
        }
        // 인접 간선 탐색

        // 그래프 분리
        // 그래프 합치기

        // private 메서드
        private int GetEmptyIdx()
        {
            if(_emptyIdxMinHeap.Count > 0)
            {
                return _emptyIdxMinHeap.Dequeue();
            }

            return _itemIdxDict.Count;
        }

        private bool[] CreateVisited()
        {
            int visitedSize = _itemIdxDict.Count + _emptyIdxMinHeap.Count;
            bool[] visited = new bool[visitedSize];

            foreach ((int, int) idx in _emptyIdxMinHeap.UnorderedItems)
            {
                visited[idx.Item1] = true;
            }

            return visited;
        }

        private List<Graph<T>> EnsureComponents()
        {
            Debug.Log("[EnsureComponents] 그래프 내 컴포넌트를 구분합니다.");

            Verify();

            List<Graph<T>> components = new();
            bool[] visited = CreateVisited();

            for (int i = 0; i < visited.Length; i++)
            {
                if (visited[i])
                {
                    continue;
                }

                List<int> compVertexIdxs = BFS(i, -1);
                List<T> compVertices = new();
                List<(T, T)> compEdges = new();

                foreach (int idx in compVertexIdxs)
                {
                    T vertex = (T)_reverseDict[idx];
                    compVertices.Add(vertex);
                    foreach(int adjIdx in _adjIdxListDict[idx])
                    {
                        T adjVertex = (T)_reverseDict[adjIdx];
                        compEdges.Add((vertex, adjVertex));
                    }

                    visited[idx] = true;
                }

                components.Add(new Graph<T>(compVertices, compEdges));
            }

            return components;
        }

        // TODO
        private void SetArticulationAndBridge()
        {

        }

        private bool IsArticulationIdx(int idx)
        {
            if (_adjIdxListDict[idx].Count <= 1)
            {
                return false;
            }

            if (_articulationIdxList is null)
            {
                SetArticulationAndBridge();
            }

            return _articulationIdxList!.Contains(idx);
        }

        private bool IsBridgeIdx(Edge edge)
        {
            if (_adjIdxListDict[edge.Idx1].Count <= 1 || _adjIdxListDict[edge.Idx2].Count <= 1)
            {
                return true;
            }

            if(_articulationIdxList is null)
            {
                SetArticulationAndBridge();
            }

            return _bridgeList!.Contains(edge);
        }

        private List<int> BFS(int rootIdx, int searchDepth)
        {
            if(rootIdx < 0)
            {
                throw new ArgumentOutOfRangeException("RootIdx must be greater than or equal to 0.");
            }

            if (searchDepth < -1)
            {
                throw new ArgumentOutOfRangeException("Search depth must be greater than or equal to -1.");
            }

            List<int> searchingVertexIdxs = new();
            var visited = CreateVisited();

            PriorityQueue<(int idx, int depth), int> bfsQ = new();

            searchingVertexIdxs.Add(rootIdx);
            bfsQ.Enqueue((rootIdx, 0), 0);
            visited[rootIdx] = true;

            while (bfsQ.Count > 0)
            {
                (int idx, int depth) cur = bfsQ.Dequeue();
                int nextDepth = cur.depth + 1;

                if (searchDepth == -1 || nextDepth <= searchDepth)
                {
                    foreach (int idx in _adjIdxListDict[cur.idx])
                    {
                        if (!visited[idx])
                        {
                            searchingVertexIdxs.Add(idx);
                            bfsQ.Enqueue((idx, nextDepth), nextDepth);
                            visited[idx] = true;
                        }
                    }
                }
            }

            return searchingVertexIdxs;
        }

        // TODO
        private List<int> DFS(int searchIdx, ref int[] dfn, ref int[] low)
        {

        }

        public void Verify()
        {
            if (_itemIdxDict.CullMissingKey(out List<int> missingIdxs))
            {
                InvalidateIndex(missingIdxs);
            }
        }

        private void InvalidateIndex(List<int> missingIdxs)
        {
            foreach (int idx in missingIdxs)
            {
                Debug.Log($"[Vertices] : {idx}가 key를 잃었습니다.");

                // component
                if (_components != null)
                {
                    foreach (var component in _components)
                    {
                        if (component.ContainsVertex((T)_reverseDict[idx]))
                        {
                            component.Verify();
                        }
                    }
                }
                _articulationIdxList = null;
                _bridgeList = null;

                // edge
                foreach (int adjIdx in _adjIdxListDict[idx])
                {
                    _edgeWeightDict.Remove(new Edge(idx, adjIdx));
                    _adjIdxListDict[adjIdx].Remove(idx);
                }
                _adjIdxListDict.Remove(idx);

                // vertex
                _reverseDict.Remove(idx);
                _emptyIdxMinHeap.Enqueue(idx, idx);
            }
        }
        // 전체 순회(enumerable)


        // 내부 클래스
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

            public override string ToString()
            {
                return string.Format($"({_idx1}, {_idx2})");
            }

            public override int GetHashCode() =>
                (_idx1, _idx2).GetHashCode();

            public override bool Equals(object? obj) => 
                obj is Edge other && Equals(other);

            public bool Equals(Edge e) => 
                _idx1 == e._idx1 && _idx2 == e._idx2;

            public static bool operator ==(Edge lhs, Edge rhs) => lhs.Equals(rhs);

            public static bool operator !=(Edge lhs, Edge rhs) => !(lhs == rhs);
        }
    }

}