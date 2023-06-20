using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text txtSkinName;
    [SerializeField] private Image imgSkin;
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private TMP_Text txtLockMessage;
    [SerializeField] private GameObject equipIcon;

    #region Helpers
    private Skin skinCache;
    private bool isLocked = true;
    public bool IsLocked { get { return isLocked; } }

    private Button btnEntry;
    public delegate void OnSkinEntryClicked();
    #endregion

    private void Awake()
    {
        btnEntry = GetComponent<Button>();
        ToggleLock(true);
    }

    public void InitData(Skin skin, OnSkinEntryClicked onClicked)
    {
        skinCache = skin;

        ToggleLock(!skin.is_owned);

        txtSkinName.text = skin.skin_name;

        Texture2D tTexture = Resources.Load<Texture2D>(skin.skin_image);
        if (tTexture)
        {
            Sprite tImage = Sprite.Create(tTexture, new Rect(0, 0, tTexture.width, tTexture.height), Vector2.zero);
            imgSkin.color = Color.white;
            imgSkin.sprite = tImage;
        }

        // Unlock level requirement
        if (!string.IsNullOrEmpty(skin.stage_id))
        {
            string[] levelRequirement = skin.stage_id.Split('-');
            txtLockMessage.text = $"Selesaikan\nLevel {levelRequirement[2]} dari World {levelRequirement[1]}";
        }

        btnEntry.onClick.RemoveAllListeners();
        btnEntry.onClick.AddListener(() =>
        {
            if (!isLocked) onClicked.Invoke();
        });
    }

    public void ToggleLock(bool isLocked)
    {
        this.isLocked = isLocked;
        lockIcon.gameObject.SetActive(isLocked);

        if (this.isLocked) ToggleEquip(false);
    }

    public void ToggleEquip(bool isEquip)
    {
        equipIcon.SetActive(isEquip);
    }

    public string GetSkinId() {  return skinCache.skin_id; }
}
