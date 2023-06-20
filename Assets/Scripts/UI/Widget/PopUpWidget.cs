using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopUpWidget : BaseWidget
{
    public delegate void OnPopUpResult(PopUpResult result);

    public enum PopUpType
    {
        OK,
        CONFIRM_CANCEL,
        YES_NO
    }

    public enum PopUpResult
    {
        CONFIRMED,
        DECLINED
    }

    [Header("User Interface")]
    [SerializeField] private TMP_Text txtTitle;
    [SerializeField] private TMP_Text txtSubtitle;
    [SerializeField] private GameObject imgContainer;
    [SerializeField] private Image imgPreview;
    [SerializeField] private Button btnConfirm;
    [SerializeField] private TMP_Text txtConfirm;
    [SerializeField] private Button btnDecline;
    [SerializeField] private TMP_Text txtDecline;

    protected override void SetupWidget()
    {
        base.SetupWidget();

        imgContainer.gameObject.SetActive(false);

        btnConfirm.onClick.RemoveAllListeners();
        btnConfirm.onClick.RemoveAllListeners();

        btnConfirm.onClick.AddListener(Deactivate);
        btnDecline.onClick.AddListener(Deactivate);
    }

    public void SetPopUp(string title, string subtitle, PopUpType type, OnPopUpResult onResult = null)
    {
        SetupWidget();

        if (onResult != null)
        {
            btnConfirm.onClick.AddListener(() =>
            {
                onResult.Invoke(PopUpResult.CONFIRMED);
            });

            btnDecline.onClick.AddListener(() =>
            {
                onResult.Invoke(PopUpResult.DECLINED);
            });
        }

        txtTitle.text = title;
        txtSubtitle.text = subtitle;

        btnConfirm.gameObject.SetActive(true);
        btnDecline.gameObject.SetActive(true);

        switch (type)
        {
            case PopUpType.OK:
                txtConfirm.text = "Ok";
                btnDecline.gameObject.SetActive(false);
                break;
            case PopUpType.CONFIRM_CANCEL:
                txtConfirm.text = "Confirm";
                txtDecline.text = "Cancel";
                break;
            case PopUpType.YES_NO:
                txtConfirm.text = "Yes";
                txtDecline.text = "No";
                break;
        }
    }

    public void SetImage(string path)
    {
        Texture2D tTexture = Resources.Load<Texture2D>(path);
        if (tTexture)
        {
            Sprite tImage = Sprite.Create(tTexture, new Rect(0, 0, tTexture.width, tTexture.height), Vector2.zero);
            imgPreview.color = Color.white;
            imgPreview.sprite = tImage;

            imgContainer.gameObject.SetActive(true);
        }
    }
}
