using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinsWidget : BaseWidget
{
    [Header("User Interface")]
    [SerializeField] private SkinEntry entry;
    [SerializeField] private EntryLayoutElement entryContainer;

    private List<SkinEntry> skinEntries = new List<SkinEntry>();

    protected override void OnWidgetActivated()
    {
        base.OnWidgetActivated();

        skinEntries.Clear();
        entryContainer.ClearEntries();

        if (GameplayStatics.IsOnline) PromptSubsystem.Instance.ShowLoading();

        NetworkingSubsystem.Instance.GetSkins(
            GameplayStatics.currentPlayer.player_id,
            (bool isSuccessful, string message, GetSkinsResponse result) =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (isSuccessful)
                {
                    foreach (Skin tSkin in result.skins)
                    {
                        SkinEntry tNewEntry = Instantiate(entry);
                        tNewEntry.InitData(tSkin, () => 
                        {
                            EquipSkins(tSkin);
                        });

                        tNewEntry.ToggleEquip(tSkin.skin_id == GameplayStatics.currentPlayer.equiped_skin);

                        skinEntries.Add(tNewEntry);
                        entryContainer.AddEntry(tNewEntry.gameObject);
                    }
                }
                else
                {
                    PromptSubsystem.Instance.ShowPopUp("Error", message, PopUpWidget.PopUpType.OK);
                    Debug.LogWarning(message);
                }
            }
        );
    }

    protected override void SetupWidget()
    {
        base.SetupWidget();

        entryContainer.ClearEntries();
    }

    private void EquipSkins(Skin skin)
    {
        foreach(SkinEntry skinEntry in skinEntries)
        {
            skinEntry.ToggleEquip(skinEntry.GetSkinId() == skin.skin_id);
        }

        if (GameplayStatics.IsOnline) PromptSubsystem.Instance.ShowLoading("Mengganti Kostum...");

        NetworkingSubsystem.Instance.EquipSkin(
            GameplayStatics.currentPlayer.player_id,
            skin.skin_id,
            (bool isSuccessful, string message) =>
            {
                PromptSubsystem.Instance.HideLoading();

                if (!isSuccessful)
                {
                    PromptSubsystem.Instance.ShowPopUp("Error", message, PopUpWidget.PopUpType.OK);
                    Debug.LogWarning(message);
                }
            }
        );
    }
}
