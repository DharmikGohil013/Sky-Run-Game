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
    [SerializeField] private float rollHeight = 1f; // Height while rolling
    [SerializeField] private Vector3 rollCenter = new Vector3(0, 0.5f, 0); // Center while rolling

    private float normalHeight;
    private Vector3 normalCenter;

    private CharacterController controller;
    private Animator animator;

    private Vector3 moveDirection;

    private int currentLane = 1;
    // 0 = Left
    // 1 = Middle
    // 2 = Right

    private bool isRolling;
    private float rollTimer;

    private bool gameStarted;
    private bool isDead;

    // --- Input Variables ---
    private bool leftTriggered;
    private bool rightTriggered;
    private bool jumpTriggered;
    private bool rollTriggered;

    [Header("Mobile Swipe Settings")]
    [SerializeField] private float swipeThreshold = 50f;
    private Vector2 startTouchPos;
    private bool swipeProcessed;

    // =========================================
    // UNITY
    // =========================================

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        // Store original CharacterController dimensions
        normalHeight = controller.height;
        normalCenter = controller.center;

        // Start In Idle
        if (animator != null)
        {
            // Crucial: Disable root motion so jump animations don't physically lock the player in place
            animator.applyRootMotion = false;
            animator.SetBool("IsRunning", false);
        }
    }

    private void Update()
    {
        // Completely stop processing inputs and logic if the game is paused/stopped
        if (Time.timeScale == 0f)
            return;

        // START GAME
        if (!gameStarted)
        {
            // The game will only start when UIManager's OnPlayButton calls StartRunning()
            return;
        }

        if (!isDead)
        {
            CheckInput();
            LaneMovement();
            JumpSystem();
            RollSystem();
        }

        // ForwardMovement must still run so gravity can pull a dead player to the ground
        ForwardMovement();

        // FIX ROTATION: Ensure Y rotation is always exactly 0
        Vector3 currentRot = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(currentRot.x, 0f, currentRot.z);

        if (!isDead)
        {
            ResetInput();
        }
    }

    // =========================================
    // INPUT HANDLING
    // =========================================

    private void CheckInput()
    {
        // Keyboard
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) leftTriggered = true;
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) rightTriggered = true;
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) jumpTriggered = true;
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) rollTriggered = true;

        // Mobile Touch Swipe
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
                Vector2 diff = touch.position - startTouchPos;
                if (diff.magnitude > swipeThreshold)
                {
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
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                swipeProcessed = false;
            }
        }
        
        // Editor Mouse Swipe fallback
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
            swipeProcessed = false;
        }
        else if (Input.GetMouseButton(0) && !swipeProcessed)
        {
            Vector2 diff = (Vector2)Input.mousePosition - startTouchPos;
            if (diff.magnitude > swipeThreshold)
            {
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
        }
        else if (Input.GetMouseButtonUp(0))
        {
            swipeProcessed = false;
        }
    }

    private void ResetInput()
    {
        leftTriggered = false;
        rightTriggered = false;
        jumpTriggered = false;
        rollTriggered = false;
    }

    // =========================================
    // START RUN
    // =========================================

    public void StartRunning()
    {
        gameStarted = true;

        if (animator != null)
        {
            animator.SetBool("IsRunning", true);
        }
    }

    // =========================================
    // LANE MOVEMENT
    // =========================================

    private void LaneMovement()
    {
        // LEFT
        if (leftTriggered)
        {
            currentLane--;

            if (currentLane < 0)
                currentLane = 0;
        }

        // RIGHT
        if (rightTriggered)
        {
            currentLane++;

            if (currentLane > 2)
                currentLane = 2;
        }

        // TARGET POSITION
        float targetX = 0f;
        if (currentLane == 0)
        {
            targetX = leftLaneX;
        }
        else if (currentLane == 2)
        {
            targetX = rightLaneX;
        }

        // EXACT POSITION LOCK (No drifting in any other direction)
        float diffX = targetX - transform.position.x;
        
        if (Mathf.Abs(diffX) < 0.05f)
        {
            // Snap perfectly to the exact lane coordinate to prevent infinite float drifting
            Vector3 exactPos = transform.position;
            exactPos.x = targetX;
            transform.position = exactPos;
            
            moveDirection.x = 0f;
        }
        else
        {
            // Smoothly move towards the lane
            moveDirection.x = diffX * laneSmoothSpeed;
        }
    }

    // =========================================
    // FORWARD MOVEMENT
    // =========================================

    private void ForwardMovement()
    {
        moveDirection.z = moveSpeed;

        if (!controller.isGrounded)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }

        // Move applies X (Lane), Y (Jump/Gravity), and Z (Forward) velocities
        controller.Move(moveDirection * Time.deltaTime);
    }

    // =========================================
    // JUMP SYSTEM
    // =========================================

    private void JumpSystem()
    {
        // Tell the animator if we are grounded or in the air, allowing it to transition back to running smoothly
        if (animator != null)
        {
            animator.SetBool("IsGrounded", controller.isGrounded);
        }

        if (!controller.isGrounded)
            return;

        moveDirection.y = -1f;

        if (jumpTriggered)
        {
            moveDirection.y = jumpForce;

            // Cancel roll early if we jump
            if (isRolling)
            {
                isRolling = false;
                controller.height = normalHeight;
                controller.center = normalCenter;
            }

            if (animator != null)
            {
                animator.SetTrigger("Jump");
                // Also trigger RunningJump just in case you named your new animation trigger this!
                animator.SetTrigger("RunningJump"); 
            }
        }
    }

    // =========================================
    // ROLL SYSTEM
    // =========================================

    private void RollSystem()
    {
        // START ROLL
        if (rollTriggered)
        {
            if (!isRolling)
            {
                isRolling = true;
                rollTimer = rollDuration;

                // Shrink collider to physically go under obstacles
                controller.height = rollHeight;
                controller.center = rollCenter;

                if (animator != null)
                {
                    animator.SetTrigger("Roll");
                }
            }
        }

        // ROLL TIMER
        if (isRolling)
        {
            rollTimer -= Time.deltaTime;

            if (rollTimer <= 0f)
            {
                isRolling = false;
                
                // Restore original collider size when roll is over
                controller.height = normalHeight;
                controller.center = normalCenter;
            }
        }
    }

    // =========================================
    // HIT OBSTACLE
    // =========================================

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            // If the power is active, we are invincible!
            if (UIManager.Instance != null && UIManager.Instance.IsPowerActive)
            {
                Debug.Log("Smashed obstacle with power!");
                
                // Destroy the obstacle so the player visually smashes right through it
                Destroy(other.gameObject);
            }
            else
            {
                Debug.Log("Hit by: " + other.gameObject.name);
                HitObstacle();
            }
        }
        else if (other.CompareTag("Coin"))
        {
            // Do not collect coins if the player is dead or the game is paused!
            if (isDead || Time.timeScale == 0f)
                return;

            // Destroy the collected coin
            Destroy(other.gameObject);

            // Add +1 to total coins
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddCoins(1);
            }
        }
    }

    private void HitObstacle()
    {
        // Don't die before the game has even started!
        if (!gameStarted || isDead)
            return;

        isDead = true;

        // Stop Running Animation
        if (animator != null)
        {
            animator.SetBool("IsRunning", false);

            animator.SetTrigger("Hit");
        }

        // Stop Movement
        moveSpeed = 0;

        // Show Game Over
        Invoke(nameof(ShowGameOver), 2f);
    }

    private void ShowGameOver()
    {
        UIManager.Instance.ShowGameOver();
    }

    // =========================================
    // OPTIONAL DEBUG
    // =========================================

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(
            new Vector3(leftLaneX, transform.position.y, transform.position.z),
            new Vector3(leftLaneX, transform.position.y, transform.position.z + 5f));

        Gizmos.DrawLine(
            new Vector3(0f, transform.position.y, transform.position.z),
            new Vector3(0f, transform.position.y, transform.position.z + 5f));

        Gizmos.DrawLine(
            new Vector3(rightLaneX, transform.position.y, transform.position.z),
            new Vector3(rightLaneX, transform.position.y, transform.position.z + 5f));
    }
}