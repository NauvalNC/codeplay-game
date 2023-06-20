using UnityEngine;

public class GameplayRenderer : MonoBehaviour
{
    private void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Camera.main.aspect = (rectTransform.rect.width) / (rectTransform.rect.height);
    }
}
