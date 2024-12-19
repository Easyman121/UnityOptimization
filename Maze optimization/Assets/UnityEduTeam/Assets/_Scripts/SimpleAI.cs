using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class SimpleAI : MonoBehaviour {
    [Header("Agent Field of View Properties")]
    public float viewRadius;
    public float viewAngle;

    public LayerMask playerMask;
    public LayerMask obstacleMask;

    [Header("Agent Properties")]
    public float runSpeed;
    public float walkSpeed;
    public float patrolRadius;

    private NavMeshAgent agent;
    private Animator anim;
    private Transform playerTarget;
    private Vector3 currentDestination;
    private bool playerSeen;

    private enum State { Wandering, Chasing }
    private State currentState;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        SetRandomDestination();
        currentState = State.Wandering;

        // Запускаем корутину
        StartCoroutine(UpdateAIState());
    }

    // Корутина для обновления состояния через интервалы
    IEnumerator UpdateAIState() {
        while (true) {
            CheckState();

            if (playerSeen) {
                currentState = State.Chasing;
            } else {
                currentState = State.Wandering;
            }

            // Ждём 0.2 секунды перед следующим обновлением
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void CheckState() {
        FindVisibleTargets();

        if (currentState == State.Chasing) {
            ChaseBehavior();
        } else {
            WanderBehavior();
        }
    }

    private void WanderBehavior() {
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending) {
            SetRandomDestination();
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Run")) {
            anim.SetTrigger("walk");
        }

        agent.speed = walkSpeed;
    }

    private void ChaseBehavior() {
        if (playerTarget != null) {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Run")) {
                anim.SetTrigger("run");
            }

            agent.speed = runSpeed;
            agent.SetDestination(playerTarget.position);
        } else {
            playerSeen = false;
            currentState = State.Wandering;
        }
    }

    private void SetRandomDestination() {
        currentDestination = RandomNavSphere(transform.position, patrolRadius, -1);
        agent.SetDestination(currentDestination);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
    }

    #region Vision
    private void FindVisibleTargets() {
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, playerMask);

        playerTarget = null;
        playerSeen = false;

        foreach (var target in targetsInViewRadius) {
            Transform targetTransform = target.transform;
            Vector3 dirToTarget = (targetTransform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2) {
                float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

                if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask)) {
                    playerSeen = true;
                    playerTarget = targetTransform;
                    return;
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
        if (!angleIsGlobal) {
            angleInDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float distance, int layerMask) {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;

        NavMesh.SamplePosition(randomDirection, out NavMeshHit navHit, distance, layerMask);

        return navHit.position;
    }
    #endregion
}
