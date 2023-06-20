using UnityEngine;
using UnityEngine.UI;

/* Class to destroy code block when it enters this class instance area. */
[RequireComponent(typeof(BoxCollider2D))]
public class CodeTrashBin : MonoBehaviour
{
    [SerializeField] private Image imgTrashBin;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color hoverColor;

    public void ToggleHover(bool isHover)
    {
        imgTrashBin.color = isHover ? hoverColor : defaultColor;
    }
}
