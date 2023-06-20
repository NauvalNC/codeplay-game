using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ForLoopAction : CodeAction
{
    [Header("User Interface")]
    [SerializeField] private TMP_InputField inputLoopCount;
    [SerializeField] private Button btnPrev;
    [SerializeField] private Button btnNext;

    private int loopCount = 1;
    private int maxCount = 99;

    private void Awake()
    {
        loopCount = 1;
        
        if (inputLoopCount)
        {
            inputLoopCount.text = loopCount.ToString();
            inputLoopCount.onValueChanged.AddListener((string newValue) =>
            {
                try
                {
                    loopCount = Math.Clamp(int.Parse(newValue), 1, maxCount);
                } 
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            });
            inputLoopCount.onEndEdit.AddListener((string finalValue) =>
            {
                inputLoopCount.text = loopCount.ToString();
            });
        }
    
        if (btnPrev)
        {
            btnPrev.onClick.AddListener(() => 
            { 
                loopCount--;
                inputLoopCount.text = loopCount.ToString();
            });
        }

        if (btnNext)
        {
            btnNext.onClick.AddListener(() =>
            {
                loopCount++;
                inputLoopCount.text = loopCount.ToString();
            });
        }
    }

    public override IEnumerator Execute()
    {
        yield return base.Execute();

        // Get first inner port code members.
        RectTransform tContainer = GetOwner().innerPorts[0].containerRect;

        // Loop execution based on loop count.
        for (int i = 0; i < loopCount; i++)
        {
            if (i > 0) yield return base.Execute();

            if (CodeManager.Instance.isBreakRequested) break;

            // Execute children.
            int tChildCount = tContainer.childCount;
            for (int j = 0; j < tChildCount; j++)
            {
                if (CodeManager.Instance.isBreakRequested) break;

                CodeAction tCode = tContainer.GetChild(j).GetComponent<CodeAction>();
                yield return tCode.Execute();
            }
        }

        CodeManager.Instance.isBreakRequested = false;
    }
}