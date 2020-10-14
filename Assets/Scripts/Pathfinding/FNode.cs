using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FNode {
    private int _g;
    public int F { get; private set; }
    public int G
    {
        get { return _g; }
        set {
            _g = value;
            F = _g + H;
        }
    }
    public int H { get; private set; }
    public Node node;
    public FNode prev;

    public FNode( Node _node, int _g, Coord _end, int _extraWeight)
    {
        node = _node;
        this._g = _g;
        SetH(_end, _extraWeight);
    }

    public FNode(Node _node, int _g, Coord _end)
       : this(_node, _g, _end, 0)
    {
        //Do nothing
    }

    public FNode( Node _node, int _g, Node _end)
        :this(_node, _g, _end.P, 0)
    {
        //Do nothing
    }

    public FNode(Node _node, int _g, Node _end, int _extraWeight)
        : this(_node, _g, _end.P, _extraWeight)
    {
        //Do nothing
    }

    public void SetH(Coord _end)
    {
        SetH(_end, 0);
    }

    public void SetH(Coord _end, int _extraWeight)
    {
        int dx = UDF.Abs(node.P.X - _end.X);
        int dy = UDF.Abs(node.P.Y - _end.Y);
        H = Node.D * (dx + dy) + Node.D_CALC_CONST * UDF.Min(dx, dy) + _extraWeight;
        F = _g + H;
    }

    public static bool operator ==(FNode _one, FNode _two)
    {
        if (object.ReferenceEquals(_one, null) && object.ReferenceEquals(_two, null))
            return true;
        if (object.ReferenceEquals(_one, null) || object.ReferenceEquals(_two, null))
            return false;
        return _one.node == _two.node;
    }

    public static bool operator !=(FNode _one, FNode _two)
    {
        return !(_one == _two);
    }

    public override bool Equals(object obj)
    {
        var node = obj as FNode;
        return node != null &&
               EqualityComparer<Node>.Default.Equals(this.node, node.node);
    }

    public override int GetHashCode()
    {
        return -231681771 + EqualityComparer<Node>.Default.GetHashCode(node);
    }

    public override string ToString()
    {
        return node.P + " F:" + F + ", H:" + H + ", G:" + G;
    }
}
