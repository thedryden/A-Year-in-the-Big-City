using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FNodeList {
    /// <summary>
    /// Start of the list. The DistLink with the minDist
    /// </summary>
    private FNodeLinked head;
    /// <summary>
    /// Last item of the list. The FNodeLinked with the maxDist
    /// </summary>
    private FNodeLinked foot;
    /// <summary>
    /// The number of items in the list
    /// </summary>
    public int Length { get; private set; }
    /// <summary>
    /// Array that allows O(1) lookup of any node that is in the list if you know its x and y coord (which are stored in the node)
    /// </summary>
    private FNodeLinked[,] map; //Array that allows quick lookup based on x,y. x,y will match the locations in graph

    /// <summary>
    /// Creates a new linked list, with a sinlge item, and also initalizes map based on the sizes _maxX and _maxY
    /// </summary>
    /// <param name="_node">New head/foot to intailze the list</param>
    /// <param name="_maxX">number of columns in the map array</param>
    /// <param name="_maxY">number of rows in the map array</param>
    public FNodeList(FNode _node, int _maxX, int _maxY)
    {
        head = new FNodeLinked(_node);
        foot = head;
        Length = 1;
        map = new FNodeLinked[_maxX, _maxY];
        map[_node.node.P.X, _node.node.P.Y] = head;
    }

    /// <summary>
    /// Adds a new node to the list, making sure its in the proper sorted order
    /// </summary>
    /// <param name="_node">The node to add</param>
    public void Add(FNode _node)
    {
        FNodeLinked aDist = head;
        FNodeLinked newDist = null;

        //Loop in order, starting from head
        while (aDist != null)
        {
            //If the new F is less than the current F OR the F's are equal and the new H is less than the current one
            if (aDist.node.F > _node.F || (aDist.node.F == _node.F && aDist.node.H >= _node.H ))
            {
                //Create node
                newDist = new FNodeLinked(_node, aDist.prev, aDist);
                //If we're creating the new head
                if (newDist.prev == null)
                    head = newDist;
                else
                    aDist.prev.next = newDist;
                aDist.prev = newDist;
                break;
            }
            aDist = aDist.next;
        }
        //If this is the new max
        if (aDist == null)
        {
            newDist = new FNodeLinked(_node, foot, null);
            if(head == null)
                head = newDist;
            else
                foot.next = newDist;
            foot = newDist;
        }
        map[_node.node.P.X, _node.node.P.Y] = newDist;
        Length++;
    }

    public bool ProcessNode( FNode _node)
    {
        FNodeLinked oldNode = map[_node.node.P.X, _node.node.P.Y];
        if (oldNode == null)
        {
            Add(_node);
            return true;
        }
        else if(oldNode.node.F > _node.F)
        {
            UpdateF(_node);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns the min node (head)
    /// </summary>
    /// <returns>The min node (head)</returns>
    public FNode GetMin()
    {
        return head.node;
    }

    /// <summary>
    /// Removes the min node (head)
    /// </summary>
    public void RemoveMin()
    {
        if (head == null)
            return;
        FNodeLinked temp = head;
        head = head.next;
        if (head != null)
            head.prev = null;
        else
            foot = null;
        map[temp.node.node.P.X, temp.node.node.P.Y] = null;
        Length--;
    }

    /// <summary>
    /// Returns the min node (head) and then removes it.
    /// </summary>
    /// <returns>The min node (head)</returns>
    public FNode PopMin()
    {
        if (head == null)
            return null;
        FNode output = head.node;
        RemoveMin();
        return output;
    }

    /// <summary>
    /// Takes a node, that must be in this list, and changes its distance and then readds it to the list to update the sort order.
    /// </summary>
    /// <param name="_node">The node who's distance you wish to udpate</param>
    public void UpdateF(FNode _node)
    {
        //If node is null do nothing
        if (_node == null)
            return;
        //If node is not in the map, do nothing
        FNodeLinked node = map[_node.node.P.X, _node.node.P.Y];
        if (node == null)
            return;

         
        /* General logic is to cut this node out, then add it back in.
         * Possiblites: I'm head and foot, do nothing.
        * I'm head but not foot, set next to head and remove prev from head
        * I'm foot but not head, set prev to foot and remove next for foot
        * I'm neither head nor foot. set prev of my next to my prev, set next of my prev to my next
        */
        if (node.prev == null && node.next == null)
        {
            node.node = _node;
            return;
        }
        else if (node.prev == null)
        {
            head = node.next;
            head.prev = null;
        }
        else if (node.next == null)
        {
            foot = node.prev;
            foot.next = null;
        }
        else
        {
            node.prev.next = node.next;
            node.next.prev = node.prev;
        }
        Length--;
        Add(node.node);
    }

    public override string ToString()
    {
        string output = "";
        FNodeLinked aNode = head;

        while( aNode != null)
        {
            output += aNode.node + "\n";
            aNode = aNode.next;
        }
        return output;
    }
}
