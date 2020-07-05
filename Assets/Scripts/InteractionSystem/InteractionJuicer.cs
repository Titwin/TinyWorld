using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionJuicer : MonoBehaviour
{
    [Header("Ressource collection")]
    public Transform pivot;
    public TextMesh textMesh;
    
    public float amplitude;
    public int duration;
    public AnimationCurve animCurve;

    [Header("WoodCutting anim")]
    public GameObject treeInteractor = null;
    public AnimationCurve woodCuttingAnimation;

    private IEnumerator gainCoroutine;
    private IEnumerator treeCoroutine;

    [Header("Hovered item")]
    public float ratio = 1f;
    public GameObject hovered;
    public Transform cornerPivot;
    public List<Transform> corners;

    [Header("Loading")]
    public float loadingRate = 0f;
    public GameObject loadingBar;
    public SpriteMask loadingMask;
    

    public void OnDelete()
    {
        Destroy(cornerPivot.gameObject);
    }

    private void Update()
    {
        // hovered object identifier
        if(hovered)
        {
            cornerPivot.transform.parent = null;
            Collider box = hovered.GetComponent<Collider>();
            cornerPivot.position = new Vector3(box.bounds.center.x, 0.02f, box.bounds.center.z);
            cornerPivot.localEulerAngles = new Vector3(0, hovered.transform.rotation.eulerAngles.y, 0);
            Vector3 s = Vector3.one;
            if (box.bounds.extents.x < 0.7f || box.bounds.extents.z < 0.7f)
                s *= 0.5f;

            foreach (Transform c in corners)
            {
                c.gameObject.SetActive(true);
                c.localScale = s;
            }

            corners[0].localPosition = ratio * new Vector3(-box.bounds.extents.x, 0, box.bounds.extents.z);
            corners[1].localPosition = ratio * new Vector3( box.bounds.extents.x, 0, box.bounds.extents.z);
            corners[2].localPosition = ratio * new Vector3(-box.bounds.extents.x, 0,-box.bounds.extents.z);
            corners[3].localPosition = ratio * new Vector3( box.bounds.extents.x, 0,-box.bounds.extents.z);
        }
        else
        {
            foreach (Transform c in corners)
                c.gameObject.SetActive(false);
        }

        // loading bar
        if(loadingRate > 0f)
        {
            loadingBar.SetActive(true);
            loadingMask.alphaCutoff = 1f - loadingRate;
        }
        else
        {
            loadingBar.SetActive(false);
        }
    }


    public void LaunchGainAnim(string text, InteractionType.Type type, float delay = 0f)
    {
        textMesh.text = text;
        if (type == InteractionType.Type.collectWood)
        {
            treeCoroutine = WoodCuttingAnimation();
            StartCoroutine(treeCoroutine);
        }

        textMesh.color = ResourceDictionary.instance.resourcesFromType[type].color;
        gainCoroutine = RessourceGainAnimation(delay);
        StartCoroutine(gainCoroutine);
    }

    
    private IEnumerator RessourceGainAnimation(float delay)
    {
        if(delay > 0f)
            yield return new WaitForSeconds(delay);

        pivot.gameObject.SetActive(true);
        for (int i = 0; i < duration; i++)
        {
            pivot.localPosition = Vector3.Lerp(Vector3.zero, amplitude * Vector3.up, animCurve.Evaluate((float)i / duration));
            pivot.right = Camera.main.transform.right;
            yield return null;
        }
        pivot.gameObject.SetActive(false);
    }
    private IEnumerator WoodCuttingAnimation()
    {
        //MapModifier.instance.grid.MakeObjectInteractable(treeInteractor.transform.parent.gameObject);
        Transform tree = FindChildContains(treeInteractor.transform.parent, "Tree");
        Quaternion initial = tree.rotation;
        Vector3 v = (tree.position - transform.position).normalized;
        Quaternion q = Quaternion.AngleAxis(-15f, Vector3.Cross(v, Vector3.up).normalized) * initial;
        int speed = 30;
        for (int i = 0; i < speed; i++)
        {
            if (!treeInteractor)
                break;

            tree.rotation = Lerp(initial, q, woodCuttingAnimation.Evaluate((float)i / speed) - 0.5f);
            yield return null;
        }
    }
    private Quaternion Lerp(Quaternion a, Quaternion b, float t)
    {
        Quaternion q = new Quaternion();
        q.x = (1f - t) * a.x + t * b.x;
        q.y = (1f - t) * a.y + t * b.y;
        q.z = (1f - t) * a.z + t * b.z;
        q.w = (1f - t) * a.w + t * b.w;
        return q.normalized;
    }
    private Transform FindChildContains(Transform parent, string childSubName)
    {
        foreach(Transform child in parent)
        {
            if (child.name.Contains(childSubName))
                return child;
        }
        return null;
    }
}
