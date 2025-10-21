using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;


public class Physics2DBridge : MonoBehaviour,IBridge
{
    public Rigidbody2D rb;
    public float groundCheckRadius = 0.2f;
    public Vector2 groundCheckOffset = Vector2.zero;
    public LayerMask groundLayerMask = 1; // 默认层
    
    [Header("Debug")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.green;
    
    public void Initialize(LuaTable luaInstance)
    {
        Initialize();
    }
    
    public void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        if (groundLayerMask == 0)
            groundLayerMask = LayerMask.GetMask("Ground");
    }

    public void ApplyVelocity(Vector2 velocity)
    {
        rb.velocity = new Vector2(velocity.x, rb.velocity.y);
    }
    
    public void SetVelocity(Vector2 velocity)
    {
        rb.velocity = velocity;
    }

    public Vector2 GetVelocity()
    {
        return rb.velocity;
    }
    
    public void ApplyImpulse(Vector2 dir, float force)
    {
        rb.AddForce(dir.normalized * force, ForceMode2D.Impulse);
    }

    public void ApplyForce(Vector2 dir, float force)
    {
        rb.AddForce(dir.normalized * force, ForceMode2D.Force);
    }

    public void SetAngularVelocity(float angularVelocity)
    {
        rb.angularVelocity = angularVelocity;
    }
    
    public float GetAngularVelocity()
    {
        return rb.angularVelocity;
    }
    
    public void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }
    
    public void SetBodyType(string bodyType)
    {
        switch (bodyType)
        {
            case "Static":
                rb.bodyType = RigidbodyType2D.Static;
                break;
            case "Kinematic":
                rb.bodyType = RigidbodyType2D.Kinematic;
                break;
            case "Dynamic":
                rb.bodyType = RigidbodyType2D.Dynamic;
                break;
        }
    }
    
    public void SetLinearDrag(float drag)
    {
        rb.drag = drag;
    }
    
    public void SetAngularDrag(float drag)
    {
        rb.angularDrag = drag;
    }
    
    public float GetMass()
    {
        return rb.mass;
    }
    
    public void SetMass(float mass)
    {
        rb.mass = mass;
    }

    #region 检测
    
    public RaycastHit2D Raycast(Vector2 direction, float distance, string layerMask)
    {
        return Physics2D.Raycast(transform.position, direction, distance, LayerMask.GetMask(layerMask));
    }
    
    public Collider2D[] OverlapCircleAll(Vector2 offset, float radius, string layerMask)
    {
        Vector2 checkPosition = (Vector2)transform.position + offset;
        return Physics2D.OverlapCircleAll(checkPosition, radius, LayerMask.GetMask(layerMask));
    }
    
    public Collider2D[] OverlapBoxAll(Vector2 offset, Vector2 size, float angle, string layerMask)
    {
        Vector2 checkPosition = (Vector2)transform.position + offset;
        return Physics2D.OverlapBoxAll(checkPosition, size, angle, LayerMask.GetMask(layerMask));
    }
    
    public bool IsGrounded()
    {
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
        return Physics2D.OverlapCircle(checkPosition, groundCheckRadius, groundLayerMask);
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
    }
    
    #endregion
}
