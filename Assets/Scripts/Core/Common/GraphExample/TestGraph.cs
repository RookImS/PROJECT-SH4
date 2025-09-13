using UnityEngine;
using System.Collections.Generic;
using Sh4;
using System;
using System.Linq;

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

        List<Graph<GameObject>> forDebug;
        // 일반 생성
        graph = new();
        forDebug = graph.Components.ToList();

        Debug.LogWarning("!!!!!기본 Vertex테스트!!!!!");
        Debug.LogWarning("!!!!!기본 추가!!!!!");
        foreach(GameObject go in goList)
            graph.AddVertex(go);

        // Debug.Log("!!!!!중복 추가!!!!!");
        //foreach (GameObject go in goList)
        //    graph.AddVertex(go);

        Debug.LogWarning("!!!!!기본 Edge테스트!!!!!");
        Debug.LogWarning("!!!!!vertex 2개 존재하는 경우의 추가!!!!!");
        graph.AddEdge(goList[0], goList[1]);
        graph.AddEdge(goList[0], goList[5]);
        graph.AddEdge(goList[1], goList[2]);
        graph.AddEdge(goList[1], goList[3]);
        graph.AddEdge(goList[2], goList[4]);
        graph.AddEdge(goList[3], goList[4]);

        Debug.LogWarning("!!!!!vertex 1개만 존재하는 경우의 추가!!!!!");
        graph.RemoveVertex(goList[7]);
        graph.AddEdge(goList[6], goList[7]);

        Debug.LogWarning("!!!!!vertex 없는 경우의 추가!!!!!");
        graph.RemoveVertex(goList[6]);
        graph.RemoveVertex(goList[7]);
        graph.AddEdge(goList[6], goList[7]);

        Debug.LogWarning("!!!!!edge 삭제 테스트!!!!!");
        Debug.LogWarning("!!!!!vertex 하나만 없는 경우!!!!!");
        graph.RemoveVertex(goList[6]);
        graph.RemoveEdge(goList[6], goList[7]);
        graph.AddEdge(goList[6], goList[7]);
        Debug.LogWarning("!!!!!vertex 둘다 없는 경우!!!!!");
        graph.RemoveVertex(goList[7]);
        graph.RemoveVertex(goList[6]);
        graph.RemoveEdge(goList[6], goList[7]);
        Debug.LogWarning("!!!!!기본 삭제!!!!!");
        graph.AddEdge(goList[6], goList[7]);
        graph.RemoveEdge(goList[6], goList[7]);

        Debug.LogWarning("!!!!!절단점 테스트!!!!!");
        List<GameObject> arttemp = graph.Articulations.ToList();
        graph.RemoveVertex(goList[0]);
        graph.AddEdge(goList[0], goList[5]);
        graph.AddEdge(goList[1], goList[0]);
        Debug.LogWarning("!!!!!절단선 테스트!!!!!");
        List<(GameObject, GameObject, int)> bridgetemp = graph.Bridges.ToList();
        graph.RemoveEdge(goList[0], goList[1]);
        graph.RemoveEdge(goList[0], goList[5]);
        //Debug.LogWarning("!!!!!중복 추가!!!!!");
        //graph.AddEdge(goList[6], goList[7]);

        //Debug.LogWarning("!!!!!EmptyIdx 작동 테스트!!!!!");
        //graph.RemoveVertex(goList[6]);
        //Debug.Log($"현재 Edge개수 : " + graph.EdgeCount);
        //graph.RemoveVertex(goList[5]);
        //Debug.Log($"현재 Edge개수 : " + graph.EdgeCount);

        //graph.AddVertex(goList[6]);
        //graph.AddVertex(goList[5]);
        //graph.AddEdge(goList[0], goList[5]);

        //graph.RemoveVertex(goList[6]);
        //Debug.Log($"현재 Edge개수 : " + graph.EdgeCount);

        //Debug.LogWarning("!!!!!Contains작동 테스트!!!!!");
        //foreach (GameObject go1 in goList)
        //{
        //    Debug.Log($"{go1} vertex : " + graph.ContainsVertex(go1));
        //    foreach (GameObject go2 in goList)
        //    {
        //        Debug.Log($"{{{go1} - {go2}}} edge : " + graph.ContainsEdge(go1, go2));
        //    }
        //}

        //Debug.LogWarning("!!!!!인접 테스트!!!!!");
        //for (int i = 1; i < 7; i++)
        //{
        //    List<GameObject> temp = graph.GetAdjVertices(goList[5], i);

        //    string tempStr = "";
        //    foreach (GameObject go in temp)
        //    {
        //        tempStr += (go.ToString() + " ");
        //    }
        //    Debug.Log($"깊이 {i}의 인접 노드 : " + tempStr);
        //}

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
        List<GameObject> vertices = graph.Vertices.ToList();
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

    //public void Verify()
    //{
    //    graph.Verify();
    //}

    public void DestroyGameObject(int i)
    {
        Destroy(listObject.transform.GetChild(i).gameObject);
    }
}
