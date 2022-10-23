using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed;
    public float rotateSpeed;
    
    private Animator _animator;

    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    
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
        var movement = y * Vector3.forward;
        if (Input.GetKey(KeyCode.LeftShift))
            movement *= 1.75f;
        
        _animator.SetFloat(Velocity, movement.z);
        _animator.SetBool(IsMoving, movement.sqrMagnitude > 0);

        transform.Translate(Time.deltaTime * speed * movement);
        transform.Rotate(Vector3.up, Time.deltaTime * x * rotateSpeed);
    }
}
