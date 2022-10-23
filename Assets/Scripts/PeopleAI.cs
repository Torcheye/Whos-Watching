using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public enum MoveState
{
    Idle,
    Wander,
    Chase
}

public class PeopleAI : MonoBehaviour
{
    public Vector2 idleTime;
    public float wanderWalkDist;
    public float wanderAimAngle;
    public float viewAngle;
    public float notVisibleTime;
    public bool isEnemy;
    
    private Animator _animator;
    private NavMeshAgent _agent;
    private Camera _cam;
    private float _stateTimer;
    private float _currentIdleTime;
    private bool _playerInSight;
    private float _notVisibleTimer;
    private bool _notVisibleOvershooting;
    private float _originSpeed;

    protected MoveState MoveState
    {
        get => _moveState;
        set => OnMoveStateChanged(value);
    }
    private MoveState _moveState;
    
    private static readonly int Velocity = Animator.StringToHash("Velocity");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _cam = GetComponentInChildren<Camera>();
    }

    protected virtual void Start()
    {
        _originSpeed = _agent.speed;
        MoveState = MoveState.Idle;
    }

    private void Update()
    {
        UpdateMoveState();
        
        _notVisibleTimer += Time.deltaTime;
        _stateTimer += Time.deltaTime;

        var transform1 = transform;
        var playerDir = GameManager.Instance.player.transform.position - transform1.position;
        var sees = Vector3.Angle(transform1.forward, playerDir) < viewAngle;

        if (sees)
            sees &= !Physics.Raycast(transform1.position + Vector3.up * 1.8f,
                playerDir, playerDir.magnitude, LayerMask.GetMask("Wall"));
        
        if (!_playerInSight && sees)
        {
            if (!_notVisibleOvershooting)
                GameManager.Instance.AddToDisplayList(this);
            _notVisibleOvershooting = false;
        }
        else if (_playerInSight && !sees) 
        {
            _notVisibleTimer = 0;
            _notVisibleOvershooting = true;
        }
        _playerInSight = sees;

        if (_notVisibleOvershooting && !sees)
        {
            if (MoveState != MoveState.Chase)
            {
                if (_notVisibleTimer > notVisibleTime)
                {
                    GameManager.Instance.RemoveFromDisplayList(this);
                    _notVisibleOvershooting = false;
                    _cam.transform.localRotation = Quaternion.identity; 
                }
            }
            else
            {
                if (_notVisibleTimer > notVisibleTime * 3)
                    MoveState = MoveState.Idle;
            }
        }
        
        if (isEnemy && sees && MoveState != MoveState.Chase)
        {
            MoveState = MoveState.Chase;
        }

        if (_playerInSight)
        {
            var dest = Vector3.Lerp(_cam.transform.TransformPoint(Vector3.forward),
                GameManager.Instance.player.transform.position + 1.8f * Vector3.up,
                0.35f * Time.deltaTime);
            _cam.transform.LookAt(dest);
        }
    }

    private void UpdateMoveState()
    {
        if (MoveState == MoveState.Idle)
        {
            if (_stateTimer >= _currentIdleTime)
            {
                MoveState = MoveState.Wander;
            }
        }
        else if (MoveState == MoveState.Wander)
        {
            if (!_agent.pathPending && 
                _agent.remainingDistance <= _agent.stoppingDistance
                && (!_agent.hasPath || _agent.velocity.sqrMagnitude < 0.1f) )
            {
                MoveState = MoveState.Idle;
            }

            if (_stateTimer > wanderWalkDist / _agent.speed + 1)
                _moveState = MoveState.Idle;
        }
        else if (MoveState == MoveState.Chase)
        {
            _agent.SetDestination(GameManager.Instance.player.transform.position);
        }

        _animator.SetFloat(Velocity, _agent.velocity.magnitude);
        _animator.SetBool(IsMoving, _agent.velocity.sqrMagnitude > 0);
    }

    private void OnMoveStateChanged(MoveState newState)
    {
        if (newState == MoveState.Idle)
        {
            _agent.speed = _originSpeed;
            _agent.ResetPath();
            _currentIdleTime = Random.Range(idleTime.x, idleTime.y);
        }
        else if (newState == MoveState.Wander)
        {
            _agent.speed = _originSpeed;
            var pos = transform.position;
            var playerDir = Vector3.Normalize(GameManager.Instance.player.transform.position - pos);
            var target = Quaternion.Euler(0, Random.Range(-wanderAimAngle, wanderAimAngle), 0) 
                         * playerDir * wanderWalkDist;

            _agent.SetDestination(pos + target);
        }
        else if (newState == MoveState.Chase)
        {
            _agent.speed = _originSpeed * 1.3f;
        }

        _moveState = newState;
        _stateTimer = 0;
    }
}
