using System;
using System.Collections.Generic;
using System.Linq;

namespace Sh4
{
    /// <summary>
    /// <typeparamref name="T"/>의 인스턴스를 정점으로 사용하는 무방향 그래프 자료구조입니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Graph<T> : IComparable<Graph<T>> where T : class, new()
    {
#nullable enable
        // Vertex 관리
        private readonly WeakKeyDictionary<T, int> _itemIdxDict = new();
        private readonly Dictionary<int, WeakKey<T>> _reverseDict = new();
        private readonly PriorityQueue<int, int> _emptyIdxMinHeap = new();
        // Edge 관리
        private readonly Dictionary<int, List<int>> _adjIdxListDict = new();
        private readonly Dictionary<EdgeIdx, int> _edgeIdxWeightDict = new();
        // Component 관리
        private List<Graph<T>>? _components = null;
        private HashSet<int>? _articulationIdxSet = null;
        private HashSet<EdgeIdx>? _bridgeIdxSet = null;

        // 생성자
        public Graph() { UnityEditorTools.Log($"[Initializer] : 새로운 그래프 생성 {this}"); }

        public Graph(IEnumerable<T> vertices) : this(vertices, null) { }

        public Graph(IEnumerable<T> vertices, IEnumerable<Edge>? edges)
        {
            UnityEditorTools.Log($"[Initializer] : 입력을 활용해 새로운 그래프 생성 : {this}");
            foreach (T item in vertices)
            {
                AddVertex(item);
            }

            if (edges is not null)
            {
                foreach (Edge edge in edges)
                {
                    AddEdge(edge);
                }
            }
        }

        public Graph(Graph<T> graph) : this(graph.Vertices, graph.Edges) { }

        // 프로퍼티
        public IEnumerable<T> Vertices
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

        public IEnumerable<Edge> Edges
        {
            get
            {
                EnsureValid();
                return _edgeIdxWeightDict.Keys.Select(
                    (EdgeIdx edgeIdx) => (Edge)((T)_reverseDict[edgeIdx.Idx1], (T)_reverseDict[edgeIdx.Idx2], _edgeIdxWeightDict[edgeIdx])
                    );
            }
        }

        public int VertexCount
        {
            get
            {
                EnsureValid();
                return _itemIdxDict.Count;
            }
        }

        public int EdgeCount
        {
            get
            {
                EnsureValid();
                return _edgeIdxWeightDict.Count;
            }
        }

        public int Size
        {
            get
            {
                EnsureValid();
                return _itemIdxDict.Count + _edgeIdxWeightDict.Count;
            }
        }

        public IEnumerable<Graph<T>> Components { get => _components ??= EnsureComponents(); }

        public IEnumerable<T> Articulations
        {
            get
            {
                EnsureArticulationAndBridge();
                return _articulationIdxSet!.Select(
                    (int idx) => (T)_reverseDict[idx]
                    );
            }
        }

        public IEnumerable<Edge> Bridges
        {
            get
            {
                EnsureArticulationAndBridge();
                return _bridgeIdxSet!.Select(
                    (EdgeIdx edgeIdx) => (Edge)((T)_reverseDict[edgeIdx.Idx1], (T)_reverseDict[edgeIdx.Idx2], _edgeIdxWeightDict[edgeIdx])
                    );
            }
        }

        // public 메서드
        public void Print()
        {
            string dictListStr = "[Dict]\n";
            string adjMatStr = $"[Adj Matrix] Edge의 수 : {_edgeIdxWeightDict.Count}\n";

            foreach (var keyValue in _itemIdxDict)
            {
                dictListStr += "{[" + keyValue.Value + "], [" + keyValue.Key + "]}\n";
            }

            List<int> idxList = _adjIdxListDict.Keys.ToList();
            idxList.Sort();
            adjMatStr += $"{"0",-6}";
            idxList.ForEach(idx => adjMatStr += $"{idx,-6}");
            adjMatStr += '\n';

            for (int i = 0; i < idxList.Count; i++)
            {
                adjMatStr += $"{idxList[i],-6}";
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

            UnityEditorTools.Log($"Dict: {dictListStr}");
            UnityEditorTools.Log(adjMatStr);
        }

        public void Clear()
        {
            UnityEditorTools.Log($"[Clear] : {this} 그래프 초기화");
            // vertex
            _itemIdxDict.Clear();
            _reverseDict.Clear();
            _emptyIdxMinHeap.Clear();

            // edge
            _adjIdxListDict.Clear();
            _edgeIdxWeightDict.Clear();

            // component
            _components = null;
            _articulationIdxSet = null;
            _bridgeIdxSet = null;
        }

        public bool AddVertex(T item)
        {
            if (_itemIdxDict.ContainsKey(item))
            {
                UnityEditorTools.Log($"[AddVertex] : {item} 정점이 {_itemIdxDict[item]}에 이미 있습니다.");
                return false;
            }
            UnityEditorTools.Log($"[AddVertex] : {item} 정점 추가 시작");

            int idx = GetEmptyIdx();

            // vertex
            _itemIdxDict.Add(item, idx);
            _reverseDict.Add(idx, item);

            // edge
            _adjIdxListDict.Add(idx, new());

            UnityEditorTools.Log($"[AddVertex] : {item}를 {_itemIdxDict[item]}로 정점 추가");

            // component
            if (_components is not null)
            {
                UnityEditorTools.Log($"[AddVertex] : 정점 추가에 따른 component 추가");
                _components.Add(new Graph<T>(new List<T> { item }));
            }

            return true;
        }

        public bool RemoveVertex(T item)
        {
            if (!_itemIdxDict.TryGetValue(item, out int targetIdx))
            {
                UnityEditorTools.Log($"[RemoveVertex] : {item} 정점이 없습니다.");
                return false;
            }
            UnityEditorTools.Log($"[RemoveVertex] : {item} 정점 삭제 시작");

            // component
            if (_components is not null)
            {
                UnityEditorTools.Log($"[RemoveVertex] : 정점 삭제에 따른 component 갱신");
                bool isArticulation = IsArticulationIdx(targetIdx);
                foreach (Graph<T> sub in _components)
                {
                    if (sub.RemoveVertex(item))
                    {
                        if (sub._itemIdxDict.Count == 0)
                        {
                            UnityEditorTools.Log($"[RemoveVertex] : 정점 삭제에 따른 component 삭제");
                            _components.Remove(sub);
                            break;
                        }

                        if (isArticulation)
                        {
                            UnityEditorTools.Log($"[RemoveVertex] : 정점 삭제에 따른 component 분리");
                            _components.AddRange(sub.Components);
                            _components.Remove(sub);
                        }
                        break;
                    }
                }
            }
            _articulationIdxSet = null;
            _bridgeIdxSet = null;

            // edge
            foreach (int adjIdx in _adjIdxListDict[targetIdx])
            {
                _edgeIdxWeightDict.Remove((targetIdx, adjIdx));
                _adjIdxListDict[adjIdx].Remove(targetIdx);
            }
            _adjIdxListDict.Remove(targetIdx);

            // vertex
            _itemIdxDict.Remove(item);
            _reverseDict.Remove(targetIdx);
            _emptyIdxMinHeap.Enqueue(targetIdx, targetIdx);

            UnityEditorTools.Log($"[RemoveVertex] : {targetIdx}의 {item} 정점 삭제");

            return true;
        }

        public bool AddEdge(Edge edge) => AddEdge(edge.From, edge.To, edge.Weight);

        public bool AddEdge(T from, T to, int weight = 1)
        {
            AddVertex(from);
            AddVertex(to);

            int fromIdx = _itemIdxDict[from];
            int toIdx = _itemIdxDict[to];

            if (_adjIdxListDict[fromIdx].Contains(toIdx))
            {
                UnityEditorTools.Log($"[AddEdge] : {{{_itemIdxDict[from]} - {_itemIdxDict[to]}}} 이미 존재하는 간선입니다.");
                return false;
            }
            UnityEditorTools.Log($"[AddEdge] : {{{from} - {to}}} 간선 추가 시작");

            // edge
            _adjIdxListDict[fromIdx].Add(toIdx);
            _adjIdxListDict[toIdx].Add(fromIdx);
            _edgeIdxWeightDict.Add((fromIdx, toIdx), weight);

            UnityEditorTools.Log($"[AddEdge] : {{{_itemIdxDict[from]} - {_itemIdxDict[to]}}} 간선 추가");

            // component
            if (_components is not null)
            {
                UnityEditorTools.Log($"[AddEdge] : 간선 추가에 따른 component 갱신");
                List<Graph<T>> targetComponents = new();
                foreach (Graph<T> sub in _components)
                {
                    if (sub.ContainsVertex(from) || sub.ContainsVertex(to))
                    {
                        targetComponents.Add(sub);
                    }

                    if (targetComponents.Count == 2)
                    {
                        break;
                    }
                }

                targetComponents[0].AddEdge(from, to, weight);
                targetComponents.Sort();
                if (targetComponents.Count == 2)
                {
                    UnityEditorTools.Log($"[AddEdge] : 간선 추가에 따른 component 병합");
                    targetComponents[1].UnionWith(targetComponents[0]);
                    _components.Remove(targetComponents[0]);
                }
            }
            _articulationIdxSet = null;
            _bridgeIdxSet = null;

            return true;
        }

        public bool RemoveEdge(T from, T to)
        {
            int fromIdx = 0;
            int toIdx = 0;

            if (!_itemIdxDict.TryGetValue(from, out fromIdx))
            {
                UnityEditorTools.Log($"[RemoveEdge] : {from} 정점이 없습니다.");
                return false;
            }

            if (!_itemIdxDict.TryGetValue(to, out toIdx))
            {
                UnityEditorTools.Log($"[RemoveEdge] : {to} 정점이 없습니다.");
                return false;
            }

            if (!_adjIdxListDict[fromIdx].Remove(toIdx) || !_adjIdxListDict[toIdx].Remove(fromIdx))
            {
                UnityEditorTools.Log($"[RemoveEdge] : {{{fromIdx} - {toIdx}}} 존재하지 않는 간선입니다.");
                return false;
            }
            UnityEditorTools.Log($"[RemoveEdge] : {{{from} - {to}}} 간선 삭제 시작");

            EdgeIdx edgeIdx = (fromIdx, toIdx);

            // component
            if (_components is not null)
            {
                UnityEditorTools.Log($"[RemoveEdge] : 간선 삭제에 따른 component 갱신");
                bool isBridge = IsBridgeIdx(edgeIdx);
                foreach (Graph<T> sub in _components)
                {
                    if (sub.RemoveEdge(from, to))
                    {
                        if (isBridge)
                        {
                            UnityEditorTools.Log($"[RemoveEdge] : 간선 삭제에 따른 component 분리");
                            _components.AddRange(sub.Components);
                            _components.Remove(sub);
                        }
                        break;
                    }
                }
            }
            _articulationIdxSet = null;
            _bridgeIdxSet = null;

            // edge
            _edgeIdxWeightDict.Remove(edgeIdx);

            UnityEditorTools.Log($"[RemoveEdge] : {{{fromIdx} - {toIdx}}} 간선 삭제");

            return true;
        }

        public bool ContainsVertex(T item) =>
            _itemIdxDict.ContainsKey(item);

        public bool ContainsEdge(Edge edge) => ContainsEdge(edge.From, edge.To);

        public bool ContainsEdge(T from, T to) =>
            ContainsVertex(from) && ContainsVertex(to) && _adjIdxListDict[_itemIdxDict[from]].Contains(_itemIdxDict[to]);

        public IEnumerable<T> GetAdjVertices(T item, int depth = 1)
        {
            if (depth < 1)
            {
                throw new ArgumentException("Depth must be greater than or equal to 1.");
            }

            if (!ContainsVertex(item))
            {
                new ArgumentException($"The argument must be vertex of graph.");
            }

            EnsureValid();

            UnityEditorTools.Log($"[GetAdjVertices] : {item} 인접 {depth} 깊이의 정점 검색");
            int targetIdx = _itemIdxDict[item];
            List<int> adjIdxs = BFS(targetIdx, depth);
            adjIdxs.Remove(targetIdx);

            return adjIdxs.Select(
                (int adjIdx) => (T)_reverseDict[adjIdx]
                );
        }

        public bool TryGetAdjVertices(T item, out IEnumerable<T>? adjVertices) => TryGetAdjVertices(item, 1, out adjVertices);

        public bool TryGetAdjVertices(T item, int depth, out IEnumerable<T>? adjVertices)
        {
            if (depth < 1 || !ContainsVertex(item))
            {
                adjVertices = null;
                return false;
            }

            adjVertices = GetAdjVertices(item, depth);

            return true;
        }

        public int GetEdgeWeight(T from, T to)
        {
            if (!ContainsEdge(from, to))
            {
                throw new ArgumentException($"The arguments must be edge of graph.");
            }

            int fromIdx = _itemIdxDict[from];
            int toIdx = _itemIdxDict[to];

            return _edgeIdxWeightDict[(EdgeIdx)(fromIdx, toIdx)];
        }

        public bool TryGetEdgeWeight(T from, T to, out int? weight)
        {
            if(!ContainsEdge(from, to))
            {
                weight = null;
                return false;
            }

            weight = GetEdgeWeight(from, to);
            
            return true;
        }

        public bool SetEdgeWeight(Edge edge, int weight) => SetEdgeWeight(edge.From, edge.To, weight);

        public bool SetEdgeWeight(T from, T to, int weight)
        {
            if (!ContainsEdge(from, to))
            {
                return false;
            }

            int fromIdx = _itemIdxDict[from];
            int toIdx = _itemIdxDict[to];

            _edgeIdxWeightDict[(EdgeIdx)(fromIdx, toIdx)] = weight;

            return true;
        }

        public IEnumerable<Edge> GetEdgesOf(T item)
        {
            if (!ContainsVertex(item))
            {
                new ArgumentException($"The argument must be vertex of graph.");
            }

            EnsureValid();

            UnityEditorTools.Log($"[GetEdgesOf] : {item}이 연관된 간선 검색");
            int targetIdx = _itemIdxDict[item];

            return _adjIdxListDict[targetIdx].Select(
                (int adjIdx) => (Edge)((T)_reverseDict[targetIdx], (T)_reverseDict[adjIdx], _edgeIdxWeightDict[(EdgeIdx)(targetIdx, adjIdx)])
                );
        }

        public bool TryGetEdgesOf(T item, out IEnumerable<Edge>? edges)
        {
            if (!ContainsVertex(item))
            {
                edges = null;
                return false;
            }

            edges = GetEdgesOf(item);

            return true;
        }

        public bool IsArticulation(T item) =>
            IsArticulationIdx(_itemIdxDict[item]);

        public bool IsBridge(Edge edge) => IsBridge(edge.From, edge.To);

        public bool IsBridge(T from, T to) =>
            IsBridgeIdx((_itemIdxDict[from], _itemIdxDict[to]));

        public Graph<T> UnionWith(Graph<T> graph)
        {
            EnsureValid();

            UnityEditorTools.Log($"[UnionWith] : 그래프 병합");

            foreach (T item in graph.Vertices)
            {
                AddVertex(item);
            }

            foreach (Edge edge in graph.Edges)
            {
                AddEdge(edge);
            }

            return this;
        }

        // public static 메서드
        public static Graph<T> Union(Graph<T> a, Graph<T> b) =>
            new Graph<T>(a).UnionWith(b);

        // private 메서드
        private int GetEmptyIdx()
        {
            if (_emptyIdxMinHeap.Count > 0)
            {
                return _emptyIdxMinHeap.Dequeue();
            }

            return _itemIdxDict.Count;
        }

        private bool[] CreateVisited()
        {
            int visitedSize = _itemIdxDict.Count + _emptyIdxMinHeap.Count;
            bool[] visited = new bool[visitedSize];

            foreach ((int idx, int priority) in _emptyIdxMinHeap.UnorderedItems)
            {
                visited[idx] = true;
            }

            return visited;
        }

        private bool IsArticulationIdx(int idx)
        {
            if (_adjIdxListDict[idx].Count <= 1)
            {
                return false;
            }

            EnsureArticulationAndBridge();
            return _articulationIdxSet!.Contains(idx);
        }

        private bool IsBridgeIdx(EdgeIdx edgeIdx)
        {
            if (_adjIdxListDict[edgeIdx.Idx1].Count <= 1 || _adjIdxListDict[edgeIdx.Idx2].Count <= 1)
            {
                return true;
            }

            EnsureArticulationAndBridge();
            return _bridgeIdxSet!.Contains(edgeIdx);
        }

        private List<Graph<T>> EnsureComponents()
        {
            UnityEditorTools.Log("[EnsureComponents] : 그래프 내 컴포넌트를 구분합니다.");

            EnsureValid();

            List<Graph<T>> components = new();
            bool[] visited = CreateVisited();

            for (int i = 0; i < visited.Length; i++)
            {
                if (visited[i])
                {
                    continue;
                }

                List<int> compVertexIdxs = BFS(i, -1);

                List<T> compVertices = new(compVertexIdxs.Count);
                List<Edge> compEdges = new();

                foreach (int idx in compVertexIdxs)
                {
                    T vertex = (T)_reverseDict[idx];
                    compVertices.Add(vertex);
                    foreach (int adjIdx in _adjIdxListDict[idx])
                    {
                        T adjVertex = (T)_reverseDict[adjIdx];
                        compEdges.Add((vertex, adjVertex, _edgeIdxWeightDict[(EdgeIdx)(idx, adjIdx)]));
                    }

                    visited[idx] = true;
                }

                components.Add(new Graph<T>(compVertices, compEdges));
            }

            return components;
        }

        private void EnsureArticulationAndBridge()
        {
            UnityEditorTools.Log("[EnsureArticulationAndBridge] : 그래프 내 단절점, 단절선을 찾습니다.");

            EnsureValid();

            if (_articulationIdxSet is not null && _bridgeIdxSet is not null)
            {
                UnityEditorTools.Log("[EnsureArticulationAndBridge] : 이미 찾은 단절점, 단절선이 있습니다.");
                return;
            }

            _components ??= EnsureComponents();

            if (_components.Count == 1)
            {
                SetArticulationAndBridge();
            }
            else
            {
                foreach (Graph<T> sub in _components)
                {
                    sub.SetArticulationAndBridge();
                    var articulations = sub._articulationIdxSet.Select(
                        (int idx) => (T)sub._reverseDict[idx] 
                        );
                    var bridges = sub._bridgeIdxSet.Select(
                        (EdgeIdx edgeIdx) => ((T)sub._reverseDict[edgeIdx.Idx1], (T)sub._reverseDict[edgeIdx.Idx2], sub._edgeIdxWeightDict[edgeIdx])
                        );

                    foreach (T item in articulations)
                    {
                        (_articulationIdxSet ??= new()).Add(_itemIdxDict[item]);
                    }

                    foreach (Edge edge in bridges)
                    {
                        (_bridgeIdxSet ??= new()).Add((_itemIdxDict[edge.From], _itemIdxDict[edge.To]));
                    }
                }
            }
        }

        private void SetArticulationAndBridge()
        {
            _articulationIdxSet = new();
            _bridgeIdxSet = new();

            // idx, dfn, low
            Dictionary<int, int[]> dfnlow = new();
            int rootIdx = -1;

            foreach (int idx in _itemIdxDict.Values.ToList())
            {
                if (rootIdx == -1)
                {
                    rootIdx = idx;
                }

                dfnlow.Add(idx, new int[] { -1, _itemIdxDict.Count });
            }
            int count = 0;
            DfnLow(rootIdx, rootIdx, ref count, ref dfnlow);
        }

        private List<int> BFS(int rootIdx, int searchDepth)
        {
            if (rootIdx < 0)
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

        private void DfnLow(int curIdx, int parentIdx, ref int count, ref Dictionary<int, int[]> dfnlow)
        {
            dfnlow[curIdx][0] = count;
            dfnlow[curIdx][1] = count;
            count++;

            // rootIdx이면서 child가 2개 이상 이면 articulation
            if (curIdx == parentIdx && _adjIdxListDict[curIdx].Count > 1)
            {
                _articulationIdxSet!.Add(curIdx);
            }

            foreach (int adjIdx in _adjIdxListDict[curIdx])
            {
                // parent는 탐색하지 않음
                if (adjIdx == parentIdx)
                    continue;

                // unvisited, 나의 low와 child의 low를 비교
                if (dfnlow[adjIdx][0] < 0)
                {
                    int childIdx = adjIdx;
                    DfnLow(childIdx, curIdx, ref count, ref dfnlow);

                    dfnlow[curIdx][1] = Math.Min(dfnlow[curIdx][1], dfnlow[childIdx][1]);

                    // articulation 판단
                    if (curIdx != parentIdx && dfnlow[curIdx][0] <= dfnlow[childIdx][1])
                    {
                        _articulationIdxSet!.Add(curIdx);
                    }

                    // bridge 판단
                    if (dfnlow[curIdx][0] < dfnlow[childIdx][1])
                    {
                        _bridgeIdxSet!.Add((curIdx, childIdx));
                    }
                }
                else    // visitied(backward edge), 나의 low와 backward edge의 dfn을 비교
                {
                    dfnlow[curIdx][1] = Math.Min(dfnlow[curIdx][1], dfnlow[adjIdx][0]);
                }
            }
        }

        /// <summary>
        /// <see cref="WeakKeyDictionary{TKey, TValue}"/>의 사용에 따른 키의 유효성을 검사해서 그래프가 항상 유효한 상태가 되도록 보장합니다.
        /// </summary>
        /// <returns>키에 문제가 있어서 이에 알맞게 그래프를 수정했으면 <see langword="true"/>, 그렇지 않으면 <see langword="false"/></returns>
        private bool EnsureValid()
        {
            UnityEditorTools.Log($"[EnsureValid] : 실행");
            if (_itemIdxDict.CullMissingKey(out List<int> missingIdxs))
            {
                InvalidateIndex(missingIdxs);
                return true;
            }

            return false;
        }

        private void InvalidateIndex(List<int> missingIdxs)
        {
            foreach (int idx in missingIdxs)
            {
                UnityEditorTools.Log($"[InvalidateIndex] : {idx}가 key를 잃었습니다.");

                // component
                _components = null;
                _articulationIdxSet = null;
                _bridgeIdxSet = null;

                // edge
                foreach (int adjIdx in _adjIdxListDict[idx])
                {
                    _edgeIdxWeightDict.Remove((idx, adjIdx));
                    _adjIdxListDict[adjIdx].Remove(idx);
                }
                _adjIdxListDict.Remove(idx);

                // vertex
                _reverseDict.Remove(idx);
                _emptyIdxMinHeap.Enqueue(idx, idx);
            }
        }

        public int CompareTo(Graph<T> other)
        {
            if (other == null)
                return 1;

            return Size.CompareTo(other.Size);
        }

        // 내부 클래스
        public readonly struct Edge
        {
            private readonly T _from;
            private readonly T _to;
            private readonly int _weight;

            public Edge(T from, T to, int weight = 1)
            {
                _from = from;
                _to = to;
                _weight = weight;
            }

            public T From => _from;
            public T To => _to;
            public int Weight => _weight;

            public override string ToString()
            {
                return string.Format($"({_from} - {_to}, {_weight})");
            }

            public static implicit operator Edge((T from, T to) tuple) => new(tuple.from, tuple.to);

            public static implicit operator Edge((T from, T to, int weight) tuple) => new(tuple.from, tuple.to, tuple.weight);

            public static explicit operator (T, T, int)(Edge edge) => (edge._from, edge._to, edge._weight);
        }

        private readonly struct EdgeIdx
        {
            private readonly int _idx1;
            private readonly int _idx2;

            public EdgeIdx(int idx1, int idx2)
            {
                if (idx1 < idx2)
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

            public int Idx1 => _idx1;
            public int Idx2 => _idx2;
            public override string ToString()
            {
                return string.Format($"({_idx1} - {_idx2})");
            }

            public override int GetHashCode() =>
                (_idx1, _idx2).GetHashCode();

            public override bool Equals(object? obj) =>
                obj is EdgeIdx other && Equals(other);

            public bool Equals(EdgeIdx e) =>
                _idx1 == e._idx1 && _idx2 == e._idx2;

            public static bool operator ==(EdgeIdx lhs, EdgeIdx rhs) => lhs.Equals(rhs);

            public static bool operator !=(EdgeIdx lhs, EdgeIdx rhs) => !(lhs == rhs);

            public static implicit operator EdgeIdx((int idx1, int idx2) tuple) => new(tuple.idx1, tuple.idx2);

            public static explicit operator (int, int)(EdgeIdx edgeIdx) => (edgeIdx._idx1, edgeIdx._idx2);
        }
    }
}