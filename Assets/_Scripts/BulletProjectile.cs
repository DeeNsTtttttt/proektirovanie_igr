using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BulletProjectile : MonoBehaviour
{
    [SerializeField, Min(1)] private int fallbackDamage = 10;
    [SerializeField] private GameObject hitVfxPrefab;

    private Rigidbody rb;
    private Collider ownCollider;
    private BulletPool pool;
    private Coroutine lifetimeRoutine;
    private int damage;
    private GameObject owner;
    private bool hasHit;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void OnDisable()
    {
        if (lifetimeRoutine != null)
        {
            StopCoroutine(lifetimeRoutine);
            lifetimeRoutine = null;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void SetPool(BulletPool ownerPool)
    {
        pool = ownerPool;
    }

    public void Launch(Vector3 direction, float speed, int shotDamage, float lifetime, GameObject shotOwner)
    {
        owner = shotOwner;
        damage = shotDamage > 0 ? shotDamage : fallbackDamage;
        hasHit = false;

        IgnoreOwnerCollisions();

        rb.linearVelocity = direction.normalized * Mathf.Max(1f, speed);

        if (lifetimeRoutine != null)
        {
            StopCoroutine(lifetimeRoutine);
        }

        lifetimeRoutine = StartCoroutine(LifetimeRoutine(Mathf.Max(0.1f, lifetime)));
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessHit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null)
        {
            ProcessHit(collision.collider);
        }
    }

    private void ProcessHit(Collider other)
    {
        if (hasHit || other == null || other.isTrigger)
        {
            return;
        }

        if (owner != null)
        {
            Transform ownerTransform = owner.transform;
            if (other.gameObject == owner || other.transform.IsChildOf(ownerTransform))
            {
                return;
            }
        }

        hasHit = true;

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInChildren<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        if (hitVfxPrefab != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            GameObject vfx = Instantiate(hitVfxPrefab, hitPoint, Quaternion.identity);
            Destroy(vfx, 1.5f);
        }

        Despawn();
    }

    private IEnumerator LifetimeRoutine(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        Despawn();
    }

    private void Despawn()
    {
        if (lifetimeRoutine != null)
        {
            StopCoroutine(lifetimeRoutine);
            lifetimeRoutine = null;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (pool != null)
        {
            pool.ReturnBullet(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void IgnoreOwnerCollisions()
    {
        if (owner == null || ownCollider == null)
        {
            return;
        }

        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();
        foreach (Collider c in ownerColliders)
        {
            if (c != null)
            {
                Physics.IgnoreCollision(ownCollider, c, true);
            }
        }
    }
}