using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {

    public float Speed = 10;

    Animator animator;

    void Start()
    {
        EasyJoystick.On_JoystickMove += OnJoystickMove;
        EasyJoystick.On_JoystickMoveEnd += OnJostickMoveEnd;

        animator = GetComponentInChildren<Animator>();
    }

    void OnJoystickMove(MovingJoystick move)
    {
        var dir2d = move.joystickAxis;
        var dir = new Vector3(dir2d.x, 0, dir2d.y);
        var now_pos = transform.position;
        var target_pos = now_pos + dir * Time.deltaTime * Speed;
        target_pos.y += 100;
        RaycastHit hit;
        Physics.Raycast(target_pos, Vector3.down, out hit);
        target_pos.y = hit.point.y ;
        transform.position = target_pos;

        transform.rotation = Quaternion.Euler(0, -Mathf.Atan2(dir2d.y, dir2d.x) * Mathf.Rad2Deg +  90, 0);

        animator.SetFloat("WalkSpeed", dir2d.magnitude);
        animator.Play("Run");
    }

    void OnJostickMoveEnd(MovingJoystick move) {
        animator.SetFloat("WalkSpeed", 0);
    }
}
