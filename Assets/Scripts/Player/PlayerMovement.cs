using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 5f;   // 이동 속도

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    // 입력값
    private Vector2 input;     // 원본 입력 (WASD)
    private Vector2 moveDir;   // 실제 이동 방향
    private Vector2 animDir;   // 애니메이션용 방향
    private Vector2 lastMoveDir = Vector2.down; // 마지막 이동 방향
    
    // 수직 입력 충돌 처리용 (W/S)
    private bool wasWPressed = false;
    private bool wasSPressed = false;
    private float lastVerticalInput = 0f; // 마지막 수직 입력값
    
    // 수평 입력 충돌 처리용 (A/D)
    private bool wasAPressed = false;
    private bool wasDPressed = false;
    private float lastHorizontalInput = 0f; // 마지막 수평 입력값

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        // 수평 입력 개별 처리 (A/D 충돌 방지)
        bool aPressed = Input.GetKey(KeyCode.A);
        bool dPressed = Input.GetKey(KeyCode.D);
        
        // A와 D가 동시에 눌려있을 때, 먼저 누른 키만 적용
        if (aPressed && dPressed)
        {
            // 이전 프레임에서 A가 눌려있었고 D가 안 눌려있었는데, 이번에 D도 눌렸으면 → A 유지
            // 이전 프레임에서 D가 눌려있었고 A가 안 눌려있었는데, 이번에 A도 눌렸으면 → D 유지
            if (wasAPressed && !wasDPressed)
            {
                input.x = -1f; // A 유지
            }
            else if (wasDPressed && !wasAPressed)
            {
                input.x = 1f; // D 유지
            }
            else
            {
                // 둘 다 이전에도 눌려있었으면 이전 상태 유지
                input.x = lastHorizontalInput;
            }
        }
        else if (aPressed)
        {
            input.x = -1f; // A만
        }
        else if (dPressed)
        {
            input.x = 1f; // D만
        }
        else
        {
            input.x = 0f; // 둘 다 안 눌림
        }
        
        // 이전 프레임 상태 저장
        wasAPressed = aPressed;
        wasDPressed = dPressed;
        lastHorizontalInput = input.x;
        
        // 수직 입력 개별 처리 (W/S 충돌 방지)
        bool wPressed = Input.GetKey(KeyCode.W);
        bool sPressed = Input.GetKey(KeyCode.S);
        
        // W와 S가 동시에 눌려있을 때, 먼저 누른 키만 적용
        if (wPressed && sPressed)
        {
            // 이전 프레임에서 W가 눌려있었고 S가 안 눌려있었는데, 이번에 S도 눌렸으면 → W 유지 (S 무시)
            // 이전 프레임에서 S가 눌려있었고 W가 안 눌려있었는데, 이번에 W도 눌렸으면 → S 유지 (W 무시)
            if (wasWPressed && !wasSPressed)
            {
                input.y = 1f; // W 유지
            }
            else if (wasSPressed && !wasWPressed)
            {
                input.y = -1f; // S 유지
            }
            else
            {
                // 둘 다 이전에도 눌려있었으면 이전 상태 유지
                input.y = lastVerticalInput;
            }
        }
        else if (wPressed)
        {
            input.y = 1f; // W만
        }
        else if (sPressed)
        {
            input.y = -1f; // S만
        }
        else
        {
            input.y = 0f; // 둘 다 안 눌림
        }
        
        // 이전 프레임 상태 저장
        wasWPressed = wPressed;
        wasSPressed = sPressed;
        lastVerticalInput = input.y;

        // 실제 이동 방향 (대각선 포함 8방향)
        if (input.sqrMagnitude > 0.01f)
        {
            moveDir = input.normalized;
            lastMoveDir = moveDir; // 이동 중일 때 마지막 방향 저장
        }
        else
        {
            moveDir = Vector2.zero;
        }

        float speed = input.magnitude; // 0 = 멈춤, 1 = 한 방향, √2 = 대각선

        // 애니메이션용 방향 
        // 수평 입력이 있으면 수평 방향 우선, 없으면 수직 방향 사용
        if (moveDir.sqrMagnitude > 0.01f)
        {
            // 수평 입력이 있으면 항상 수평 방향 애니메이션 사용
            if (Mathf.Abs(input.x) > 0.01f)
            {
                // Right 애니메이션 사용 (flipX로 Left 표현)
                animDir = new Vector2(1f, 0f);
            }
            else
            {
                // 수직 방향만
                animDir = new Vector2(0f, Mathf.Sign(moveDir.y));
            }
        }
        else
        {
            // 멈췄을 때는 마지막 방향을 4방향으로 변환
            if (Mathf.Abs(lastMoveDir.x) > 0.01f)
            {
                animDir = new Vector2(1f, 0f); // Right 애니메이션 사용
            }
            else
            {
                animDir = new Vector2(0f, Mathf.Sign(lastMoveDir.y));
            }
        }

        // Animator 파라미터 (Left는 없으므로 Right만 사용, flipX로 처리)
        anim.SetFloat("MoveX", animDir.x);
        anim.SetFloat("MoveY", animDir.y);
        anim.SetFloat("Speed", speed > 0.01f ? 1f : 0f); // 이동 중이면 1, 멈춤이면 0

        // flipX
        if (input.x < 0)
        {
            sr.flipX = true;   // 왼쪽 바라보는 것처럼
        }
        else if (input.x > 0)
        {
            sr.flipX = false;  // 오른쪽
        }
    }

    void FixedUpdate()
    {
        // 실제 이동 
        Vector2 finalDir = moveDir;

        rb.MovePosition(rb.position + finalDir * moveSpeed * Time.fixedDeltaTime);
    }

}
