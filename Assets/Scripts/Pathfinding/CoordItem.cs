using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single item in a CoordLinked linked list.
/// </summary>
public class CoordItem {
    public Coord coord;
    public CoordItem next;

    public CoordItem(Node _node)
    {
        coord = new Coord(_node);
    }

    public CoordItem(Node _node, CoordItem _next)
    {
        coord = new Coord(_node);
        next = _next;
    }

    public CoordItem( Coord _coord )
    {
        coord = _coord;
    }

    public CoordItem(Coord _coord, CoordItem _next)
    {
        coord = _coord;
        next = _next;
    }
}
