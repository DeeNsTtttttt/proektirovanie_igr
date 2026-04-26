using TMPro;
using UnityEngine;

public class ShooterHUD : MonoBehaviour
{
    [SerializeField] private ShooterWeapon weapon;
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private TMP_Text bonusText;

    private void Update()
    {
        if (weapon == null)
        {
            return;
        }

        if (ammoText != null)
        {
            ammoText.text = $"Ammo: {weapon.CurrentAmmo}/{weapon.ReserveAmmo}";
        }

        if (bonusText != null)
        {
            if (weapon.IsReloading)
            {
                bonusText.text = "Reloading...";
            }
            else if (weapon.IsLastBulletsBoostActive)
            {
                bonusText.text = "Last bullets: damage boost";
            }
            else
            {
                bonusText.text = string.Empty;
            }
        }
    }
}
