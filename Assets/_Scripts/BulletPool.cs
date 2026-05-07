using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField, Min(0)] private int poolSize = 20;

    private readonly Queue<BulletProjectile> pool = new Queue<BulletProjectile>();

    private void Awake()
    {
        Prewarm();
    }

    public BulletProjectile GetBullet(Vector3 position, Quaternion rotation)
    {
        if (bulletPrefab == null)
        {
            return null;
        }

        BulletProjectile bullet = pool.Count > 0 ? pool.Dequeue() : CreateBullet();
        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.gameObject.SetActive(true);
        return bullet;
    }

    public void ReturnBullet(BulletProjectile bullet)
    {
        if (bullet == null)
        {
            return;
        }

        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }

    private void Prewarm()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            BulletProjectile bullet = CreateBullet();
            bullet.gameObject.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    private BulletProjectile CreateBullet()
    {
        BulletProjectile bullet = Instantiate(bulletPrefab, transform);
        bullet.SetPool(this);
        return bullet;
    }
}