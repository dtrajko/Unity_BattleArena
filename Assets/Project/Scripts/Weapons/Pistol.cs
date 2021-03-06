﻿using System.Collections;
using System.Collections.Generic;

public class Pistol : Weapon
{
    public Pistol()
    {
        clipSize = 12;
        maxAmmunition = 60;
        reloadDuration = 1.2f;
        cooldownDuration = 0.10f;
        isAutomatic = false;
        name = "Pistol";
        aimVariation = 0.03f;
        damage = 4.0f;
        sortIndex = 0;
    }
}

