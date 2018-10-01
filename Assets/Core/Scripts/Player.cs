using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LeavingEntity
{

    public float moveSpeed = 5f;

    Camera viewCamera;
    PlayerController controller;
    GunController gunController;

    protected override void Start () {
        base.Start();
        viewCamera = Camera.main;
        controller = GetComponent<PlayerController>();
        gunController = GetComponent <GunController>();
	}


    void Update() {
        //Movement input
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);

        //Look input
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if(groundPlane.Raycast(ray, out rayDistance))
        {
            //Intersection
            Vector3 point = ray.GetPoint(rayDistance);
            controller.LookAt(point);
        }

        //Weapon input
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
	}
}
