using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingThroughItem {
    public Coord P { get; private set; }
    public Actor myActor;
    public int mt;

    public MovingThroughItem( Coord _p)
        : this(_p,null,0)
    {
        //Do nothing
    }

    public MovingThroughItem( Coord _p, Actor _actor, int _mt)
    {
        P = _p.Copy().SetReadOnly();
        myActor = _actor;
        mt = _mt;
    }

    public void Set( Actor actor, int _mt)
    {
        myActor = actor;
        mt = _mt;
    }

    public void Set(MovingThroughItem _mt)
    {
        myActor = _mt.myActor;
        mt = _mt.mt;
    }

    public void Empty()
    {
        myActor = null;
        mt = 0;
    }

    public override bool Equals(object obj)
    {
        var item = obj as MovingThroughItem;
        return item != null &&
               EqualityComparer<Coord>.Default.Equals(P, item.P);
    }

    public override int GetHashCode()
    {
        return 540953319 + EqualityComparer<Coord>.Default.GetHashCode(P);
    }

    public static bool operator ==(MovingThroughItem _one, MovingThroughItem _two)
    {
        if (object.ReferenceEquals(_one, null) && object.ReferenceEquals(_two, null))
            return true;
        if (object.ReferenceEquals(_one, null) || object.ReferenceEquals(_two, null))
            return false;
        return _one.P == _two.P;
    }

    public static bool operator !=(MovingThroughItem _one, MovingThroughItem _two)
    {
        return !(_one == _two);
    }
}
