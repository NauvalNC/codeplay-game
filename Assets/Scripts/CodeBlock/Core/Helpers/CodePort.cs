using UnityEngine;

/* Class that acts as port that can be attached by other codeblock. */
public class CodePort : MonoBehaviour
{
    [Header("Port Settings")]
    public bool isInnerPort = false;
    [Tooltip("If this is activated, the port will be treated as slot for variable block. Other port settings will be ignored.")]
    public bool isSlotPort = false;

    [Header("Components")]
    [SerializeField] private CodeBlock codeOwner;
    [Tooltip("Container rect acts as wrapper to hold child code blocks assigned to this port.")]
    public RectTransform containerRect;

    private void Awake()
    {
        if (isSlotPort)
        {
            isInnerPort = false;
        }
    }

    public void SetOwner(CodeBlock owner) { codeOwner = owner; }

    public CodeBlock GetOwner() { return codeOwner; }

    public VariableBlock GetSlotData()
    {
        if (!isSlotPort) return null;

        if (containerRect.childCount <= 0) return null;

        return containerRect.GetChild(0).GetComponent<VariableBlock>();
    }
}