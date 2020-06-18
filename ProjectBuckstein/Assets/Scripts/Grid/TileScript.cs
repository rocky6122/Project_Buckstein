////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Project_Buckstein | Programmed and Designed by Parker Staszkiewicz, Anthony Pascone, and John Imgrund (c) 2018 //
//                                                                                                                //
//  TileScript : class | Written by Anthony Pascone and Parker Staszkiewicz                                       //
//      
//       
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider))]
public class TileScript : MonoBehaviour
{
    public Color selectedUnitColor;
    public Color selectedEnemyColor;
    public Color standardColor;
    public Color pathColor;

    public SpriteRenderer sr;
    private BoxCollider boxCol;

    private Node[] pathToSelf;
    private int acceptablePathLegnth;

	public int xLoc;
	public int zLoc;
    public Vector3 worldPos;

    public Node thisNode;

    private bool canBeHovered = true;

    public void Initializer(Vector3 pos, int x, int z)
    {
        worldPos = pos;
        xLoc = x;
        zLoc = z;
        thisNode = new Node(this, pos, x, z);
        sr = GetComponent<SpriteRenderer>();
        boxCol = GetComponent<BoxCollider>();
    }

    public void PrepareForPathing(int pathLength)
    {
        pathToSelf = null;
        acceptablePathLegnth = pathLength;
    }

    public void StorePath(Node[] path, bool success, bool diagonal)
    {
        if (success && path.Length <= acceptablePathLegnth)
        {
            thisNode.diagonal = diagonal;
            pathToSelf = path;
            RenderOnScreen(true);
            GridManagerScript.instance.AddTileToWalkable(this);
        }
    }

    public Node[] GetPath()
    {
        return pathToSelf;
    }

    public void RenderOnScreen(bool render)
    {
        sr.enabled = render;

        boxCol.enabled = render;

        if (render)
        {
            GridManagerScript.instance.AddTileToWalkable(this);
        }
    }

    public void SetOccupy(bool occupied)
    {
        thisNode.SetOccupied(occupied);

    }

    public void Select(bool select, bool player = true)
    {
        if (player)
        {
            sr.color = select ? selectedUnitColor : standardColor;
        }
        else
        {
            sr.color = select ? selectedEnemyColor : standardColor;
        }
    }

    public void SetObstacle(bool obstacle)
    {
        thisNode.SetObstacle(obstacle);
    }

    public bool IsObstacle()
    {
        return thisNode.GetObstacle();
    }

    public Node GetNode()
    {
        return thisNode;
    }

    void OnMouseDown()
    {
        if (canBeHovered)
        {
            PlayerUnitManager.instance.MoveUnit(thisNode);
        }
    }

    private void OnMouseEnter()
    {
        if (canBeHovered)
        {
            ShowPath();
        }
    }

    private void OnMouseExit()
    {
        HidePath();
    }

    private void ShowPath()
    {
        if (pathToSelf == null)
        {
            return;
        }

        foreach (Node n in pathToSelf)
        {
            n.parentTile.sr.color = pathColor;
        }
    }

    private void HidePath()
    {
        if (pathToSelf == null)
        {
            return;
        }

        foreach (Node n in pathToSelf)
        {
            n.parentTile.sr.color = standardColor;
        }
    }

    public void ToggleHover(bool hov)
    {
        canBeHovered = hov;
    }
}
