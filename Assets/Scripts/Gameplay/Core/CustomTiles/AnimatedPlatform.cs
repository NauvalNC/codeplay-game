using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimatedPlatform : MonoBehaviour
{
    [SerializeField] private float animationDelay = 2f;
    private Animator animator;
    private float delay;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        delay = animationDelay;
        animator.Play("AnimatedPlatform");
    }

    private void Update()
    {
        delay -= Time.deltaTime;
        if (delay <= 0)
        {
            delay = animationDelay;
            animator.Play("AnimatedPlatform");
        }
    }
}