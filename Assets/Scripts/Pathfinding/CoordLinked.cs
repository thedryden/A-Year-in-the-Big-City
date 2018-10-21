using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fist In Last Out (FILO) Linked list of Coords used to hold paths.
/// </summary>
public class CoordLinked {
    /// <summary>
    /// First and last coords in the list. Head is next location, foot is desination
    /// </summary>
    private CoordItem head, foot;
    /// <summary>
    /// Used to allow iteratoring over the linked list
    /// </summary>
    private CoordItem iterator;
    /// <summary>
    /// Holds the number of items in the list
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Constructor that takes the first node, that will be transformed into a coords
    /// </summary>
    /// <param name="_head">The first item in the list</param>
    public CoordLinked( Node _head)
    {
        head = new CoordItem( _head );
        foot = head;
        Length = 1;
    }

    /// <summary>
    /// Creates a new empty CoordLinked
    /// </summary>
    public CoordLinked()
    {
        Length = 0;
    }

    /// <summary>
    /// Adds a new item to the top of the list
    /// </summary>
    /// <param name="_node">The new node to add to the head of this list</param>
    public void Add( Node _node )
    {
        head = new CoordItem(_node, head);
        Length++;
    }

    /// <summary>
    /// Adds a new list to the top of this list
    /// </summary>
    /// <param name="_list">The list to prepend to the list</param>
    public void Add( CoordLinked _list)
    {
        _list.foot = head;
        head = _list.head;
        Length += _list.Length;
    }

    /// <summary>
    /// Adds an item to the end, rather than the begging, of the list.
    /// </summary>
    /// <param name="_node">Item to add to the end of the list</param>
    public void Append( Node _node)
    {
        CoordItem newItem = new CoordItem(_node);
        foot.next = newItem;
        foot = newItem;
    }

    /// <summary>
    /// Adds a list to the end, rather than the begging, of the list.
    /// </summary>
    /// <param name="_list">List to add to the end of the list</param>
    public void Append( CoordLinked _list)
    {
        foot.next = _list.head;
        foot = _list.foot;
        Length += _list.Length;
    }

    /// <summary>
    /// Retruns the Coord item at the top of the list, without removing it.
    /// </summary>
    /// <returns>The Coord at the top of the list</returns>
    public Coord GetHead()
    {
        return head.coord;
    }

    /// <summary>
    /// Returns the Coord item at the end of the list, without removing it.
    /// </summary>
    /// <returns></returns>
    public Coord GetFoot()
    {
        return foot.coord;
    }

    /// <summary>
    /// Starts an iterator to loop over the list, return the first item in the list (the head).
    /// </summary>
    /// <returns>The first item in the lsit, the head.</returns>
    public Coord StartIteration()
    {
        //If the head is null (which should mean the list is empty), return null as we've reached the "end" of the list
        if (head == null)
            return null;
        iterator = head;
        return iterator.coord;
    }

    /// <summary>
    /// Returns the next item in the list list. If null is returned then you have reached the end of the list.
    /// </summary>
    /// <returns>The next item in the list</returns>
    public Coord Next()
    {
        //If the iterator is null (which should mean the list is empty), return null as we've reached the "end" of the list
        if (iterator == null)
            return null;
        //If the iterator has no next item in the list, return null to indicate we're reach the end of the list
        if (iterator.next == null)
            return null;

        iterator = iterator.next;
        return iterator.coord;
    }

    /// <summary>
    /// Retunrs the next item in the list and then removes it. If the list is empty this will return null 
    /// </summary>
    /// <returns>The next item in the list, or null if the list is empty. Once a Coor is returned by this method it is removed from the list.</returns>
    public Coord Pop()
    {
        //If the list is empty, return null
        if (head == null || Length <= 0)
            return null;
        //Get head so we can return it at the end, then set head to head.next
        CoordItem temp = head;
        head = temp.next;
        Length--;//Manage length
        //If head is null then the list should now be empty, set foot to null as well to finish the process.
        if (head == null)
            foot = null;
        return temp.coord;
    }
}
