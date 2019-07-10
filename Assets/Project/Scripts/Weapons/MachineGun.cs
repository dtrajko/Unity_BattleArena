using System.Collections;
using System.Collections.Generic;

public class MachineGun : Weapon
{
    public MachineGun()
    {
        clipSize = 30;
        maxAmmunition = 120;
        reloadDuration = 2.0f;
        cooldownDuration = 0.08f;
        isAutomatic = true;
        name = "Machine Gun";
        aimVariation = 0.02f;
        damage = 4.0f;
    }
}
