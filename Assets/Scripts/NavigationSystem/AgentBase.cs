using Pathfinding.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentBase : MonoBehaviour, IQuadTreeObject
{
    static long MAX_ID = 0;
    long id = MAX_ID++;
    [Header("Actor")]
    [SerializeField] public float radius = 1;
    [SerializeField] public Vector3Int cell;
    [SerializeField] public Vector3 cellPosition;
    //[SerializeField] public WorldTile.TileShape shape;
    [SerializeField] public bool isStatic = false;
    public int SpatialGroup { set; get; }

    public Vector2 position2D;
    public Rect bounds { get; set; }
    public int team = 0;
    public int Layer => team;


    public void Subscribe()
    {
        position2D.x = this.transform.position.x;
        position2D.y = this.transform.position.z;
        bounds = new Rect(this.transform.position.x-radius,this.transform.position.z-radius,radius*2,radius*2);
        World.instance.agents.Insert(this);
    }
    void OnDisable()
    {
        if (Navigation.current)
        {
            World.instance.agents.Remove(this, bounds.position);
        }
    }

    public void OnPostMove()
    {
        Vector3Int newCell = World.instance.WorldToCell(this.transform.position);
        if (newCell != cell)
        {
            cell = newCell;
            cellPosition = World.instance.CellToWorld(cell);
            //neighborhood = World.instance.GetNeighborhood(this);
            var previousPosition2D = position2D;
            position2D = new Vector2(cellPosition.x, cellPosition.z);
            World.instance.UpdateElement(this, previousPosition2D, position2D);
        }
    }
    public void Destroy()
    {
        World.instance.RemoveElement(this);
        GameObject.Destroy(this.gameObject);
    }
    void OnDrawGizmos2()
    {
       /* if (shape == WorldTile.TileShape.Circular)
        {
            Draw.Gizmos.CircleXZ(this.transform.position, radius / World.instance.circularScale, Color.white);
        }
        else if (shape == WorldTile.TileShape.Hexagonal)
        {
            Draw.Gizmos.Hexagon(this.transform.position, radius / World.instance.circularScale, Color.white, 90);
        }
        else*/
        /*{
            Gizmos.color = Color.gray;
            Gizmos.DrawCube(this.transform.position, new Vector3(radius * 2, 0, radius * 2));
            Gizmos.DrawWireCube(this.transform.position, new Vector3(radius * 2, 0, radius * 2));
        }*/
        /*foreach (var n in neighborhood)
        {
            Draw.Gizmos.Hexagon(cellPosition, (1) / World.instance.circularScale, Color.gray, 90);
        }*/
    }
    /*
    public Selectable selectable;

    public int team = 0;
    public List<Vector3Int> footprint;
    public List<Vector3Int> neighborhood;
    protected virtual void Start()
    {
        cell = World.instance.WorldToCell(this.transform.position);
        cellPosition = World.instance.CellToWorld(cell);
        Position = new Vector2(cellPosition.x, cellPosition.z);
        neighborhood = World.instance.GetNeighborhood(this);
        OnCreate();
    }
    protected virtual void OnValidate()
    {
        if (selectable)
        {
            selectable.OnValidate();
        }
        else
        {
            Debug.LogWarning(this.name + " has no selectable");
        }
        if (!Application.isPlaying && World.instance)
        {
            cell = World.instance.WorldToCell(this.transform.position);
            this.transform.position = World.instance.CellToWorld(cell);
            neighborhood = World.instance.GetNeighborhood(this);
        }
    }


    public int GetLayer()
    {
        return team;
    }

    protected virtual void OnCreate()
    {
        World.instance.RegisterElement(this);
        //OnPreMove();
    }
    public void OnPostMove()
    {
        Vector3Int newCell = World.instance.WorldToCell(this.transform.position);
        if (newCell != cell)
        {
            cell = newCell;
            cellPosition = World.instance.CellToWorld(cell);
            neighborhood = World.instance.GetNeighborhood(this);
            var previousPosition = Position;
            Position = new Vector2(cellPosition.x, cellPosition.z);
            World.instance.UpdateElement(this, previousPosition, Position);
        }
    }

   

    protected void OnDrawGizmos()
    {
        if (shape == WorldTile.TileShape.Circular)
        {
            Draw.Gizmos.CircleXZ(this.transform.position, radius / World.instance.circularScale, Color.white);
        }
        else if(shape == WorldTile.TileShape.Hexagonal)
        {
            Draw.Gizmos.Hexagon(this.transform.position, radius / World.instance.circularScale, Color.white,90);
        }
        else
        {
            Gizmos.DrawWireCube(this.transform.position, new Vector3(radius * 2, 0, radius * 2)/World.instance.squareScale);
        }
        foreach(var n in neighborhood)
        {
            Draw.Gizmos.Hexagon(cellPosition, (1) / World.instance.circularScale, Color.gray, 90);
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        Actor c = obj as Actor;
        if (c == null)
            return false;
        return id == c.id;
    }
    public bool Equals(Actor c)
    {
        if ((object)c == null)
            return false;
        return id == c.id;
    }

    public override int GetHashCode()
    {
        var hashCode = 1550466422;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + id.GetHashCode();
        return hashCode;
    }*/
}
