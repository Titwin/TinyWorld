using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] public Rigidbody rb;
    [SerializeField] new Collider collider;
    [SerializeField] TrailRenderer trail;
    [SerializeField] int damage = 1;
    [SerializeField] public int team;
    [SerializeField] float ttl = 2;
    private void OnValidate()
    {
        Start();
    }
    // Start is called before the first frame update
    private void Start()
    {
        if(!rb)rb = GetComponent<Rigidbody>();
        if (!collider) collider = GetComponent<Collider>();
        if (!trail) trail = GetComponent<TrailRenderer>();
    }
    public void LateUpdate()
    {
        ttl -= Time.deltaTime;
        if (ttl < 0)
        {
            Destroy(this.gameObject);
        }
    }
    private void OnTriggerEnter(Collider collision)
    {
        GameObject go = collision.attachedRigidbody ? collision.attachedRigidbody.gameObject : collision.gameObject;
        IDamageable target = go.GetComponent<IDamageable>();
        if (target!=null && target.GetTeam() != this.team)
        {
            target.GetDamage(damage);

            collider.enabled = false;
            rb.isKinematic = true;
            trail.enabled = false;
            this.transform.parent = collision.transform;
        }

    }
}
