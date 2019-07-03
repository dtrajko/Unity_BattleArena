using System.Collections;
using System.Collections.Generic;

public class Weapon
{
    private int clipAmmunition;
    private int totalAmmunition;

    private int clipSize;
    private int maxAmmunition;
    private float reloadTime;
    private float cooldownTime;
    private bool isAutomatic;

    public int ClipAmmunition { get { return clipAmmunition; } set { clipAmmunition = value;  } }
    public int TotalAmmunition { get { return totalAmmunition; } set { totalAmmunition = value; } }
    public int ClipSize { get { return clipSize; } }
    public int MaxAmmunition { get { return maxAmmunition; } }
    public float ReloadTime { get { return reloadTime; } }
    public float CooldownTime { get { return cooldownTime; } }
    public bool IsAutomatic { get { return isAutomatic; } }
}
