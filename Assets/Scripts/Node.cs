#region

using System;
using UnityEngine;

#endregion

[Serializable]
public class Node : MonoBehaviour
{
    private readonly GameObject node;
    [HideInInspector] public Transform Transform;
    private readonly LineRenderer _lineRenderer;
    public Vector2 LineStart;
    public Vector2 LineFinish;
    [HideInInspector] public int LineOutIndex;

    public new GameObject gameObject
    {
        get { return node; }
    }

    public Node(GameObject item, Transform parent, Vector2 pos)
    {
        node = CreateItem(item, parent, pos);
        _lineRenderer = node.GetComponent<LineRenderer>();
    }

    public void SetLineStart(Vector3 start)
    {
        _lineRenderer.SetPosition(0, start);
        LineStart = start;
    }

    public void SetLineFinish(int finishIndex, Vector3 finish)
    {
        _lineRenderer.SetPosition(1, finish);
        LineFinish = finish;
        LineOutIndex = finishIndex;
    }

    public GameObject CreateItem(GameObject item, Transform parent, Vector3 pos)
    {
        GameObject node = Instantiate(item, pos, Quaternion.identity) as GameObject;
        if (node != null)
        {
            Transform = node.transform;
            Transform.parent = parent;
            node.AddComponent<Node>();
            return node;
        }
        return null;
    }
}
