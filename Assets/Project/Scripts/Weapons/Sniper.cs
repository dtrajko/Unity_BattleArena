using System.Collections;
using System.Collections.Generic;

public class Sniper : Weapon
{
    public Sniper()
    {
        clipSize = 1;
        maxAmmunition = 10;
        reloadDuration = 2.0f;
        cooldownDuration = 0.5f;
        isAutomatic = false;
        name = "Sniper";
        aimVariation = 0.0f;
    }
}
