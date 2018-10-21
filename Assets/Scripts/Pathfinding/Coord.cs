using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple class to store x and y coordinantes.
/// </summary>
public class Coord {
    /// <summary>
    /// If true, set is effectively turned off
    /// </summary>
    private bool readOnly = true;
    /// <summary>
    /// The stored X and Y values
    /// </summary>
    private int _x, _y;
    public int X
    {
        get { return _x; }
        set{
            if (!readOnly)
                _x = value;
        }
    }
    public int Y
    {
        get { return _y; }
        set
        {
            if (!readOnly)
                _y = value;
        }
    }

    /// <summary>
    /// Constructor. Takes a x and y and that lets you set _readOnly, thus turning it off.
    /// </summary>
    /// <param name="_x">The value for X</param>
    /// <param name="_y">The value for Y</param>
    /// <param name="_readyOnly">If true then X and Y can't be changed after construction</param>
    public Coord(int _x, int _y, bool _readyOnly)
    {
        this._x = _x;
        this._y = _y;
        readOnly = _readyOnly;
    }

    /// <summary>
    /// Constructor. Takes a node, that contains its own Coords, and uses it to set the x and y of the new Coord. Also has a _readyOnly, thus allowing you to turn it off.
    /// </summary>
    /// <param name="_node">A node from which X and Y will be extracted/</param>
    /// <param name="_readyOnly">If true then X and Y can't be changed after construction</param>
    public Coord(Node _node, bool _readyOnly)
        : this(_node.P.X, _node.P.Y, _readyOnly)
    {
        //Do nothing
    }

    /// <summary>
    /// Constructor. If you use this constructor then the result be readOnly.
    /// </summary>
    /// <param name="_x">The value for X</param>
    /// <param name="_y">The value for Y</param>
    public Coord( int _x, int _y)
        : this(_x, _y, true)
    {
        //Do nothing
    }

    /// <summary>
    /// Constructor. If you use this constructor then the result be readOnly.
    /// </summary>
    /// <param name="_node">A node from which X and Y will be extracted/</param>
    public Coord( Node _node)
        : this(_node.P.X, _node.P.Y, true)
    {
        //Do nothing
    }

    /// <summary>
    /// Returns a copy of this object.
    /// </summary>
    /// <returns>A copy of this object</returns>
    public Coord Copy()
    {
        return new Coord(_x, _y, readOnly);
    }

    /// <summary>
    /// This can be used to set a non-read only Coord to readOnly. Once set to readOnly it can't be reversed.
    /// </summary>
    /// <returns></returns>
    public Coord SetReadOnly()
    {
        readOnly = true;
        return this;
    }

    /// <summary>
    /// Retruns a new Coords set to 0,0 that can be edited.
    /// </summary>
    /// <returns>Editabled Coord set to 0,0</returns>
    public static Coord Zero()
    {
        return new Coord(0, 0, false);
    }

    public override bool Equals(object obj)
    {
        var coord = obj as Coord;
        return coord != null &&
               _x == coord._x && _y == coord._y;
    }

    public override int GetHashCode()
    {
        var hashCode = 979593255;
        hashCode = hashCode * -1521134295 + _x.GetHashCode();
        hashCode = hashCode * -1521134295 + _y.GetHashCode();
        return hashCode;
    }

    public static bool operator == ( Coord _one, Coord _two)
    {
        if (object.ReferenceEquals(_one, null) && object.ReferenceEquals(_two, null))
            return true;
        if (object.ReferenceEquals(_one, null) || object.ReferenceEquals(_two, null))
            return false;
        return _one._x == _two._x && _one._y == _two._y;
    }

    public static bool operator !=(Coord _one, Coord _two)
    {
        return !(_one == _two);
    }

    public static Coord operator + ( Coord _one, Coord _two)
    {
        return new Coord(_one._x + _two._x, _one._y + _two._y);
    }

    public static Coord operator - (Coord _one, Coord _two)
    {
        return new Coord(_one._x - _two._x, _one._y - _two._y);
    }

    public override string ToString()
    {
        return "( " + _x + ", " + _y + " )";
    }
}
