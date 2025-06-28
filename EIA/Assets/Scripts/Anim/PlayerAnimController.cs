using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Hit()
    {
        animator.Play("hit", 0, 0);
    }

    public void Walk()
    {
        //animator.Play("walk", 0, 0);
        animator.SetBool("IsWalking", true);
    }

    public void Idle()
    {
        //animator.Play("idle", 0, 0);
        animator.SetBool("IsWalking", false);
    }
    
    public void Parry()
    {
        animator.Play("parry", 0, 0);
    }
    
    public void Dodge()
    {
        animator.Play("dodge", 0, 0);
    }

    public void TriggerEffect(string effectName)
    {
        
    }

    public string GetState()
    {
        var state = animator.GetCurrentAnimatorStateInfo(0);
        return state.ToString();
    }
}
