using UnityEngine;

public class PlayerController : Pawn
{
    [Header("Player")]
    [SerializeField] private AudioSource moveSound;
    [SerializeField] private SkinObjectList skins;
    [SerializeField] private Transform skinContainer;

    [Header("Raycasting")]
    [SerializeField] private Transform centerRaycast;
    [SerializeField] private Transform frontDownRaycast;
    [SerializeField] private Transform leftDownRaycast;
    [SerializeField] private Transform rightDownRaycast;

    private bool hasObstacleAhead = false;
    public bool HasObstracleAhead { get { return hasObstacleAhead; } }


    private bool canGoLeft = false;
    public bool CanGoLeft { get { return canGoLeft; } }


    private bool canGoRight = false;
    public bool CanGoRight { get { return canGoRight; } }


    private bool isResetLevelRequested = false;

    protected override void OnAwake()
    {
        base.OnAwake();

        LoadSkin();

        if (!SoundManager.Instance.IsMuted())
        {
            OnPawnMoveDelegate += moveSound.Play;
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        CheckIsFalling();
    }

    protected override void OnFixedUpdate()
    {
        base.OnFixedUpdate();
        CheckHasObstacleAhead();
    }

    private void LoadSkin()
    {
        // Get default skin object.
        SkinObject tEquippedSkin = skins.skins[0];

        // Look for equipped skin.
        foreach(SkinObject tSkin in skins.skins)
        {
            if (tSkin.skin_id == GameplayStatics.currentPlayer.equiped_skin)
            {
                tEquippedSkin = tSkin;
                break;
            }
        }

        // Spawn skin
        Instantiate(tEquippedSkin.skin_obj, skinContainer);
    }

    private void CheckIsFalling()
    {
        if (transform.position.y <= -0.5f)
        {
            if (!isResetLevelRequested)
            {
                isResetLevelRequested = true;
                GameManager.Instance.ResetLevel();
            }
        }
    }

    private void CheckHasObstacleAhead()
    {
        hasObstacleAhead = false;
        canGoLeft = true;
        canGoRight = true;

        int tLayerMask = 1 << 8;
        // Check if there is any obstacle ahead including if there is no ground ahead.

        if (Physics.Raycast(centerRaycast.position, centerRaycast.TransformDirection(Vector3.forward), 1f, tLayerMask) ||
            Physics.Raycast(frontDownRaycast.position, frontDownRaycast.TransformDirection(Vector3.down), 1f, tLayerMask) ||
            !Physics.Raycast(frontDownRaycast.position, frontDownRaycast.TransformDirection(Vector3.down), 1f))
        {
            hasObstacleAhead = true;
        }

        // Check if there is any obstacle on the left including if there is no ground on the left.
        if (Physics.Raycast(centerRaycast.position, centerRaycast.TransformDirection(Vector3.left), 1f, tLayerMask) ||
            Physics.Raycast(leftDownRaycast.position, leftDownRaycast.TransformDirection(Vector3.down), 1f, tLayerMask) ||
            !Physics.Raycast(leftDownRaycast.position, leftDownRaycast.TransformDirection(Vector3.down), 1f))
        {
            canGoLeft = false;
        }

        // Check if there is any obstacle on the right including if there is no ground on the right.
        if (Physics.Raycast(centerRaycast.position, centerRaycast.TransformDirection(Vector3.right), 1f, tLayerMask) ||
            Physics.Raycast(rightDownRaycast.position, rightDownRaycast.TransformDirection(Vector3.down), 1f, tLayerMask) ||
            !Physics.Raycast(rightDownRaycast.position, rightDownRaycast.TransformDirection(Vector3.down), 1f))
        {
            canGoRight = false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(centerRaycast.position, centerRaycast.position + frontDownRaycast.TransformDirection(Vector3.forward));
        Gizmos.DrawLine(centerRaycast.position, centerRaycast.position + frontDownRaycast.TransformDirection(Vector3.left));
        Gizmos.DrawLine(centerRaycast.position, centerRaycast.position + frontDownRaycast.TransformDirection(Vector3.right));

        Gizmos.DrawLine(frontDownRaycast.position, frontDownRaycast.position + frontDownRaycast.TransformDirection(Vector3.down));

        Gizmos.DrawLine(leftDownRaycast.position, leftDownRaycast.position + leftDownRaycast.TransformDirection(Vector3.down));
        
        Gizmos.DrawLine(rightDownRaycast.position, rightDownRaycast.position + rightDownRaycast.TransformDirection(Vector3.down));
    }

    public override void ResetPawn()
    {
        isResetLevelRequested = false;
        base.ResetPawn();
    }
}