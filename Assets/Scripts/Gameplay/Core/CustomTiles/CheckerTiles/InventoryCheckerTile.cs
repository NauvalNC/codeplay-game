using UnityEngine;

public class InventoryCheckerTile : CheckerTile
{
    [Header("Check Inventory")]
    [SerializeField] private string itemID;
    [SerializeField] private bool hideOnSatisfied = false;

    public override void OnPreCheck()
    {
        base.OnPreCheck();

        CheckOnExecuteCode(CodeManager.Instance.GetCurrentCodeAction());

        if (hideOnSatisfied && isPass) 
        {
            gameObject.SetActive(false);
        }
    }

    public override void ResetCheck()
    {
        base.ResetCheck();
        gameObject.SetActive(true);
    }

    public override void CheckOnExecuteCode(CodeAction codeAction)
    {
        base.CheckOnExecuteCode(codeAction);

        Item tItem = InventoryManager.Instance.FindItem(itemID);
        isPass = false;

        // Check permission.
        switch (checkerType)
        {
            case ECheckerType.REQUIRE:
                isPass = tItem != null;
                if (tItem) InventoryManager.Instance.ConsumeItem(itemID);
                break;
            case ECheckerType.EXCEPT:
                isPass = tItem == null;
                break;
        }

        if (!isPass)
        {
            GameManager.Instance.ResetLevel();
            PromptSubsystem.Instance.ShowPopUp(
                $"\"{itemID}\" Diperlukan",
                $"Tidak bisa melewati jalan. Kamu harus  memiliki \"{itemID}\" untuk melewati jalannya.",
                PopUpWidget.PopUpType.OK);
        }
    }
}