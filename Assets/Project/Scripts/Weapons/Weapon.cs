using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Weapon
{
    private int clipAmmunition = 0;
    private int totalAmmunition = 0;

    protected int clipSize = 0;
    protected int maxAmmunition = 0;
    protected float reloadTime = 0.0f;
    protected float cooldownTime = 0.0f;
    protected bool isAutomatic = false;

    public int ClipAmmunition { get { return clipAmmunition; } set { clipAmmunition = value;  } }
    public int TotalAmmunition { get { return totalAmmunition; } set { totalAmmunition = value; } }
    public int ClipSize { get { return clipSize; } }
    public int MaxAmmunition { get { return maxAmmunition; } }
    public float ReloadTime { get { return reloadTime; } }
    public float CooldownTime { get { return cooldownTime; } }
    public bool IsAutomatic { get { return isAutomatic; } }

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
}
