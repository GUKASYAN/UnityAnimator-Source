


#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

public class GameManager : MonoBehaviour
{
    public Camera UiCamera;
    public static List<Node>[] FramesNodes;
    private List<LineRenderer>[] _framesLines;
    public static int FramesCurrent;
    public UILabel LabelX;
    public UILabel LabelY;
    public UILabel LabelCurrentFrame;
    public UILabel LabelFramesCount;
    public UILabel LabelFramesInterval;
    public UIButton ButtonNextFrame;
    public UIButton ButtonPreviousFrame;
    public UIButton ButtonNewFrame;
    public UIButton ButtonDeleteFrame;
    private Transform _chosedItem;
    public GameObject Item;
    private SourceManager _sourceManager;
    private Transform _transform;

    private void Start()
    {
        FramesCurrent = 0;
        FramesNodes = new List<Node>[1];
        FramesNodes[0] = new List<Node>();
        _sourceManager = new SourceManager();
        _transform = transform;
        checkButtons();
    }

    private void Update()
    {
        LabelX.text = "" + Input.mousePosition.x;
        LabelY.text = "" + Input.mousePosition.y;
        if (Input.GetMouseButtonUp(0))
        {
            if (_chosedItem != null)
            {
                if (!Physics.Raycast(UiCamera.ScreenPointToRay(Input.mousePosition)))
                {
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.up);
                    if (hit && hit.collider.name.Contains("Node"))
                    {
                        return;
                    }
                    Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    _chosedItem.position = pos;
                    int selectedIndex = getSelectedIndex(_chosedItem.gameObject);
                    _chosedItem = null;
                    if (selectedIndex >= 0 && selectedIndex < FramesNodes[FramesCurrent].Count)
                    {
                        if (selectedIndex >0  &&
                            FramesNodes[FramesCurrent][selectedIndex-1].LineOutIndex != 0)
                        {
                            buildLine(selectedIndex, false);
                        }
                        if (FramesNodes[FramesCurrent][selectedIndex].LineOutIndex != 0)
                        {
                            buildLine(selectedIndex+1, false);
                        }
                    }
                    return;
                }
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (!Physics.Raycast(UiCamera.ScreenPointToRay(Input.mousePosition)))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit)
                {
                    if (_chosedItem == null)
                    {
                        _chosedItem = hit.transform;
                    }
                }
                else
                {
                    Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    pos.z = 0;
                    Node node = new Node(Item, _transform, pos);
                    FramesNodes[FramesCurrent].Add(node);
                    node.gameObject.name = "Node#" + (FramesCurrent) + ":" + FramesNodes[FramesCurrent].Count();
                }
            }
        }
        if (Input.GetMouseButtonUp(1))
        {
            if (!Physics.Raycast(UiCamera.ScreenPointToRay(Input.mousePosition)))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.up);
                if (hit && hit.collider.name.Contains("Node"))
                {
                    int selectedIndex = getSelectedIndex(hit.collider.gameObject);
                      if (selectedIndex >= 0 && FramesNodes[FramesCurrent].Count > selectedIndex)
                    {
                        if (selectedIndex > 0 && FramesNodes[FramesCurrent][selectedIndex - 1].LineOutIndex!=0)
                        {
                          buildLine(selectedIndex,true);
                          buildLine(selectedIndex + 1, true); 
                        }
                        FramesNodes[FramesCurrent].RemoveAt(selectedIndex);
                        Destroy(hit.collider.gameObject);
                    }
                }
            }
        }
        if (Input.GetKeyUp(KeyCode.A) && ButtonPreviousFrame.enabled)
            ButtonPreviousFrame_Click();
        if (Input.GetKeyUp(KeyCode.D) && ButtonNextFrame.enabled)
            ButtonNextFrame_Click();
    }

    private int getSelectedIndex(GameObject selectedGameObject)
    {
        try
        {
            return FramesNodes[FramesCurrent].Select((node, index) => new { node, index })
                        .First(s => s.node.gameObject == selectedGameObject)
                        .index;
        }
        catch (Exception)
        {
            return 0;
        }
     
    }

    public static void showTheFrame(int framesCurrent, bool mode)
    {
        if (framesCurrent >= FramesNodes.Length || FramesNodes[framesCurrent] == null) return;
        for (int i = 0; i < FramesNodes[framesCurrent].Count; i++)
        {
            FramesNodes[framesCurrent][i].gameObject.SetActive(mode);
        }
    }

    private void checkButtons()
    {
        ButtonNextFrame.enabled = FramesCurrent < FramesNodes.Length - 1;
        ButtonPreviousFrame.enabled = FramesCurrent > 0;
        ButtonDeleteFrame.enabled = FramesNodes.Length > 1;
        LabelCurrentFrame.text = "" + (FramesCurrent + 1);
        LabelFramesCount.text = "(" + FramesNodes.Length + ")";
    }

    private void buildLine(int finishNodeIndex, bool reset)
    {
        if (finishNodeIndex < 1 || finishNodeIndex >= FramesNodes[FramesCurrent].Count) return;
        Node start = FramesNodes[FramesCurrent][finishNodeIndex - 1];
        Node finish = FramesNodes[FramesCurrent][finishNodeIndex];
        if (!reset)
        {
            start.SetLineStart(start.Transform.position);
            start.SetLineFinish(finishNodeIndex, finish.Transform.position);
        }
        else
        {
            start.SetLineStart(start.Transform.position);
            start.SetLineFinish(0, start.Transform.position);
        }
    }

    public void ButtonSave_Click()
    {
        _sourceManager.SaveToTxt(FramesNodes);
    }

    public void ButtonLoad_Click()
    {
        foreach (var item in FramesNodes.SelectMany(node => node))
        {
            Destroy(item.gameObject);
        }
        FramesNodes = _sourceManager.LoadFile(Item, _transform);
        FramesCurrent = 0;
        showTheFrame(FramesCurrent, true);
        checkButtons();
    }

    public void ButtonNewFrame_Click()
    {
        Array.Resize(ref FramesNodes, FramesNodes.Length + 1);
        FramesCurrent = FramesNodes.Length - 1;
        int indexBuffer=-1;
        FramesNodes[FramesCurrent] = new List<Node>(FramesNodes[FramesCurrent - 1].Count);
        FramesNodes[FramesCurrent - 1].ForEach(item =>
        {
            Node node = new Node(Item, _transform, item.Transform.position);
            FramesNodes[FramesCurrent].Add(node);
            node.gameObject.name = "Node#" + (FramesCurrent) + ":" + FramesNodes[FramesCurrent].Count();
            if (indexBuffer != -1)
            {
                buildLine(indexBuffer+1,false);
                indexBuffer = -1;
            }
            if (item.LineOutIndex != 0)
                indexBuffer = FramesNodes[FramesCurrent].Count - 1;

        });
        showTheFrame(FramesCurrent - 1, false);
        showTheFrame(FramesCurrent, true);
        checkButtons();
    }

    public void ButtonDeleteFrame_Click()
    {
        foreach (var node in FramesNodes[FramesCurrent])
        {
            Destroy(node.gameObject);
        }
        FramesNodes[FramesCurrent] = new List<Node>();

        if (FramesCurrent < FramesNodes.Length)
        {
            FramesNodes = FramesNodes.Where(item => item != FramesNodes[FramesCurrent]).ToArray();
            if (FramesCurrent >= 1)
                FramesCurrent--;
            else FramesCurrent = 0;
            showTheFrame(FramesCurrent, true);
        }
        checkButtons();
    }

    public void ButtonNextFrame_Click()
    {
        showTheFrame(FramesCurrent, false);
        FramesCurrent++;
        showTheFrame(FramesCurrent, true);
        checkButtons();
    }

    public void ButtonPreviousFrame_Click()
    {
        showTheFrame(FramesCurrent, false);
        FramesCurrent--;
        showTheFrame(FramesCurrent, true);
        checkButtons();
    }

    public void ButtonBuildLine_Click()
    {
        buildLine(FramesNodes[FramesCurrent].Count-1,false);
    }

    public void ButtonPlay_Click()
    {
        showTheFrame(FramesCurrent, false);
        FramesCurrent = 0;
        checkButtons();
        int ms;
        Int32.TryParse(LabelFramesInterval.text, out ms);
        if (ms == 0)
        {
            ms = 1000;
            LabelFramesInterval.text = "1000";
        }
        StartCoroutine(playAnimation(ms));
    }

    private IEnumerator playAnimation(int ms)
    {
        foreach (var frame in FramesNodes)
        {
            showTheFrame(FramesCurrent, true);
            if (FramesCurrent > 0)
                showTheFrame(FramesCurrent - 1, false);
            if (FramesCurrent < FramesNodes.Length - 1)
                FramesCurrent++;
            checkButtons();
            yield return new WaitForSeconds((float) ms/1000);
        }
    }

    public void ButtonExit_Click()
    {
        Application.Quit();
    }
}
 


