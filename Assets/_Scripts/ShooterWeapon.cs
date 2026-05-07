using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ShooterWeapon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private Camera shooterCamera;
    [SerializeField] private bool useCameraAim = false;
    [SerializeField] private LayerMask hitMask = ~0;

    [Header("Mouse Aim")]
    [SerializeField] private bool aimAtMouseCursor = true;
    [SerializeField] private bool rotatePlayerOnShot = true;
    [SerializeField, Min(0f)] private float aimOverrideDuration = 0.12f;
    [SerializeField, Min(1f)] private float cursorAimMaxDistance = 500f;
    [SerializeField] private LayerMask cursorAimMask = ~0;

    [Header("Shooting")]
    [SerializeField, Min(1)] private int baseDamage = 20;
    [SerializeField, Min(0.1f)] private float fireRate = 8f;
    [SerializeField, Min(1f)] private float maxDistance = 100f;

    [Header("Projectile")]
    [SerializeField] private bool useProjectileBullets = true;
    [SerializeField] private BulletProjectile bulletPrefab;
    [SerializeField] private BulletPool bulletPool;
    [SerializeField, Min(1f)] private float bulletSpeed = 28f;
    [SerializeField, Min(0.1f)] private float bulletLifetime = 2.2f;
    [SerializeField, Min(0f)] private float spreadAngle = 0f;
    [SerializeField, Min(0f)] private float muzzleOffset = 0.25f;
    [SerializeField] private bool debugDrawShots = true;

    [Header("Ammo")]
    [SerializeField, Min(1)] private int magazineSize = 12;
    [SerializeField, Min(0)] private int startReserveAmmo = 48;
    [SerializeField, Min(0.1f)] private float reloadTime = 1.4f;

    [Header("Genre Feature")]
    [SerializeField, Min(1)] private int boostedLastBulletsCount = 2;
    [SerializeField, Min(1f)] private float boostedDamageMultiplier = 1.5f;

    [Header("Recoil Recovery")]
    [SerializeField, Min(0f)] private float recoilPitchPerShot = 1.2f;
    [SerializeField, Min(0f)] private float recoilYawPerShot = 0.5f;
    [SerializeField, Min(0f)] private float recoilRecoverySpeed = 8f;
    [SerializeField, Min(0f)] private float maxRecoilPitch = 12f;
    [SerializeField, Min(0f)] private float maxRecoilYaw = 6f;

    [Header("Feedback")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shotClip;
    [SerializeField] private AudioClip reloadClip;
    [SerializeField, Range(0f, 1f)] private float audioVolume = 0.8f;

    private int currentAmmo;
    private int reserveAmmo;
    private bool isReloading;
    private float nextShotTime;

    private float recoilPitch;
    private float recoilYaw;

    private Transform ownerRoot;
    private PlayerPhysicsController ownerController;
    private PlayerStats playerStats;

    public int CurrentAmmo => currentAmmo;
    public int ReserveAmmo => reserveAmmo;
    public int MagazineSize => magazineSize;
    public bool IsReloading => isReloading;

    public bool IsLastBulletsBoostActive =>
        !isReloading && currentAmmo > 0 && currentAmmo <= boostedLastBulletsCount;

    private void Awake()
    {
        currentAmmo = magazineSize;
        reserveAmmo = startReserveAmmo;

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (shootPoint == null)
        {
            shootPoint = transform;
        }

        if (shooterCamera == null)
        {
            shooterCamera = Camera.main;
        }

        ownerRoot = transform.root != null ? transform.root : transform;
        ownerController = ownerRoot.GetComponent<PlayerPhysicsController>();
        playerStats = ownerRoot.GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        UpdateRecoilRecovery();

        if (isReloading)
        {
            return;
        }

        if (ReadReloadPressed())
        {
            TryStartReload();
            return;
        }

        if (ReadShootHeld())
        {
            TryShoot();
        }
    }

    public void AddReserveAmmo(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        reserveAmmo += amount;
    }

    private void TryShoot()
    {
        if (Time.time < nextShotTime)
        {
            return;
        }

        if (currentAmmo <= 0)
        {
            TryStartReload();
            return;
        }

        nextShotTime = Time.time + (1f / fireRate);

        int ammoBeforeShot = currentAmmo;
        currentAmmo--;

        ApplyRecoilKick();

        int damage = CalculateDamage(ammoBeforeShot);
        FireShot(damage);
        PlayShotFeedback();
    }

    private int CalculateDamage(int ammoBeforeShot)
    {
        int damage = baseDamage + (playerStats != null ? playerStats.DamageBonus : 0);
        if (ammoBeforeShot <= boostedLastBulletsCount)
        {
            damage = Mathf.RoundToInt(damage * boostedDamageMultiplier);
        }

        return damage;
    }

    private void FireShot(int damage)
    {
        Vector3 rawOrigin = shootPoint != null ? shootPoint.position : transform.position;
        Vector3 direction = ComputeShotDirection(rawOrigin);

        if (rotatePlayerOnShot)
        {
            RotateOwnerToDirection(direction);
        }

        Vector3 origin = rawOrigin + direction * muzzleOffset;

        if (debugDrawShots)
        {
            Debug.DrawRay(origin, direction * maxDistance, Color.cyan, 1.2f);
        }

        if (useProjectileBullets && bulletPrefab != null)
        {
            SpawnProjectile(origin, direction, damage);
            return;
        }

        FireHitscan(origin, direction, damage);
    }

    private Vector3 ComputeShotDirection(Vector3 origin)
    {
        Vector3 baseDirection;

        if (aimAtMouseCursor && TryGetCursorAimPoint(origin, out Vector3 cursorPoint))
        {
            baseDirection = (cursorPoint - origin).normalized;
        }
        else if (useCameraAim && shooterCamera != null)
        {
            Ray aimRay = shooterCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 aimPoint = aimRay.origin + aimRay.direction * maxDistance;

            if (Physics.Raycast(aimRay, out RaycastHit aimHit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
            {
                aimPoint = aimHit.point;
            }

            baseDirection = (aimPoint - origin).normalized;
        }
        else
        {
            Transform basis = shootPoint != null ? shootPoint : transform;
            baseDirection = basis.forward;
        }

        Transform recoilBasis = shootPoint != null ? shootPoint : transform;
        baseDirection = ApplyRecoilToDirection(baseDirection, recoilBasis);

        if (spreadAngle > 0f)
        {
            float pitch = Random.Range(-spreadAngle, spreadAngle);
            float yaw = Random.Range(-spreadAngle, spreadAngle);
            Quaternion spread = Quaternion.Euler(pitch, yaw, 0f);
            baseDirection = (spread * baseDirection).normalized;
        }

        return baseDirection.normalized;
    }

    private bool TryGetCursorAimPoint(Vector3 origin, out Vector3 aimPoint)
    {
        aimPoint = Vector3.zero;

        if (shooterCamera == null)
        {
            return false;
        }

        if (!TryGetMouseScreenPosition(out Vector2 mousePos))
        {
            return false;
        }

        Ray ray = shooterCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, cursorAimMaxDistance, cursorAimMask, QueryTriggerInteraction.Ignore))
        {
            aimPoint = hit.point;
            return true;
        }

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, origin.y, 0f));
        if (groundPlane.Raycast(ray, out float distance))
        {
            aimPoint = ray.GetPoint(distance);
            return true;
        }

        return false;
    }

    private bool TryGetMouseScreenPosition(out Vector2 mousePos)
    {
#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            mousePos = mouse.position.ReadValue();
            return true;
        }
        mousePos = default;
        return false;
#else
        mousePos = Input.mousePosition;
        return true;
#endif
    }

    private void RotateOwnerToDirection(Vector3 direction)
    {
        if (ownerRoot == null)
        {
            return;
        }

        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion target = Quaternion.LookRotation(direction, Vector3.up);
        ownerRoot.rotation = target;

        if (ownerController != null)
        {
            ownerController.SetAimDirection(direction, aimOverrideDuration);
        }
    }

    private void SpawnProjectile(Vector3 origin, Vector3 direction, int damage)
    {
        Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
        BulletProjectile bullet = bulletPool != null
            ? bulletPool.GetBullet(origin, rotation)
            : null;

        if (bullet == null)
        {
            bullet = Instantiate(bulletPrefab, origin, rotation);
        }

        GameObject ownerRootObject = ownerRoot != null ? ownerRoot.gameObject : gameObject;
        bullet.Launch(direction, bulletSpeed, damage, bulletLifetime, ownerRootObject);
    }

    private void FireHitscan(Vector3 origin, Vector3 direction, int damage)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, hitMask, QueryTriggerInteraction.Ignore))
        {
            ApplyDamageToHit(hit, damage, direction);
        }
    }

    private void ApplyDamageToHit(RaycastHit hit, int damage, Vector3 direction)
    {
        EnemyHealth enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = hit.collider.GetComponentInChildren<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForceAtPosition(direction * 5f, hit.point, ForceMode.Impulse);
        }
    }

    private Vector3 ApplyRecoilToDirection(Vector3 direction, Transform basis)
    {
        if (basis == null)
        {
            return direction.normalized;
        }

        Quaternion pitchRotation = Quaternion.AngleAxis(-recoilPitch, basis.right);
        Quaternion yawRotation = Quaternion.AngleAxis(recoilYaw, basis.up);
        return (yawRotation * pitchRotation * direction).normalized;
    }

    private void ApplyRecoilKick()
    {
        recoilPitch = Mathf.Clamp(recoilPitch + recoilPitchPerShot, 0f, maxRecoilPitch);
        recoilYaw = Mathf.Clamp(
            recoilYaw + Random.Range(-recoilYawPerShot, recoilYawPerShot),
            -maxRecoilYaw,
            maxRecoilYaw);
    }

    private void UpdateRecoilRecovery()
    {
        recoilPitch = Mathf.MoveTowards(recoilPitch, 0f, recoilRecoverySpeed * Time.deltaTime);
        recoilYaw = Mathf.MoveTowards(recoilYaw, 0f, recoilRecoverySpeed * Time.deltaTime);
    }

    private void TryStartReload()
    {
        if (isReloading || currentAmmo >= magazineSize || reserveAmmo <= 0)
        {
            return;
        }

        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;
        PlayClip(reloadClip, SyntheticSfx.GetReloadClip());

        yield return new WaitForSeconds(reloadTime);

        int needed = magazineSize - currentAmmo;
        int loaded = Mathf.Min(needed, reserveAmmo);
        currentAmmo += loaded;
        reserveAmmo -= loaded;

        isReloading = false;
    }

    private void PlayShotFeedback()
    {
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        PlayClip(shotClip, SyntheticSfx.GetShotClip());
    }

    private void PlayClip(AudioClip clip, AudioClip fallbackClip = null)
    {
        AudioClip activeClip = clip != null ? clip : fallbackClip;
        if (activeClip == null)
        {
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(activeClip, audioVolume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(activeClip, transform.position, audioVolume);
        }
    }

    private bool ReadShootHeld()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.isPressed;
#else
        return Input.GetMouseButton(0);
#endif
    }

    private bool ReadReloadPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard kb = Keyboard.current;
        return kb != null && kb.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }
}


