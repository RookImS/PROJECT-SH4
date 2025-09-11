using UnityEngine;
using System.Collections.Generic;
using Sh4;
using System;

public class TestGraph :MonoBehaviour
{
    private Graph<GameObject> graph;
    public GameObject listObject;
    public List<GameObject> goList = new();

    private void Awake()
    {
        for (int i = 0; i < listObject.transform.childCount; i++)
        {
            goList.Add(listObject.transform.GetChild(i).gameObject);
        }
        graph = new();

        graph.AddVertex(goList[0]);        // 0 추가
        PrintContains(goList[0]);
        graph.AddVertex(goList[1]);        // 1 추가
        graph.AddEdge(goList[0], goList[1]);    // 0, 1의 간선
        graph.AddEdge(goList[2], goList[3]);    // 2, 3 추가 하면서 간선까지
        graph.AddEdge(goList[3], goList[4]);    // 4 추가 하면서 3, 4의 간선

        PrintContains(goList[5]);
        graph.RemoveVertex(goList[5]);     // 없음
        graph.RemoveVertex(goList[1]);    // 1 삭제

        graph.AddVertex(goList[5]);     // 1 추가
        graph.AddVertex(goList[1]);     // 5 추가
        graph.AddVertex(goList[5]);     // 이미 있음

        graph.RemoveEdge(goList[2], goList[3]);     // 2, 3 간선 삭제
        graph.RemoveEdge(goList[2], goList[3]);     // 간선 없음

        graph.AddEdge(goList[2], goList[3]);        // 2, 3 간선 추가
        graph.AddEdge(goList[2], goList[3]);        // 2, 3 이미 있는 간선

        graph.Clear();

        graph.AddVertex(goList[0]);
        graph.AddVertex(goList[1]);
        graph.AddVertex(goList[2]);
        graph.AddVertex(goList[3]);
        graph.AddVertex(goList[4]);
        graph.AddVertex(goList[5]);
        graph.AddVertex(goList[6]);

        graph.AddEdge(goList[0], goList[1]);
        graph.AddEdge(goList[0], goList[5]);
        graph.AddEdge(goList[2], goList[1]);
        graph.AddEdge(goList[2], goList[5]);
        graph.AddEdge(goList[2], goList[3]);

        List<GameObject> debugReader;
        debugReader = graph.GetAdjVertices(goList[0]);
        debugReader = graph.GetAdjVertices(goList[0], 2);
        List<Graph<GameObject>> comps = graph.Components;
    }

    void PrintContains(GameObject go)
    {
        if(graph.ContainsVertex(go))
            Debug.Log($"[Contains] : {go} 있음");
        else
            Debug.Log($"[Contains] : {go} 없음");
    }

    public void VerticeTest()
    {
        List<GameObject> vertices = graph.Vertices;
        graph.Print();
    }

    public void Print()
    {
        graph.Print();
    }
    public void ClearGraph()
    {
        graph.Clear();
    }

    public void Verify()
    {
        graph.Verify();
    }

    public void DestroyGameObject(int i)
    {
        Destroy(listObject.transform.GetChild(i).gameObject);
    }
}
