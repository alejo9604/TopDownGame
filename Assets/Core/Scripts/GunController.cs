﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

    public Transform weaponHold;
    public Gun startingGun;
    Gun equippedGun;

    private void Start()
    {
        if (startingGun != null)
            EquipedGun(startingGun);
    }

    public void EquipedGun(Gun gunToEquip)
    {
        if (equippedGun != null)
            Destroy(equippedGun.gameObject);
        equippedGun = Instantiate(gunToEquip, weaponHold.position, weaponHold.rotation);
        equippedGun.transform.parent = weaponHold;
    }
   
    public void OnTriggerHold()
    {
        if (equippedGun != null)
            equippedGun.OnTriggerHold();
    }

    public void OnTriggerRelease()
    {
        if (equippedGun != null)
            equippedGun.OnTriggerRelease();
    }

    public float GunHeight
    {
        get
        {
            return weaponHold.position.y;
        }
    }

    public void Aim(Vector3 aimPoint)
    {
        if (equippedGun != null)
            equippedGun.Aim(aimPoint);
    }

    public void Reload()
    {
        if (equippedGun != null)
            equippedGun.Reload();
    }

}
