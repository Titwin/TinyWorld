using UnityEngine;
using System.Collections;

namespace Pathfinding.Examples
{
    /// <summary>Small sample script for placing obstacles</summary>
    [HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_object_placer.php")]
    public class ObjectPlacer : MonoBehaviour
    {
        /// <summary>
        /// GameObject to place.
        /// When using a Grid Graph you need to make sure the object's layer is included in the collision mask in the GridGraph settings.
        /// </summary>
        public GameObject go;

        /// <summary>Flush Graph Updates directly after placing. Slower, but updates are applied immidiately</summary>
        public bool direct = false;

        /// <summary>Issue a graph update object after placement</summary>
        public bool issueGUOs = true;

        public LayerMask obstacleLayer;
        public LayerMask floorLayer;
        /// <summary>Update is called once per frame</summary>
        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Figure out where the ground is
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayer))
            {
                Vector3 p = hit.point;
                Collider[] results = new Collider[32];
                if (0 == Physics.OverlapBoxNonAlloc(p, go.GetComponent<Collider>().bounds.extents, results, Quaternion.identity/*go.transform.rotation*/, obstacleLayer))
                {
                    position = p;
                }
            }

            if (Input.GetKeyDown("p"))
            {
                PlaceObject();
            }

            if (Input.GetKeyDown("r"))
            {
                StartCoroutine(RemoveObject());
            }
        }
        Vector3 position;
        public void PlaceObject()
        {
            GameObject obj = GameObject.Instantiate(go, position, Quaternion.identity) as GameObject;
            Bounds b = obj.GetComponent<Collider>().bounds;
            if (issueGUOs)
            {
                GraphUpdateObject guo = new GraphUpdateObject(b);
                AstarPath.active.UpdateGraphs(guo);
                if (direct)
                {
                    AstarPath.active.FlushGraphUpdates();
                }
            }
        }

        private void OnDrawGizmos()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Collider collider =go.GetComponent<Collider>();
            // Figure out where the ground is
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayer))
            {
                Vector3 p = hit.point;

                Collider[] results = new Collider[32];
                int hits = Physics.OverlapBoxNonAlloc(p, collider.bounds.extents, results, Quaternion.identity/*go.transform.rotation*/, obstacleLayer);
                if (0 == hits)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                    for (int c = 0; c < hits; ++c)
                    {
                        Vector3 size = results[c].bounds.extents * 2;
                        size.y = 0.1f;
                        Gizmos.DrawWireCube(results[c].transform.position, size);
                    }

                }
                {
                    Vector3 size = collider.bounds.extents * 2;
                    size.y = 0.1f;
                    Gizmos.DrawWireCube(position, size);
                    Gizmos.DrawWireCube(p, size);
                }
            }
            {
                Gizmos.color = Color.white;
                Vector3 size = collider.bounds.extents * 2;
                size.y = 0.1f;
                Gizmos.DrawWireCube(position, size);
            }
        }
        public IEnumerator RemoveObject()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check what object is under the mouse cursor
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Ignore ground and triggers
                if (hit.collider.isTrigger || hit.transform.gameObject.name == "Ground") yield break;

                Bounds b = hit.collider.bounds;
                Destroy(hit.collider);
                Destroy(hit.collider.gameObject);

                if (issueGUOs)
                {
                    // In Unity, object destruction is actually delayed until the end of the Update loop.
                    // This means that we need to wait until the end of the frame (or until the next frame) before
                    // we update the graph. Otherwise the graph would still think that the objects are there.
                    yield return new WaitForEndOfFrame();
                    GraphUpdateObject guo = new GraphUpdateObject(b);
                    AstarPath.active.UpdateGraphs(guo);
                    if (direct)
                    {
                        AstarPath.active.FlushGraphUpdates();
                    }
                }
            }
        }
    }
}
