using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateList : MonoBehaviour
{

    public bool jumping = false;
    public bool dashing = false;
    public bool canMove = true;
    public bool canJump = true;
    public bool canAttack = true;

    public bool Blocking = false;
    public bool recoilingX, recoilingY;
    public bool lookingRight;
    public bool invincible;
    public bool IsGrounded;
    public bool GroundLock = false;

    //[Header("Ground Check")]
    //[SerializeField] private Transform groundCheckPoint;
    //[SerializeField] private float groundCheckY = 0.2f;
    //[SerializeField] private float groundCheckX = 0.5f;
    //[SerializeField] private LayerMask whatIsGround;

    //public bool Grounded()
    //{
    //    if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
    //        || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
    //        || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
}
