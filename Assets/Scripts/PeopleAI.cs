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
    public float viewAngle;
    public float notVisibleTime;
    
    private Animator _animator;
    private NavMeshAgent _agent;
    private float _stateTimer;
    private float _currentIdleTime;
    private bool _playerInSight;
    private float _notVisibleTimer;
    private bool _notVisibleOvershooting;

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
    }

    protected virtual void Start()
    {
        MoveState = MoveState.Idle;
    }

    private void Update()
    {
        UpdateMoveState();

        _notVisibleTimer += Time.deltaTime;
        _stateTimer += Time.deltaTime;

        var playerDir = GameManager.Instance.player.transform.position - transform.position;
        var sees = Vector3.Angle(transform.forward, playerDir) < viewAngle;
        if (sees)
            sees &= !Physics.Raycast(transform.position, playerDir, playerDir.magnitude,
                LayerMask.GetMask("Wall"));

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

        if (_notVisibleOvershooting && _notVisibleTimer > notVisibleTime && !sees)
        {
            GameManager.Instance.RemoveFromDisplayList(this);
            _notVisibleOvershooting = false;
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

            if (_stateTimer > 8)
                _moveState = MoveState.Idle;
        }
        else if (MoveState == MoveState.Chase)
        {
            _agent.SetDestination(GameManager.Instance.player.transform.position);
        }

        _animator.SetFloat(Velocity, _agent.velocity.magnitude / _agent.speed);
        _animator.SetBool(IsMoving, _agent.velocity.sqrMagnitude > 0);
    }

    private void OnMoveStateChanged(MoveState newState)
    {
        if (newState == MoveState.Idle)
        {
            _agent.ResetPath();
            _currentIdleTime = Random.Range(idleTime.x, idleTime.y);
        }
        else if (newState == MoveState.Wander)
        {
            var randomPoint = (Vector2)transform.position + Random.insideUnitCircle * wanderWalkDist;
            var target = new Vector3(randomPoint.x, 0, randomPoint.y);
            _agent.SetDestination(target);
        }

        _moveState = newState;
        _stateTimer = 0;
    }
}
