using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float rotateSpeed;
    
    private Animator _animator;
    private bool _isWalking;
    private bool _isRunning;

    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(PlayFootStep());
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        var movement = y * Vector3.forward;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movement *= 1.75f;
            _isRunning = true;
        }
        else
        {
            _isRunning = false;
        }

        _isWalking = movement.z > 0.01f;
        
        _animator.SetFloat(Velocity, movement.z);
        _animator.SetBool(IsMoving, _isWalking);

        transform.Translate(Time.deltaTime * speed * movement);
        transform.Rotate(Vector3.up, Time.deltaTime * x * rotateSpeed);
    }
    
    private IEnumerator PlayFootStep()
    {
        while (true)
        {
            if (_isWalking || _isRunning)
            {
                GameManager.Instance.PlaySound(Sound.Footstep);
            }
            yield return new WaitForSeconds(_isRunning ? .23f : .4f);
        }
    }
}
