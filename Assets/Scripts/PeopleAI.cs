using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

enum MoveState
{
    Idle,
    Wander,
    Chase
}
    
enum LookState
{
    Idle,
    Look
}

public class PeopleAI : MonoBehaviour
{
    public Vector2 idleTime;
    public float wanderWalkDist;
    
    private Animator _animator;
    private LookState _lookState;
    private NavMeshAgent _agent;
    private Camera _camera;
    private float _stateTimer;
    private float _currentIdleTime;
    private bool _playerInSight;

    private MoveState MoveState
    {
        get => _moveState;
        set => OnMoveStateChanged(value);
    }
    private MoveState _moveState;
    
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _camera = GetComponentInChildren<Camera>();
    }

    private void Start()
    {
        MoveState = MoveState.Wander;
        _lookState = LookState.Look;
    }

    private void Update()
    {
        //UpdateLookState();
        UpdateMoveState();

        var sees = Vector3.Dot(transform.forward,
            GameManager.Instance.player.transform.position - transform.position) > 0;
        if (!_playerInSight && sees)
        {
            GameManager.Instance.AddToDisplayList(this);
        }
        else if (_playerInSight && !sees)
        {
            GameManager.Instance.RemoveFromDisplayList(this);
        }
        _playerInSight = sees;
    }

    private void UpdateLookState()
    {
        if (_lookState == LookState.Idle)
        {
            
        }
        else if (_lookState == LookState.Look)
        {
            _camera.transform.LookAt(GameManager.Instance.player.transform.position);
        }
    }

    private void UpdateMoveState()
    {
        _stateTimer += Time.deltaTime;
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
        }
        else if (MoveState == MoveState.Chase)
        {
            _agent.SetDestination(GameManager.Instance.player.transform.position);
        }

        if (_agent.velocity.sqrMagnitude > 0.1f)
        {
            _animator.SetFloat("VelocityX", _agent.velocity.x);
            _animator.SetFloat("VelocityY", _agent.velocity.y);
            _animator.SetBool("IsWalking", true);
        }
        else
        {
            _animator.SetBool("IsWalking", false);
        }
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
            var randomPoint = Random.insideUnitCircle * wanderWalkDist;
            var target = new Vector3(randomPoint.x, 0, randomPoint.y);
            _agent.SetDestination(target);
        }

        _moveState = newState;
        _stateTimer = 0;
    }
}
