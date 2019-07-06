using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Weapon
{
    // Ammunition fields
    private int clipAmmunition = 0;
    private int totalAmmunition = 0;

    // Weapon settings (customizable on weapon classes)
    protected int clipSize = 0;
    protected int maxAmmunition = 0;
    protected float reloadDuration = 0.0f;
    protected float cooldownDuration = 0.0f;
    protected bool isAutomatic = false;
    protected string name = "";

    // Private fields
    private float reloadTimer = 0.0f;
    private float cooldownTimer = 0.0f;
    private bool pressedTrigger = false;

    // Properties
    public int ClipAmmunition { get { return clipAmmunition; } set { clipAmmunition = value;  } }
    public int TotalAmmunition { get { return totalAmmunition; } set { totalAmmunition = value; } }
    public int ClipSize { get { return clipSize; } }
    public int MaxAmmunition { get { return maxAmmunition; } }
    public float ReloadDuration { get { return reloadDuration; } }
    public float CooldownDuration { get { return cooldownDuration; } }
    public bool IsAutomatic { get { return isAutomatic; } }
    public string Name { get { return name; } }

    internal void AddAmmunition(int amount)
    {
        totalAmmunition = System.Math.Min(totalAmmunition + amount, maxAmmunition);
    }

    public void LoadClip() {
        int maximumAmmunitionToLoad = clipSize - clipAmmunition;
        int ammunitionToLoad = System.Math.Min(maximumAmmunitionToLoad, totalAmmunition);

        clipAmmunition += ammunitionToLoad;
        totalAmmunition -= ammunitionToLoad;
    }

    public bool Update(float deltaTime, bool isPressingTrigger) {
        bool hasShot = false;
        cooldownTimer -= deltaTime;
        if (cooldownTimer <= 0) {
            bool canShoot = false;
            if (isAutomatic) canShoot = isPressingTrigger;
            else if (!pressedTrigger && isPressingTrigger) canShoot = true;

            if (canShoot) {
                cooldownTimer = cooldownDuration;
                if (clipAmmunition > 0) {
                    clipAmmunition--;
                    hasShot = true;
                }
            }
            pressedTrigger = isPressingTrigger;
        }
        return hasShot;
    }
}
