﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput_Ctrl : MonoBehaviour {

    public enum DriveType { JetThrusters, TankTread , RoboLegs};

    public int PlayerNumber = 0;

    public float Speed_1 = 300f;
    public float Speed_2 = 300f;
    public float Jump = 100f;
    public float Rotate_F_Max = 400f;
    public float Rotate_V_Break = 50f;
    public float WheelFriction = 0.3f;


    public DriveType MountedDrive = DriveType.JetThrusters;

    public Camera PlayerView = null;
    public GameObject GunTower = null;
    public Wepon_Ctrl Wepon_R = null;
    public Wepon_Ctrl Wepon_L = null;

    InputTable_G MyInput = new InputTable_G();
    Rigidbody Body;

	// Use this for initialization
	void Start () {
        Body = gameObject.GetComponent<Rigidbody>();

        // protective debug
        if (Body == null)
            Debug.Log(" ERROR: Robot Body not found ! ");
        if (PlayerView == null)
            Debug.Log(" ERROR: Player camera not assigned ! ");
        if (GunTower == null)
            Debug.Log(" ERROR: robot model not assigned ! ");
        if (Wepon_R == null)
            Debug.Log(" ERROR: right wepon not assigned ! ");
        if (Wepon_L == null)
            Debug.Log(" ERROR: left wepon not assigned ! ");

    }
	
    float ToCursorRotation(GameObject Rot_obj) 
    {
        float result = 0f;

        //Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)PlayerView.ScreenToViewportPoint(Input.mousePosition);


        float Rt_Target = Mathf.Atan2(mouseOnScreen.x - 0.5f, mouseOnScreen.y - 0.5f);
        Rt_Target *= 180f / Mathf.PI;
        Rt_Target += (Rt_Target < 0f) ? (360f) : (0f);

        float Rt_Current = Rot_obj.transform.localRotation.eulerAngles.y;

        result = Rt_Target - Rt_Current;
        if(result > 180f)
            result -= 360f;
        if (result < -180f)
            result += 360f;
        
        return result;
    }

    void KB_Apply_movement(bool up, bool down, bool left, bool right, bool M_Start=false)
    {
        Vector3 MoveDir = Vector3.zero;
        if (up) MoveDir += Vector3.forward;
        if (down) MoveDir += Vector3.back;
        if (left) MoveDir += Vector3.left;
        if (right) MoveDir += Vector3.right;
        MoveDir.Normalize();

        if (M_Start)
        {
            switch (MountedDrive)
            {
                case DriveType.JetThrusters: // jet thrusters chave slow start
                    break;
                case DriveType.TankTread:
                    Body.AddForce(MoveDir * Speed_2 * WheelFriction, ForceMode.Impulse);
                    break;
            }
        }
        else
        {

            switch (MountedDrive)
            {
                case DriveType.JetThrusters:
                    Body.AddForce(MoveDir * Speed_1, ForceMode.Force);
                    break;
                case DriveType.TankTread:
                    Body.AddForce(MoveDir * Speed_2, ForceMode.Force);
                    break;
            }
        }

    }

    void KB_Jump(bool jump, bool J_Start = false)
    {
        if (jump)
        {
            if (J_Start)
            {
                if (IsGrounded())
                {
                    Body.AddForce(Vector3.up * Jump, ForceMode.Impulse);
                }
            }
        }
    }

    // interface with Wepon_Ctrl class
    void KB_Attak(bool left,  bool right, bool alt_active, bool Attac_Start = false)
    {
        if (Attac_Start)
        {
            if (left)
            {
                Wepon_L.Attak(alt_active);
            }
            if (right)
            {
                Wepon_R.Attak(alt_active);
            }
        }
        else
        {
            if (left)
            {
                Wepon_L.RefreshAttak(alt_active);
            }
            if (right)
            {
                Wepon_R.RefreshAttak(alt_active);
            }
        }
    }

    void CameraFollow()
    {
        Vector3 Position_Dif = gameObject.transform.position - PlayerView.transform.position ;
        PlayerView.transform.Translate(Position_Dif.x, Position_Dif.z, 0f); // (x,y,z) -> (x,z,y) WTF !!!!!

    }

    float integral = 0f;
    float oldRotSpeed = 0f;
    float RotAccel = 0f;

    float Rotation_Force()
    {
        float result = 0f;
        
        RotAccel = (Body.angularVelocity.y - oldRotSpeed) / Time.deltaTime;


        //result = Body.inertiaTensor.y * ( (ToCursorRotation(gameObject) - Body.angularVelocity.y* Time.deltaTime)/(Time.deltaTime* Time.deltaTime) );

        float dist = ToCursorRotation(gameObject);

        result = dist - Body.angularVelocity.y* Mathf.Abs( Rotate_V_Break/ dist);


        if (result > Rotate_F_Max)
            result = Rotate_F_Max;
        if (result < -Rotate_F_Max)
            result = -Rotate_F_Max;

        oldRotSpeed = Body.angularVelocity.y;

        return result;
    }

    bool IsGrounded()
    {
        return Physics.BoxCast(transform.position, new Vector3(0.28f, 0.1f, 0.28f), -Vector3.up, Quaternion.identity, 0.2f);
    }

// Update is called once per frame
void Update () {


        if (Input.anyKey)
        {
            KB_Apply_movement(
                Input.GetKey(MyInput.Default_0.Up),
                Input.GetKey(MyInput.Default_0.Down),
                Input.GetKey(MyInput.Default_0.Left),
                Input.GetKey(MyInput.Default_0.Right),
                false);

            KB_Jump(
                Input.GetKeyDown(MyInput.Default_0.Jump),
                false);

            KB_Attak(
                Input.GetKeyDown(MyInput.Default_0.FireLeft),
                Input.GetKeyDown(MyInput.Default_0.FireRight),
                Input.GetKey(MyInput.Default_0.AltFire),
                false);

        }
        if (Input.anyKeyDown)
        {
            KB_Apply_movement(
                Input.GetKey(MyInput.Default_0.Up),
                Input.GetKey(MyInput.Default_0.Down),
                Input.GetKey(MyInput.Default_0.Left),
                Input.GetKey(MyInput.Default_0.Right),
                true);

            KB_Jump(
                Input.GetKeyDown(MyInput.Default_0.Jump),
                true);

            KB_Attak(
                Input.GetKeyDown(MyInput.Default_0.FireLeft),
                Input.GetKeyDown(MyInput.Default_0.FireRight),
                Input.GetKey(MyInput.Default_0.AltFire),
                true);
            


            if (Input.GetKeyDown(MyInput.Default_0.FireRight))
            {
               // Debug.Log(" Shoot 2 " + Body.inertiaTensor.ToString() );
            }
        }

        CameraFollow();

        //Body.AddTorque(0f, ToCursorRotation(gameObject), 0f, ForceMode.Force);
        Body.AddTorque(0f, Rotation_Force(), 0f, ForceMode.Force);
        
        //gameObject.transform.Rotate(Vector3.up, ToCursorRotation(gameObject) , Space.Self);
        //GunTower.transform.Rotate(Vector3.up, ToCursorRotation(GunTower) , Space.Self);

    }
}
