using System.Collections;
using UnityEngine;

public class CodeAction : MonoBehaviour
{
    [SerializeField] private CodeBlock codeOwner;

    public void SetOwner(CodeBlock owner) { codeOwner = owner; }

    public CodeBlock GetOwner() { return codeOwner; }

    public virtual IEnumerator Execute() 
    {
        MainThreadWorker.worker.AddJob(() =>
        {
            CodeManager.Instance.onCurrentCodeExecutedDelegate?.Invoke(this);
        });
        
        CodeManager.Instance.PrintLog($"CodeBlock {gameObject.name} executes code action.");
        
        yield return null;
    }
}