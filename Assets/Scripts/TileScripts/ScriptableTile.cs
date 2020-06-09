using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "ScriptableTile", menuName = "Custom/ScriptableTile", order = 1)]
public class ScriptableTile : Tile
{
    public GameObject ground;
    public GameObject building;
    public GameObject decoration;

    public bool neighbourUpdate = false;
    public bool buildingUpdate = false;

    public bool isTerrain = false;
    public Material optionalMaterial;
    public Sprite optionalSprite;
    public string helperText;
}
