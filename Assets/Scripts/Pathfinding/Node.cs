using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores your own x and y position as well as a referacne to all of the nodes that are walkable from your current location.
/// </summary>
public class Node {
    //List of walkable nodes from your current location
    public List<Node> edges;
    //The coordinates in graph of this node
    public Coord P { get; private set; }

    //Creates a new node
    public Node(int _x, int _y)
    {
        P = new Coord( _x, _y, true );
        edges = new List<Node>();
    }

    public Node(Coord _point)
    {
        P = _point.Copy().SetReadOnly();
        edges = new List<Node>();
    }

    /// <summary>
    /// Static function used to calculate the distance between two nodes on the same, passed, tiles object.
    /// This function assume your never traveling beyond one "step" on the graph, i.e. both of the passed nodes should contain the other node in their edges list.
    /// This is NOT check for speed, so please don't violate this rule.
    /// </summary>
    /// <param name="tiles">The tiles object both nodes are on</param>
    /// <param name="_start">The node your starting from</param>
    /// <param name="_end">The node your traveling to</param>
    /// <returns></returns>
    public static float DistanceBetween( Tiles tiles, Node _start, Node _end )
    {
        //All distances start as 1.
        float distanceTo = 1;
        //Add another 1 (doubling the distance) if the terain of the tile you'd be entering is difficult
        if (tiles.tiles[_end.P.X, _end.P.Y].Type == TileCell.TileType.Difficult)
            distanceTo += 1f;
        //If moving diagonally, make distance as long as possible without stoping diagonal moves
        if (_start.P.X != _end.P.X && _start.P.Y != _end.P.Y)
            distanceTo += .9f;

        return distanceTo;
    }

    public static bool operator ==(Node _one, Node _two)
    {
        if (object.ReferenceEquals(_one, null) && object.ReferenceEquals(_two, null))
            return true;
        if (object.ReferenceEquals(_one, null) || object.ReferenceEquals(_two, null))
            return false;
        return _one.P == _two.P;
    }

    public static bool operator !=(Node _one, Node _two)
    {
        return !(_one == _two);
    }

    public override bool Equals(object obj)
    {
        var node = obj as Node;
        return node != null &&
               EqualityComparer<Coord>.Default.Equals(P, node.P);
    }

    public override int GetHashCode()
    {
        return 540953319 + EqualityComparer<Coord>.Default.GetHashCode(P);
    }
}
