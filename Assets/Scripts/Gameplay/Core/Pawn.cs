using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : MonoBehaviour
{
    public delegate void OnPawnMove();

    private struct FPawnData
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    [Header("Generics")]
    [SerializeField] private float moveStep = 1f;
    [SerializeField] private float rotateDegStep = 90f;
    [SerializeField] private float transitionSpeed = 0.1f;
    [SerializeField] private bool useGravity = false;
    private FPawnData initialData;
    private FPawnData destinationData;

    public OnPawnMove OnPawnMoveDelegate;

    protected Animator animator;

    private void Awake()
    {
        OnAwake();
    }

    private void Update()
    {
        OnUpdate();
    }

    protected virtual void OnAwake()
    {
        animator = GetComponent<Animator>();

        initialData.position = transform.position;
        initialData.rotation = transform.rotation;
        destinationData = initialData;
    }

    protected virtual void OnUpdate()
    {

    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    protected virtual void OnFixedUpdate()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, destinationData.rotation, transitionSpeed);

        if (useGravity && transform.position.y <= -0.01f) return;

        transform.position = Vector3.Lerp(transform.position, destinationData.position, transitionSpeed);

        animator?.SetBool("IsWalking", CodeManager.Instance.IsExecuting);
    }

    public void MoveToward(Vector3 direction)
    {
        destinationData.position = transform.position + direction.normalized * moveStep;
        OnPawnMoveDelegate?.Invoke();
    }

    public void MoveForward()
    {
        destinationData.position = transform.position + transform.forward * moveStep;
        OnPawnMoveDelegate?.Invoke();
    }

    public void RotateLeft()
    {
        destinationData.rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, -rotateDegStep, 0f));
        OnPawnMoveDelegate?.Invoke();
    }

    public void RotateRight()
    {
        destinationData.rotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0f, rotateDegStep, 0f));
        OnPawnMoveDelegate?.Invoke();
    }

    public virtual void ResetPawn()
    {
        transform.position = initialData.position;
        transform.rotation = initialData.rotation;
        destinationData = initialData;
    }
}
