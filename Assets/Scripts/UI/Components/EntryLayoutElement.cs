using System.Collections.Generic;
using UnityEngine;

public class EntryLayoutElement : MonoBehaviour
{
    private List<GameObject> entries = new List<GameObject>();

    public List<GameObject> GetEntries()
    {
        return entries;
    }

    public void AddEntry(GameObject entry)
    {
        entry.transform.SetParent(transform);
        entry.transform.localScale = Vector3.one;
        entries.Add(entry);
    }

    public void RemoveEntry(GameObject entry)
    {
        entries.Remove(entry);
        Destroy(entry);
    }

    public void ClearEntries()
    {
        foreach(Transform entry in transform)
        {
            RemoveEntry(entry.gameObject);
        }
    }
}
