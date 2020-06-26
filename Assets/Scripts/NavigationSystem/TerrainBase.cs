using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBase : MonoBehaviour, IQuadTreeObject
{
    public enum TerrainLayer
    {
        Default, Grass, Water, Road, Empty
    }
    static long MAX_ID = 0;
    long id = MAX_ID++;
    [Header("Terrain")]
    [SerializeField] public float radius = 1;
    [SerializeField] public Vector3Int cell;
    [SerializeField] public Vector3 cellPosition;
    //[SerializeField] public WorldTile.TileShape shape;
    [SerializeField] public bool isStatic = false;
    public int SpatialGroup { set; get; }
    public Rect bounds { get; set; }
    public int team = 0;
    public int Layer => team;

    public bool walkable = true;
    public TerrainLayer terrainLayer;

    public void Subscribe()
    {
        bounds = new Rect(this.transform.position.x- radius, this.transform.position.z- radius, radius * 2, radius * 2);
        World.instance.terrain.Insert(this);
        var node = Navigation.current.GetNodeFromWorld(this.transform.position);
        node.Walkable = this.walkable;
        node.Tag = (uint)terrainLayer;
    }
    void OnDisable()
    {
        if (Navigation.current)
        {
            World.instance.terrain.Remove(this, bounds.position);
        }
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
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(this.transform.position, new Vector3(radius * 2, 0, radius * 2));
            Gizmos.DrawWireCube(this.transform.position, new Vector3(radius * 2, 0, radius * 2));
        }
        /*foreach (var n in neighborhood)
        {
            Draw.Gizmos.Hexagon(cellPosition, (1) / World.instance.circularScale, Color.gray, 90);
        }*/
    }
}
