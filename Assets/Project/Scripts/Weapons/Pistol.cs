using System.Collections;
using System.Collections.Generic;

public class Pistol : Weapon
{
    public Pistol()
    {
        clipSize = 12;
        maxAmmunition = 60;
        reloadDuration = 2.0f;
        cooldownDuration = 0.10f;
        isAutomatic = false;
        name = "Pistol";
    }
}
