using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores your own x and y position as well as a referacne to all of the nodes that are walkable from your current location.
/// </summary>
public class Node {
    /// <summary>
    /// D = cost to move 1 tile. DD cost to move diagonally.
    /// D_CAL_CONST should always equal (Node.DD - 2 * Node.D)
    /// </summary>
    public static int D = 10, DD = 14, D_CALC_CONST = -1;
    
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
    /// This is NOT a check for speed, so please don't violate this rule.
    /// This will also not check to make sure the move is on the graph, and thus can return an error if you pass a _start or _end that is not.
    /// </summary>
    /// <param name="_tiles">The tiles object both nodes are on</param>
    /// <param name="_start">The node your starting from</param>
    /// <param name="_end">The node your traveling to</param>
    /// <returns>The cost to move between _start and _end on tiles</returns>
    public static int DistanceBetween( Tiles _tiles, Node _start, Node _end )
    {
        //Set start value based on moving diagonally or not
        int distanceTo = (Coord.AreDiagonal(_start.P, _end.P)) ? DD : D;
        //Double the distance if the terain of the tile you'd be entering is difficult
        if (_tiles.GetTile(_end.P).Type == TileCell.TileType.Difficult)
            distanceTo *= 2;

        return distanceTo;
    }

    /// <summary>
    /// If the move is weighted more than the baseline (terain is difficult) then return the ammont the move is more difficult than baseline.
    /// I.E. if move cost is baseline move cost is 10 and this move will cost 20 this will return 10.
    /// This function assume your never traveling beyond one "step" on the graph, i.e. both of the passed nodes should contain the other node in their edges list.
    /// This is NOT a check for speed, so please don't violate this rule.
    /// This will also not check to make sure the move is on the graph, and thus can return an error if you pass a _start or _end that is not.
    /// </summary>
    /// <param name="_tiles">The tiles object both nodes are on</param>
    /// <param name="_start">The node your starting from</param>
    /// <param name="_end">The node your traveling to</param>
    /// <returns></returns>
    public static int WeightOfMove(Tiles _tiles, Node _start, Node _end)
    {
        if (_tiles.GetTile(_end.P).Type == TileCell.TileType.Difficult)
            return (Coord.AreDiagonal(_start.P, _end.P)) ? DD : D;

        return 0;
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
