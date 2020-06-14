using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class ConstructionCamera : MonoBehaviour
{
    [Header("Interfaces")]
    public bool activated;
    public bool quit;
    public bool mouseControl = true;
    public bool instantConstruct = false;

    [Header("Linking & entry variables")]
    public KeyCode keyMode;
    public KeyCode keyLeft;
    public KeyCode keyRight;
    public CameraController trackballController;
    public GameObject constructionUI;
    public GameObject destroyInteractor;
    public MapModifier modifier;
    public GameObject currentObject;
    private MeshRenderer currentRenderer;
    public ConstructionTemplate currentTemplate;
    public Material buildingMaterial;
    public Material okMaterial;
    public Material nokMaterial;
    public AudioClip buildingOk;
    public AudioClip terrainOk;
    public float scavengeRatio = 0.5f;

    [Header("Control parameters")]
    public float speed = 4f;
    public float scrollSpeed = 1f;
    public int borderThickness = 10;
    public float limit = 20f;
    public Vector2 distanceLimit;

    private float height = 30f;
    private bool lastActivated;
    private Vector3 entryPosition;
    private Quaternion entryRotation;

    [Header("UI linking")]
    public Transform iconContainer;
    public BuildingIconTemplate template;
    public Text helperText;
    public UIHandler uihandler;
    public ConstructionHelper helper;

    [Header("Debug")]
    private EventSystem eventsystem;
    private RaycastHit[] scanResults = new RaycastHit[20];
    private Vector3Int prevTerrainBrushTile;
    private GameObject ocludedTerrainTile;

    private void Start()
    {
        lastActivated = !activated;
        eventsystem = (EventSystem)FindObjectOfType(typeof(EventSystem));
        if (eventsystem == null)
            Debug.LogError("No event system in scene");
        helper.transform.parent = null;
        helper.SetKeys(keyLeft, keyRight);
    }


    private void Update()
    {
        // standart stuff
        if (Input.GetKeyDown(keyMode))
            activated = true;
        helper.gameObject.SetActive(uihandler.toolName != "terrain");
        if (lastActivated != activated)
        {
            if (activated)
            {
                entryPosition = transform.position;
                entryRotation = transform.rotation;
                transform.position = new Vector3(trackballController.target.position.x, height, trackballController.target.position.z);
                currentObject = null;
            }
            trackballController.enabled = !activated;
            constructionUI.SetActive(activated);
        }
        if (!activated)
        {
            lastActivated = activated;
            return;
        }

        // position update
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.Z) || (mouseControl && Input.mousePosition.y >= Screen.height - borderThickness))
            direction = new Vector3(0, 0, 1);
        else if (Input.GetKey(KeyCode.S) || (mouseControl && Input.mousePosition.y <= borderThickness))
            direction = new Vector3(0, 0, -1);
        if (Input.GetKey(KeyCode.D) || (mouseControl && Input.mousePosition.x >= Screen.width - borderThickness))
            direction += new Vector3(1, 0, 0);
        else if (Input.GetKey(KeyCode.Q) || (mouseControl && Input.mousePosition.x <= borderThickness))
            direction += new Vector3(-1, 0, 0);
        direction.Normalize();

        Vector3 p = transform.position + speed * direction;
        p.x = Mathf.Clamp(p.x, trackballController.target.position.x - limit, trackballController.target.position.x + limit);
        p.z = Mathf.Clamp(p.z, trackballController.target.position.z - limit, trackballController.target.position.z + limit);

        if (!eventsystem.IsPointerOverGameObject())
            height = Mathf.Clamp(height - scrollSpeed * Input.GetAxis("Mouse ScrollWheel"), distanceLimit.x, distanceLimit.y);
        p.y = height;
        transform.position = p;
        transform.forward = -Vector3.up;

        // raycast
        if (!eventsystem.IsPointerOverGameObject() && !uihandler.helpVideo.activeSelf)
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50f, 1 << LayerMask.NameToLayer("Ground")))
            {

            }

            /*RaycastHit hit;
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50f, 1 << LayerMask.NameToLayer("Ground")))
            {
                Vector3 pointing = modifier.GetTileCenter(modifier.WorldToCell(hit.point));


                if (uihandler.toolName == "building")
                {
                    if(currentObject)
                    {
                        currentTemplate.meshFilter.sharedMesh = currentTemplate.preview;
                        helper.transform.localScale = Vector3.one;
                        Vector3 s = currentTemplate.colliderSize;
                        if (s.x > 4f || s.y > 4f)
                        {
                            pointing += new Vector3(2f, 0, 2f);
                            helper.transform.localScale *= 1.5f;
                        }

                        p = new Vector3(pointing.x, 0, pointing.z);
                        currentObject.transform.position = p;
                        helper.mode = currentObject.name.Contains("Wall") ? 0 : 1;

                        Vector3 deltaAngle = Vector3.zero;
                        if (Input.GetKeyDown(keyLeft))
                        {
                            deltaAngle.y = -90;
                            helper.orientationAtrigger();
                        }
                        else if (Input.GetKeyDown(keyRight))
                        {
                            deltaAngle.y = 90;
                            helper.orientationBtrigger();
                        }
                        currentObject.transform.localEulerAngles += deltaAngle;
                        helper.transform.position = currentObject.transform.position;
                        helper.transform.rotation = currentObject.transform.rotation;
                        
                        int scan = Physics.BoxCastNonAlloc(currentObject.transform.position + new Vector3(0, 0.49f * s.z, 0), 0.49f * new Vector3(s.x, s.z, s.y), Vector3.up,
                            scanResults, Quaternion.identity, 0.2f, 1 << LayerMask.NameToLayer("Default"));
                        currentRenderer.material = scan <= 1 ? okMaterial : nokMaterial;
                        
                        if (Input.GetMouseButtonDown(0))
                        {
                            if(scan <= 1)
                            {
                                if(instantConstruct || currentTemplate.instantConstruct)
                                {
                                    if(currentObject.name.Contains("Wall"))
                                    {
                                        List<Vector3> positions = new List<Vector3>();
                                        List<GameObject> tileList = Map.Instance.SearchTilesGameObject(currentObject.transform.position, 4f);
                                        foreach (GameObject go2 in tileList)
                                            positions.Add(go2.transform.position);
                                        Map.Instance.PlaceTiles(positions, tileList, currentTemplate.tileInitializerOption);
                                    }
                                    else
                                    {
                                        GameObject go = currentTemplate.Finish();
                                        if(go)
                                            go.name = currentObject.name;
                                    }
                                }
                                else
                                {
                                    GameObject go = Instantiate(currentObject);
                                    go.name = currentObject.name;
                                    //go.transform.parent = map.buildingsContainer.transform;
                                    go.transform.position = currentObject.transform.position;
                                    go.transform.rotation = currentObject.transform.rotation;
                                    go.transform.Find("mesh").GetComponent<MeshRenderer>().sharedMaterial = buildingMaterial;
                                }
                                uihandler.audiosource.clip = buildingOk;
                            }
                            else
                                uihandler.audiosource.clip = uihandler.nokSound;
                            uihandler.audiosource.Play();
                        }
                    }
                }
                else if(uihandler.toolName == "delete")
                {
                    if (currentObject)
                        Destroy(currentObject);
                    helper.mode = 2;
                    helper.transform.rotation = Quaternion.identity;
                    List<GameObject> buildings = new List<GameObject>();// map.SearchBuildingsGameObject(pointing, 4f);

                    if (buildings.Count != 0)
                    {
                        helper.transform.position = buildings[0].transform.position;

                        Transform mesh = buildings[0].transform.Find("mesh");
                        if (!mesh)
                            mesh = buildings[0].transform;
                        Vector3 s = mesh.GetComponent<Collider>().bounds.size;

                        if (s.x > 4f || s.z > 4f)
                            helper.transform.localScale = new Vector3(8f, 5f, 8f);
                        else helper.transform.localScale = new Vector3(4f, 4f, 4f);
                    }
                    else
                    {
                        helper.transform.position = pointing + new Vector3(0, 2f, 0);
                        helper.transform.localScale = new Vector3(4f, 3f, 4f);
                    }

                    if (Input.GetMouseButtonDown(0) && buildings.Count != 0)
                    {
                        uihandler.audiosource.clip = uihandler.selectedSound;
                        uihandler.audiosource.Play();
                        
                        Transform interactorTransform = buildings[0].transform.Find("interactor");
                        Dictionary<string, int> interactorContent = new Dictionary<string, int>();
                        if (interactorTransform)
                        {
                            if (interactorTransform.GetComponent<RessourceContainer>())
                                interactorTransform.GetComponent<RessourceContainer>().CopyInventory(interactorContent);
                            Destroy(interactorTransform.gameObject);
                        }

                        GameObject newInteractor = Instantiate(destroyInteractor, buildings[0].transform);
                        newInteractor.name = "interactor";
                        newInteractor.transform.localPosition = Vector3.zero;
                        newInteractor.transform.localRotation = Quaternion.identity;
                        newInteractor.transform.localScale = Vector3.one;
                        newInteractor.GetComponent<BoxCollider>().size = (helper.transform.localScale.x + 1f) * Vector3.one;
                        newInteractor.SetActive(true);

                        DestructionTemplate desInt = newInteractor.GetComponent<DestructionTemplate>();
                        desInt.previousContent = interactorContent;
                        desInt.recuperation = scavengeRatio;
                    }
                    else if (Input.GetMouseButtonDown(0))
                    {
                        uihandler.audiosource.clip = uihandler.nokSound;
                        uihandler.audiosource.Play();
                    }
                }
                else if (uihandler.toolName == "terrain")
                {
                    if (currentObject)
                    {
                        pointing = new Vector3(pointing.x, 0, pointing.z);
                        currentObject.transform.position = pointing;
                        Vector3Int cell;// = map.GetCellFromWorld(pointing);
                        ScriptableTile tile;// = map.tilemap.GetTile<ScriptableTile>(cell);
                        
                        if (prevTerrainBrushTile != cell)
                        {
                            if (ocludedTerrainTile)
                                ocludedTerrainTile.SetActive(true);

                            List<GameObject> ocludedList = new List<GameObject>();// map.SearchTilesGameObject(pointing, 3f);
                            if (ocludedList.Count != 0)
                            {
                                ocludedTerrainTile = ocludedList[0];
                                Debug.Log(ocludedList.Count);
                                ocludedTerrainTile.SetActive(false);
                            }
                            else ocludedTerrainTile = null;
                        }

                        if (Input.GetMouseButtonDown(0) || ((prevTerrainBrushTile != cell) && Input.GetMouseButton(0)))
                        {
                            List<GameObject> buildings = map.SearchBuildingsGameObject(pointing, 4f);
                            if (buildings.Count == 0)
                            {
                                List<Vector3> positions = new List<Vector3>();
                                positions.Add(pointing);
                                List<GameObject> originals = new List<GameObject>();
                                originals.Add(ocludedTerrainTile);
                                map.PlaceTiles(positions, originals, currentObject.name);
                                uihandler.audiosource.clip = terrainOk;

                                List<GameObject> ocludedList = map.SearchTilesGameObject(pointing, 3f);
                                if (ocludedList.Count != 0)
                                {
                                    foreach (GameObject go in ocludedList)
                                    {
                                        if (go != ocludedTerrainTile)
                                            ocludedTerrainTile = go;
                                    }
                                    ocludedTerrainTile.SetActive(false);
                                }
                                else ocludedTerrainTile = null;
                            }
                            else uihandler.audiosource.clip = uihandler.nokSound;

                            uihandler.audiosource.Play();
                        }
                        
                        prevTerrainBrushTile = cell;
                    }                    
                }
                else if (uihandler.toolName.Length != 0)
                    Debug.LogWarning("Construction mode tool " + uihandler.toolName + " unknown");
            }*/
        }

        // check if quit mode
        if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(keyMode) && lastActivated == activated) || quit)
        {
            trackballController.enabled = true;
            activated = false;
            transform.position = entryPosition;
            transform.rotation = entryRotation;
            constructionUI.SetActive(false);
            quit = false;
            if (currentObject)
                Destroy(currentObject);
            if (ocludedTerrainTile)
                ocludedTerrainTile.SetActive(true);
            ocludedTerrainTile = null;
            uihandler.Reset();
            helper.mode = 0;
        }
        lastActivated = activated;
    }
    public void SelectedBuilding(GameObject icon)
    {
        Debug.Log("selected brush : " + icon.name);
        return;



        if(uihandler.toolName == "building")
        {
            if (currentObject)
                Destroy(currentObject);
            currentObject = ConstructionDictionary.Instance.Get(icon.name);
            currentTemplate = currentObject.transform.Find("interactor").GetComponent<ConstructionTemplate>();
            currentRenderer = currentObject.transform.Find("mesh").GetComponent<MeshRenderer>();
        }
        else if(uihandler.toolName == "terrain")
        {
            if (currentObject)
                Destroy(currentObject);
            if (ocludedTerrainTile)
                ocludedTerrainTile.SetActive(true);
            ocludedTerrainTile = null;

            ScriptableTile tilePrefab = null;
            /*foreach(ScriptableTile st in map.tileList)
            {
                if(st.name == icon.name)
                {
                    tilePrefab = st;
                    break;
                }
            }*/

            if (tilePrefab != null)
            {
                currentObject = Instantiate(tilePrefab.ground);
                currentObject.name = tilePrefab.name;

                Water water = currentObject.GetComponent<Water>();
                if (water) water.Initialize(true, true, true, true, 0.1f);

                Bridge bridge = currentObject.GetComponent<Bridge>();
                if (bridge) bridge.Initialize(true);

                Stone stone = currentObject.GetComponent<Stone>();
                if (stone) stone.Initialize(2);

                MineralRessource mineral = currentObject.GetComponent<MineralRessource>();
                if (mineral) mineral.Initialize(tilePrefab.optionalMaterial);
                
                /*Grass grass = currentObject.GetComponent<Grass>();
                if (grass) grass.Initialize(9);*/

                Dirt dirt = currentObject.GetComponent<Dirt>();
                if (dirt) dirt.Initialize(true, true, true, true, 0.1f);

                ocludedTerrainTile = null;
            }
            else Debug.LogWarning("no registred tile i map with name :" +  icon.name);
        }
        else Debug.LogWarning("Construction mode tool (" + uihandler.toolName + ") unknown for brush (" + icon.name + ")");
    }
}
