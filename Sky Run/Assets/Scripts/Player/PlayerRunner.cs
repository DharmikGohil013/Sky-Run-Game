using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerRunner : MonoBehaviour
{
    [Header("Forward Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;

    [Header("Lane Movement")]
    [SerializeField] private float leftLaneX = -2.77f;
    [SerializeField] private float rightLaneX = 3.12f;
    [SerializeField] private float laneSmoothSpeed = 10f;

    [Header("Roll")]
    [SerializeField] private float rollDuration = 1f;
    [SerializeField] private float rollHeight = 1f;
    [SerializeField] private Vector3 rollCenter = new Vector3(0, 0.5f, 0);

    [Header("Mobile Swipe Settings")]
    [SerializeField] private float swipeThreshold = 50f;

    private CharacterController controller;
    private Animator animator;

    private Vector3 moveDirection;
    private float normalHeight;
    private Vector3 normalCenter;

    private int currentLane = 1;

    private bool isRolling;
    private float rollTimer;
    private bool gameStarted;
    private bool isDead;

    private bool leftTriggered;
    private bool rightTriggered;
    private bool jumpTriggered;
    private bool rollTriggered;

    private Vector2 startTouchPos;
    private bool swipeProcessed;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        normalHeight = controller.height;
        normalCenter = controller.center;

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetBool("IsRunning", false);
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        if (!gameStarted) return;

        if (!isDead)
        {
            CheckInput();
            LaneMovement();
            JumpSystem();
            RollSystem();
        }

        ForwardMovement();

        Vector3 rot = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(rot.x, 0f, rot.z);

        if (!isDead) ResetInput();
    }

    private void CheckInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) leftTriggered = true;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) rightTriggered = true;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) jumpTriggered = true;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) rollTriggered = true;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                startTouchPos = touch.position;
                swipeProcessed = false;
            }
            else if (touch.phase == TouchPhase.Moved && !swipeProcessed)
            {
                ProcessSwipe(touch.position - startTouchPos);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                swipeProcessed = false;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            swipeProcessed = false;
        }
        else if (Input.GetMouseButton(0) && !swipeProcessed)
        {
            ProcessSwipe((Vector2)Input.mousePosition - startTouchPos);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            swipeProcessed = false;
        }
    }

    private void ProcessSwipe(Vector2 diff)
    {
        if (diff.magnitude < swipeThreshold) return;

        if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
        {
            if (diff.x > 0) rightTriggered = true;
            else leftTriggered = true;
        }
        else
        {
            if (diff.y > 0) jumpTriggered = true;
            else rollTriggered = true;
        }

        swipeProcessed = true;
    }

    private void ResetInput()
    {
        leftTriggered = false;
        rightTriggered = false;
        jumpTriggered = false;
        rollTriggered = false;
    }

    public void StartRunning()
    {
        gameStarted = true;
        animator?.SetBool("IsRunning", true);
    }

    private void LaneMovement()
    {
        if (leftTriggered) currentLane = Mathf.Max(0, currentLane - 1);
        if (rightTriggered) currentLane = Mathf.Min(2, currentLane + 1);

        float targetX = currentLane == 0 ? leftLaneX : currentLane == 2 ? rightLaneX : 0f;
        float diffX = targetX - transform.position.x;

        if (Mathf.Abs(diffX) < 0.05f)
        {
            Vector3 pos = transform.position;
            pos.x = targetX;
            transform.position = pos;
            moveDirection.x = 0f;
        }
        else
        {
            moveDirection.x = diffX * laneSmoothSpeed;
        }
    }

    private void ForwardMovement()
    {
        moveDirection.z = moveSpeed;

        if (!controller.isGrounded)
            moveDirection.y += gravity * Time.deltaTime;

        controller.Move(moveDirection * Time.deltaTime);
    }

    private void JumpSystem()
    {
        animator?.SetBool("IsGrounded", controller.isGrounded);

        if (!controller.isGrounded) return;

        moveDirection.y = -1f;

        if (!jumpTriggered) return;

        moveDirection.y = jumpForce;

        if (isRolling)
        {
            isRolling = false;
            controller.height = normalHeight;
            controller.center = normalCenter;
        }

        if (animator != null)
        {
            animator.SetTrigger("Jump");
            animator.SetTrigger("RunningJump");
        }
    }

    private void RollSystem()
    {
        if (rollTriggered && !isRolling)
        {
            isRolling = true;
            rollTimer = rollDuration;
            controller.height = rollHeight;
            controller.center = rollCenter;
            animator?.SetTrigger("Roll");
        }

        if (isRolling)
        {
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0f)
            {
                isRolling = false;
                controller.height = normalHeight;
                controller.center = normalCenter;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            if (UIManager.Instance != null && UIManager.Instance.IsPowerActive)
            {
                Destroy(other.gameObject);
            }
            else
            {
                HitObstacle();
            }
        }
        else if (other.CompareTag("Coin"))
        {
            if (isDead || Time.timeScale == 0f) return;

            Destroy(other.gameObject);
            UIManager.Instance?.AddCoins(1);
        }
    }

    private void HitObstacle()
    {
        if (!gameStarted || isDead) return;

        isDead = true;
        moveSpeed = 0;

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetTrigger("Hit");
        }

        Invoke(nameof(ShowGameOver), 2f);
    }

    private void ShowGameOver() => UIManager.Instance.ShowGameOver();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        void DrawLane(float x) => Gizmos.DrawLine(
            new Vector3(x, transform.position.y, transform.position.z),
            new Vector3(x, transform.position.y, transform.position.z + 5f));

        DrawLane(leftLaneX);
        DrawLane(0f);
        DrawLane(rightLaneX);
    }
}