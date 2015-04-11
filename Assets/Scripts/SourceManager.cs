
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine; 

public class SourceManager : MonoBehaviour
{
    private static readonly SourceManager instance = new SourceManager();
    /*static SourceManager()
    {
    } */

    public static SourceManager Instance
    {
        get { return instance; }
    }

    public void SaveToTxt(List<Node>[] _nodes)
    {
        StringBuilder str = new StringBuilder();
        for (int i = 0; i < _nodes.Length; i++)
        {
            str.Append("FRAME#" + i + "#");
            str.AppendLine();
            for (int j = 0; j < _nodes[i].Count; j++)
            {
                str.Append("(" + _nodes[i][j].Transform.position.x + ";" + _nodes[i][j].Transform.position.y + ")");
            }
            str.AppendLine();
            for (int j = 0; j < _nodes[i].Count; j++)
            {
                str.Append("(" + _nodes[i][j].LineOutIndex + ")");
            }
            str.AppendLine();
        }
        System.IO.File.WriteAllText(@"C:\data.saf", str.ToString());
        Debug.Log("Saved");
    }

    public List<Node>[] LoadFile(GameObject item, Transform parent)
    {
        string[] lines = System.IO.File.ReadAllLines(@"C:\data.saf");
        var _nodes = new List<Node>[(lines.Length)/3];
        int count = 0;
        for (int i = 0; i < lines.Length; i += 3)
        {
            string[] split = lines[i + 1].Split('(', ';', ')')
                .Select(s => s.Trim())
                .Where(s => s != "")
                .ToArray();
            _nodes[count] = new List<Node>();
            for (int j = 0; j < split.Length; j += 2)
            {
                Node node = new Node(item, parent, new Vector2(float.Parse(split[j]), float.Parse(split[j + 1])));
                if (node.gameObject != null)
                {
                    _nodes[count].Add(node);
                    node.gameObject.name = "Node#" + count + ":" + _nodes[count].Count();
                    if (i != 0)
                        node.gameObject.SetActive(false);
                }
            }
            string[] split2 = lines[i + 2].Split('(', ')')
                .Select(s => s.Trim())
                .Where(s => s != "")
                .ToArray();
            for (int j = 0; j < split2.Length; j ++)
            {
                int lineFinish;
                int.TryParse(split2[j], out lineFinish);
                if (lineFinish != 0)
                {
                    _nodes[count][j].SetLineStart(_nodes[count][j].Transform.position);
                    _nodes[count][j].SetLineFinish(lineFinish, _nodes[count][lineFinish].Transform.position);
                }
            }
            count++;
        }
        Debug.Log("Loaded");
        return _nodes;
    }
} 
