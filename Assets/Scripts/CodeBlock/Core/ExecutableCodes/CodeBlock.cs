using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/* Struct that holds information about the code. */
[System.Serializable]
public struct CodeInfo
{
    public string codeName;
    public Sprite codeIcon;
    [TextArea(3, 10)] public string codeDescription;
}

/* Default codeblock class. */
[RequireComponent(typeof(CanvasGroup))]
public class CodeBlock : UIBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    protected struct CachedCodeConfig
    {
        public CodePort port;
        public int siblingIndex;
    }

    [Header("Attributes")]
    public bool isAssignable = true;
    public bool isInteractable = true;
    public bool isTemplate = false;
    public bool deleteOnFailedReassign = true;
    public CodeInfo codeInfo;
    [HideInInspector] public CodePort containerPort;
    protected CachedCodeConfig lastConfig;

    [Header("Code Visuals")]
    [SerializeField] private Image[] codeVisuals;
    [SerializeField] private Color codeColor;
    [SerializeField] private Color hoverColor;

    [Header("Components")]
    public CodeAction codeAction;
    public CodeDongle dongle;
    public CodePort outerPort;
    private RectTransform rectTransform;
    protected Transform originTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;

    [Header("Container Setup")]
    public bool isContainer = false;
    public bool shouldHasOuterPort = true;
    public CodePort[] innerPorts;

    [Header("Slot Setup")]
    public CodePort[] slotPorts;

    [Header("User Interface")]
    [SerializeField] private TMP_Text txtCodeName;
    [SerializeField] private Image imgCodeIcon;

    #region Helper Variables
    private bool isDragging = false;
    public bool IsDragging { get { return isDragging; } }
    #endregion

    #region Delegates
    public delegate void OnEndDragDelegate();
    public OnEndDragDelegate onEndDragDelegate;

    public delegate void OnDragDelegate();
    public OnDragDelegate onDragDelegate;

    public delegate void OnBeginDragDelegate();
    public OnBeginDragDelegate onBeginDragDelegate;
    #endregion

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        SetOriginalCodeVisual();

        if (txtCodeName)
        {
            txtCodeName.text = codeInfo.codeName;
        }

        if (imgCodeIcon && codeInfo.codeIcon)
        {
            imgCodeIcon.sprite = codeInfo.codeIcon;
        }

        if (outerPort)
        {
            outerPort.gameObject.SetActive(shouldHasOuterPort);
        }
    }
#endif

    protected override void Awake()
    {
        OnAwakeSetup();
    }

    public virtual void OnAwakeSetup()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = CodeManager.Instance.rootCanvas;

        rectTransform = GetComponent<RectTransform>();
        originTransform = canvas.transform;

        onBeginDragDelegate += () => { isDragging = true; };
        onEndDragDelegate += () => { isDragging = false; };

        if (!codeAction)
        {
            codeAction = GetComponent<CodeAction>();
        }
        if (codeAction)
        {
            codeAction.SetOwner(this);
        }

        if (dongle)
        {
            dongle.SetOwner(this);
        }

        if (outerPort)
        {
            outerPort.SetOwner(this);
        }

        foreach (CodePort tPort in innerPorts)
        {
            tPort.SetOwner(this);
        }

        foreach (CodePort tSlot in slotPorts)
        {
            tSlot.SetOwner(this);
        }

        if (isTemplate)
        {
            isAssignable = false;
        }
    }

    #region Code Assignment
    /* Assign a codeblock as this codeblock next code. */
    public virtual void AssignNextCode(CodeBlock newCode, CodePort parentPort)
    {
        // Cannot assign code if it currently running.
        if (CodeManager.Instance.IsExecuting)
        {
            Debug.LogWarning("Cannot assign code when the code execution is currently running.");
            newCode.TryReassign();
            return;
        }

        // Cannot assign code to template code.
        if (isTemplate)
        {
            Debug.LogWarning("Cannot assign code to a template code.");
            newCode.TryReassign();
            return;
        }

        // If this code is not assignable, then return.
        if (!isAssignable)
        {
            Debug.LogWarning("Failed to assign code. Target code is not assignable. Retry to reassign.");
            newCode.TryReassign();
            return;
        }

        // Check if it valid to add code, if not then simply delete it.
        if (!newCode.lastConfig.port && !CodeManager.Instance.IsValidToAddCode())
        {
            SoundManager.Instance.PlayErrorSound();

            PromptSubsystem.Instance.ShowPopUp(
                "Terlalu Banyak Kode", 
                "Jumlah kode yang kamu susun sudah melebihi batas yang ditentukan. Coba susun ulang kode kamu agar lebih efektif!", 
                PopUpWidget.PopUpType.OK);

            Debug.LogWarning("Failed to assign code. It is invalid to add code. Code is deleted.");
            newCode.Delete();
            return;
        }

        // Assign code based on port type.
        bool tIsAssignmentSuccess = false;
        if (parentPort.isSlotPort)
        {
            tIsAssignmentSuccess = AssignToSlotPort(newCode, parentPort);
        } else
        {
            tIsAssignmentSuccess = AssignToCodePort(newCode, parentPort);
        }

        // Assignment failed, fallback.
        if (!tIsAssignmentSuccess)
        {
            newCode.TryReassign();
            return;
        }

        // Tell code manager that a new code is added if it was not in the history yet.
        if (!newCode.lastConfig.port)
        {
            CodeManager.Instance.OnCodeAdded();
        }
    }

    protected bool AssignToCodePort(CodeBlock newCode, CodePort parentPort)
    {
        // Try to assign container code to the new code.
        bool tIsAssignToContainer = (isContainer && parentPort.isInnerPort);
        if (tIsAssignToContainer)
        {
            newCode.containerPort = parentPort;
        }
        else
        {
            newCode.containerPort = containerPort;
        }

        // If there is no container found, then new code cannot be assigned.
        if (!tIsAssignToContainer && !containerPort)
        {
            Debug.LogWarning("Failed to assign code. There is no container found, then new code cannot be assigned.");
            return false;
        }

        // Set new code parent to the container.
        int tTargetIndex = 0;
        if (!tIsAssignToContainer)
        {
            tTargetIndex = transform.GetSiblingIndex() + 1;
        }
        newCode.transform.SetParent(newCode.containerPort.containerRect);
        newCode.transform.SetSiblingIndex(tTargetIndex);

        // Refresh container layout.
        newCode.RefreshContainerLayout(newCode.containerPort.containerRect);

        return true;
    }

    protected bool AssignToSlotPort(CodeBlock newCode, CodePort parentPort)
    {
        if (parentPort.containerRect.childCount >= 1)
        {
            Debug.LogWarning("Cannot assign new code to slot. Slot only can be assigned by one code.");
            return false;
        }

        newCode.containerPort = parentPort;
        newCode.transform.SetParent(parentPort.containerRect);
        return true;
    }

    /* Detach code blocks from its siblings. 
     * Detached code block have no relationship to other codeblocks. */
    public virtual void DetachFromSiblings()
    {
        RectTransform tParent = containerPort ? containerPort.containerRect : null;

        // Reparent to origin.
        transform.SetParent(originTransform);
        containerPort = null;

        // Refresh ex-parent container layout.
        if (tParent)
        {
            RefreshContainerLayout(tParent);
        }
    }

    /* Try to reassign to last container if any. */
    public virtual void TryReassign()
    {
        // Try to reassign.
        if (lastConfig.port)
        {
            containerPort = lastConfig.port;
            transform.SetParent(containerPort.containerRect);
            transform.SetSiblingIndex(lastConfig.siblingIndex);

            RefreshContainerLayout(containerPort.containerRect);
        }
        // If failed to reassign, then destroy.
        else if (deleteOnFailedReassign)
        {
            Delete();
        }
    }

    /* Delete code block */
    public virtual void Delete(bool destroyObject = true)
    {
        // If code block was assigned, then delete its history from code manager.
        if (lastConfig.port || containerPort)
        {
            CodeManager.Instance.OnCodeDeleted();
        }

        // If has slot port, then delete the assigned slot.
        foreach(CodePort tSlot in slotPorts)
        {
            Transform tContainer = tSlot.containerRect;
            int tChildCount = tContainer.childCount;
            for (int i = 0; i < tChildCount; i++)
            {
                tContainer.GetChild(i).GetComponent<CodeBlock>().Delete(true);
            }
        }

        // If code block is a code container, then recurse delete.
        if (isContainer)
        {
            foreach (CodePort tPort in innerPorts)
            {
                Transform tContainer = tPort.containerRect;
                int tChildCount = tContainer.childCount;

                for (int i = 0; i < tChildCount; i++)
                {
                    tContainer.GetChild(i).GetComponent<CodeBlock>().Delete(false);
                }
            }
        }

        // Destroy code block game object.
        if (destroyObject)
        {
            Destroy(gameObject);
        }
    }

    /* Change origin transform. 
     * Useful when the codeblock is moved to other canvas or part of other UI components
     * This function doesn't reparent the codeblock directly.
     * It only reparent the codeblock when the codeblock is being detached from a code container if any. */
    public virtual void ChangeOriginTransform(Transform newOrigin)
    {
        originTransform = newOrigin;
    }

    protected virtual void RefreshContainerLayout(RectTransform targetTransform)
    {
        if (!targetTransform)
        {
            return;
        }

        VerticalLayoutGroup tLayout = targetTransform.GetComponent<VerticalLayoutGroup>();
        LayoutElement tLayoutElement = targetTransform.GetComponent<LayoutElement>();
        if (tLayoutElement)
        {
            tLayoutElement.enabled = targetTransform.childCount <= 0;
        }

        if (tLayout)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(targetTransform);
            RefreshContainerLayout(targetTransform.parent.GetComponent<RectTransform>());
        }
    }
    #endregion

    #region Interaction
    public void OnPointerClick(PointerEventData eventData)
    {
        if (isTemplate)
        {
            CodeManager.Instance.DisplayCodeInfo(codeInfo);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        SoundManager.Instance.PlayButtonSound();

        /** If this code is template, mark this as non-template code.
         * Then instantiate new template to replace this code. */
        if (isTemplate)
        {
            int tIndex = transform.GetSiblingIndex();
            Transform tTransform = Instantiate(gameObject, transform.position, Quaternion.identity, transform.parent).transform;
            tTransform.name = codeInfo.codeName;
            tTransform.SetSiblingIndex(tIndex);

            isAssignable = true;
            isTemplate = false;
        }

        if (!isInteractable)
        {
            return;
        }

        // Cached config.
        lastConfig.port = containerPort;
        lastConfig.siblingIndex = transform.GetSiblingIndex();

        /** Bring code to the top-most, so it is visible 
         * and not covered by other code blocks. */
        transform.SetAsLastSibling();

        canvasGroup.alpha = 0.6f;
        onBeginDragDelegate?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isInteractable)
        {
            return;
        }

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        onDragDelegate?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SoundManager.Instance.PlayButtonSound();

        if (!isInteractable)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        onEndDragDelegate?.Invoke();
    }
    #endregion

    #region Events
    /* On port is hovered by other codeblock */
    public virtual void OnPortHovered()
    {
        SetHoverCodeVisual();
    }

    /* On port is not hovered anymore by other codeblock */
    public virtual void OnPortExitHover()
    {
        SetOriginalCodeVisual();
    }
    #endregion

    #region Visuals
    private void SetOriginalCodeVisual()
    {
        foreach (Image tVisual in codeVisuals)
        {
            tVisual.color = codeColor;
        }
    }

    private void SetHoverCodeVisual()
    {
        foreach (Image tVisual in codeVisuals)
        {
            tVisual.color = hoverColor;
        }
    }
    #endregion
}