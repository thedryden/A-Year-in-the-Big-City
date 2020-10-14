using UnityEngine;

/// <summary>
/// Custom implmentation of a linked list sorted on distance.
/// </summary>
public class DNodeList {
    /// <summary>
    /// Start of the list. The DistLink with the minDist
    /// </summary>
    private DNodeLinked head;
    /// <summary>
    /// Last item of the list. The DNodeLinked with the maxDist
    /// </summary>
    private DNodeLinked foot;
    /// <summary>
    /// The number of items in the list
    /// </summary>
    public int Length { get; private set; }
    /// <summary>
    /// Array that allows O(1) lookup of any node that is in the list if you know its x and y coord (which are stored in the node)
    /// </summary>
    private DNodeLinked[,] map; //Array that allows quick lookup based on x,y. x,y will match the locations in graph

    /// <summary>
    /// Creates a new linked list, with a sinlge item, and also initalizes map based on the sizes _maxX and _maxY
    /// </summary>
    /// <param name="_node">New head/foot to intailze the list</param>
    /// <param name="_dist">Distance value of the new head / foot</param>
    /// <param name="_maxX">number of columns in the map array</param>
    /// <param name="_maxY">number of rows in the map array</param>
    public DNodeList(DNode _node, int _maxX, int _maxY) {
        head = new DNodeLinked(_node);
        foot = head;
        Length = 1;
        map = new DNodeLinked[_maxX, _maxY];
        map[_node.node.P.X, _node.node.P.Y] = head;
    }

    /// <summary>
    /// Adds a new node to the list, making sure its in the proper sorted order based on the past dist
    /// </summary>
    /// <param name="_node">The node to add</param>
    /// <param name="_dist">The distance from the origin of this node, the node will be placed in sorted order based on this value</param>
    public void Add(DNode _node) {
        DNodeLinked aNode = head;
        DNodeLinked newDist = null;

        //Loop in order, starting from head
        while (aNode != null) {
            //If the new dist is equal to or less than then the value of the current node, insert the new node before the current item
            if (aNode.node.dist >= _node.dist) {
                //Create node
                newDist = new DNodeLinked(_node, aNode.prev, aNode);
                //If we're creating the new head
                if (newDist.prev == null)
                    head = newDist;
                else
                    aNode.prev.next = newDist;
                aNode.prev = newDist;
                break;
            }
            aNode = aNode.next;
        }
        //If this is the new max
        if (aNode == null) {
            newDist = new DNodeLinked(_node, foot, null);
            //If we go here because the list was empty, set head to new item. Otherwise (this is required because if head is null foot is null) set this as the next for foot
            if (head == null)
                head = newDist;
            else
                foot.next = newDist;
            foot = newDist;
        }
        map[_node.node.P.X, _node.node.P.Y] = newDist;
        Length++;
    }

    /// <summary>
    /// Adds a new node as the new foot with an Mathf.Infinate dist. This was created to make the process of initalizing the list with a bunch of infinate dist values faster O(1) vs O(2).
    /// </summary>
    /// <param name="_node">Node to add</param>
    public void AddToFoot( DNode _node ) {
        //Create new DNodeLinked and add it as the foot, moving the existing foot down one position
        DNodeLinked newDist = new DNodeLinked(_node, foot, null);
        foot.next = newDist;
        foot = newDist;
        map[_node.node.P.X, _node.node.P.Y] = newDist;
        Length++;
    }

    /// <summary>
    /// Returns the min node (head)
    /// </summary>
    /// <returns>The min node (head)</returns>
    public DNode GetMin() {
        return head.node;
    }

    /// <summary>
    /// Removes the min node (head)
    /// </summary>
    public void RemoveMin()
    {
        if (head == null)
            return;
        DNodeLinked temp = head;
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
    public DNode PopMin() {
        if (head == null)
            return null;
        DNode output = head.node;
        RemoveMin();
        return output;
    }

    public bool ProcessNode(DNode _node)
    {
        DNodeLinked oldNode = map[_node.node.P.X, _node.node.P.Y];
        if(oldNode == null)
        {
            Add(_node);
            return true;
        }
        else if( oldNode.node.dist > _node.dist)
        {
            UpdateNode(_node);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Takes a node, that must be in this list, and changes its distance and then readds it to the list to update the sort order.
    /// </summary>
    /// <param name="_node">The node who's distance you wish to udpate</param>
    public void UpdateNode(DNode _node) {
        //If node is null do nothing
        if (_node == null)
            return;
        //If node is not in the map, do nothing
        DNodeLinked node = map[_node.node.P.X, _node.node.P.Y];
        if (node == null)
            return;


        /* General logic is to cut this node out, then add it back in.
         * Possiblites: I'm head and foot, set net _dist and done.
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
}