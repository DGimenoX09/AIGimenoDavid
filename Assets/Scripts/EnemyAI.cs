using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; 

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patrolling, 
        Chasing,  
        Searching,
        Waiting,   // Nuevo estado
        Attacking   // Nuevo estado
    }
    
    public EnemyState currentState; 

    private NavMeshAgent _AIAgent; 
    private Transform _playerTransform; 

    // Puntos de control
    [SerializeField] Transform[] _patrolPoints; 
    private int _currentPatrolIndex = 0; // Índice del punto de patrullaje actual

    [SerializeField] Vector2 _patrolAreaSize = new Vector2(5, 5);
    [SerializeField] Transform _patrolAreaCenter; 

    // Cosas detección 
    [SerializeField] float _visionRange = 20;
    [SerializeField] float _visionAngle = 120; 
    private Vector3 _playerLastPosition; 

    // Cosas búsqueda
    float _searchTimer; 
    float _searchWaitTime = 15; 
    float _searchRadius = 10; 

    // Rango de ataque
    [SerializeField] float _attackRange = 2.0f; // Rango en el que el enemigo puede atacar

    void Awake()
    {
        _AIAgent = GetComponent<NavMeshAgent>(); 
        _playerTransform = GameObject.FindWithTag("Player").transform; 
    }

    void Start()
    {
        currentState = EnemyState.Patrolling; 
        SetPatrolPoint(); 
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyState.Chasing:
                Chase(); 
                break; 
            case EnemyState.Searching: 
                Search(); 
                break;
            case EnemyState.Waiting:
                Wait();
                break;
            case EnemyState.Attacking:
                Attack();
                break;
        }
    }

    void Patrol()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing; 
            return; // Salir del método para evitar seguir patrullando
        }

        if (_AIAgent.remainingDistance < 0.5f)
        {
            currentState = EnemyState.Waiting; // Cambia al estado Waiting al llegar al punto
            StartCoroutine(WaitAndPatrol()); // Inicia la coroutine para esperar
        }
    }

    void Chase()
    {
        if (!OnRange())
        {
            currentState = EnemyState.Searching; 
        }
        else if (Vector3.Distance(transform.position, _playerTransform.position) <= _attackRange)
        {
            currentState = EnemyState.Attacking; // Cambia al estado de ataque si está dentro del rango
        }

        _AIAgent.destination = _playerTransform.position; 
    }

    void Search()
    {
        if (OnRange())
        {
            currentState = EnemyState.Chasing; 
        }

        _searchTimer += Time.deltaTime; 

        if (_searchTimer < _searchWaitTime)
        {
            if (_AIAgent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint; 
                if (RandomSearchPoint(_playerLastPosition, _searchRadius, out randomPoint))
                {
                    _AIAgent.destination = randomPoint; 
                }
            }
        }
        else
        {
            currentState = EnemyState.Patrolling; 
            _searchTimer = 0; 
        }
    }

    void Wait()
    {
        // Este método no necesita lógica adicional, ya que la espera se maneja en la coroutine
    }

    void Attack()
    {
        // Simular ataque aquí
        Debug.Log("Atacando al jugador!");

        // Después de atacar, volver al estado Chasing
        currentState = EnemyState.Chasing; 
    }

    IEnumerator WaitAndPatrol()
    {
        yield return new WaitForSeconds(5); // Espera 5 segundos
        _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length; // Cambia al siguiente punto
        SetPatrolPoint(); 
        currentState = EnemyState.Patrolling; // Regresa al estado Patrolling
    }

    bool RandomSearchPoint(Vector3 center, float radius, out Vector3 point)
    {
        Vector3 randomPoint = center + Random.insideUnitSphere * radius; 
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(center, out hit, 4, NavMesh.AllAreas))
        {
            point = hit.position; 
            return true; 
        }

        point = Vector3.zero; 
        return false; 
    }

    bool OnRange()
    {
        Vector3 directionToPlayer = _playerTransform.position - transform.position; 
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer); 
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position); 

        if (_playerTransform.position == _playerLastPosition)
        {
            return true; 
        }
        if (distanceToPlayer > _visionRange)
        {
            return false; 
        }

        if (angleToPlayer > _visionAngle * 0.5f)
        {
            return false; 
        }

        RaycastHit hit; 
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                _playerLastPosition = _playerTransform.position; 
                return true; 
            }
            else
            {
                return false; 
            }
        }

        return true; 
    }

    void SetPatrolPoint()
    {
        if (_patrolPoints.Length > 0)
        {
            _AIAgent.destination = _patrolPoints[_currentPatrolIndex].position; 
        }
    }

    void OnDrawGizmos() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_patrolAreaCenter.position, new Vector3(_patrolAreaSize.x, 1, _patrolAreaSize.y));

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _visionRange);

        Gizmos.color = Color.magenta;

        Vector3 fovLine1 = Quaternion.AngleAxis(_visionAngle * 0.5f, transform.up) * transform.forward * _visionRange; 
        Vector3 fovLine2 = Quaternion.AngleAxis(-_visionAngle * 0.5f, transform.up) * transform.forward * _visionRange; 

        Gizmos.DrawLine(transform.position, transform.position + fovLine1); 
        Gizmos.DrawLine(transform.position, transform.position + fovLine2); 
    }
}