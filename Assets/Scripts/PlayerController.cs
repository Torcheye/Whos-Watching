using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        Move();
    }

    private void Move()
    {
        var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");
        var movement = y * Vector3.forward + x * Vector3.right;

        _animator.SetFloat("VelocityX", x);
        _animator.SetFloat("VelocityY", y);
        _animator.SetBool("IsWalking", movement.magnitude > 0.1f);

        transform.Translate(Time.deltaTime * speed * movement);
    }
}
