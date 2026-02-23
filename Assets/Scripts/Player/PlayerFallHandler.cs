using UnityEngine;
using System;

/// <summary>
/// 06:00 시퀀스 시 플레이어 엎어짐 처리
/// DayEndSequenceManager가 NotifyFall 호출 → 여기서 수신
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerFallHandler : MonoBehaviour
{
    public static event Action<bool> OnFallRequested;

    public static void NotifyFall(bool fall)
    {
        OnFallRequested?.Invoke(fall);
    }

    private PlayerMovement movement;
    private Animator anim;
    private SpriteRenderer sr;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        OnFallRequested += HandleFall;
    }

    void OnDisable()
    {
        OnFallRequested -= HandleFall;
    }

    void HandleFall(bool fall)
    {
        if (movement != null)
            movement.enabled = !fall;

        // TODO: 엎어짐 애니메이션/스프라이트 전환 (작업필요)
        // 예: anim.SetBool("Fallen", fall); 또는 스프라이트 교체
        if (anim != null && anim.parameters.Length > 0)
        {
            foreach (var p in anim.parameters)
                if (p.name == "Fallen") { anim.SetBool("Fallen", fall); break; }
        }
    }
}
