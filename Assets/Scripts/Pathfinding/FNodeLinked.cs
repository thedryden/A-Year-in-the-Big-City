using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FNodeLinked {
    public FNode node;
    public FNodeLinked prev, next;

    public FNodeLinked(FNode _node)
    {
        node = _node;
        prev = null;
        next = null;
    }

    public FNodeLinked(Node _node, int _g, Coord _end)
    {
        node = new FNode( _node, _g, _end );
        prev = null;
        next = null;
    }

    public FNodeLinked(FNode _node, FNodeLinked _prev, FNodeLinked _next)
    {
        node = _node;
        prev = _prev;
        next = _next;
    }

    public FNodeLinked(Node _node, int _g, Coord _end, FNodeLinked _prev, FNodeLinked _next)
    {
        node = new FNode(_node, _g, _end);
        prev = _prev;
        next = _next;
    }
}
