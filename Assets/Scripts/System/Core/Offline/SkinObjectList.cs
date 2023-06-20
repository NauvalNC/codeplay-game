using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skin Object List", menuName = "CodePlay/Offline/New Skin Object List", order = 3)]
public class SkinObjectList : ScriptableObject
{
    [Header("Skin Objects")]
    public List<SkinObject> skins;
}
