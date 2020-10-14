using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DNode {
    public Node node;
    public int dist;
    public DNode prev;

    public DNode( Node _node, int _dist, DNode _prev)
    {
        node = _node;
        dist = _dist;
        prev = _prev;
    }

    public DNode( Node _node)
        : this(_node, int.MaxValue, null)
    {
        //Do Nothing
    }

    public DNode(Node _node, int _dist)
        : this(_node, _dist, null)
    {
        //Do Nothing
    }
}
