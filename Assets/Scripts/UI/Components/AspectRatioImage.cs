using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AspectRatioImage : MonoBehaviour
{
    private Image img;

    private void OnEnable()
    {
        Rect tRect = GetComponent<RectTransform>().rect;
        tRect.height = tRect.width;
    }
}
