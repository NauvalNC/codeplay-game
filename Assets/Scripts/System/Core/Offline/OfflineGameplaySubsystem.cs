using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class OfflineGameplaySubsystem : MonoBehaviour
{
    private static readonly string OFFLINE_FILE_PATH = "Resources";
    private static readonly string OFFLINE_FILE_SUBPATH = "Saved";

    private static readonly string OFFLINE_CHAPTER_JSON = "offline_chapters.json";
    private static readonly string OFFLINE_SKIN_JSON = "offline_skins.json";

    public static readonly string TUTORIAL_CHAPTER_CODENAME = "TUT";
    public static readonly string TUTORIAL_HOWTO_STAGE_CODENAME = "TUT-0";

    private static OfflineGameplaySubsystem instance;
    public static OfflineGameplaySubsystem Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<OfflineGameplaySubsystem>();
            }
            return instance;
        }

    }

    public static PublicPlayerInfo offlinePlayer = new PublicPlayerInfo()
    {
        player_id = "offline_player",
        username = "Akun Offline",
        email = "offline_player@email.com",
        equiped_skin = "SK-0"
    };

    public OfflineChapterList offlineChapterList;

    public OfflineChapter tutorialChapter
    {
        get
        {
            OfflineChapter tTutorialChapter = null;

            if (offlineChapterList)
            {
                foreach(OfflineChapter tChapter in offlineChapterList.chapters)
                {
                    if (tChapter.chapterInfo.chapter_id == TUTORIAL_CHAPTER_CODENAME)
                    {
                        tTutorialChapter = tChapter;
                        break;
                    }
                }
            }

            return tTutorialChapter;
        }
    }

    public OfflineSkinList offlineSkinList;

    private void Awake()
    {
        DeserializeOfflineData();
    }

    public void SerializeOfflineData()
    {
        // Serialize offline chapters.
        ListChapterStagesPair tListChapterStagesPair = new ListChapterStagesPair();
        foreach(OfflineChapter tChapter in offlineChapterList.chapters)
        {
            // Skip tutorial chapter.
            if (tChapter.chapterInfo.chapter_id == TUTORIAL_CHAPTER_CODENAME)
            {
                continue;
            }

            ChapterStagesPair tPair = new ChapterStagesPair();
            tPair.chapter = tChapter.chapterInfo;
            tPair.stages = tChapter.stages;

            tListChapterStagesPair.chapterStagesPairs.Add(tPair);
        }
        SaveOfflineData(tListChapterStagesPair, OFFLINE_CHAPTER_JSON);

        // Serialize offline skins.
        EquippedSkinPair tEquippedSkinPair = new EquippedSkinPair();
        tEquippedSkinPair.equippedSkin = offlineSkinList.equippedSkin;
        tEquippedSkinPair.skins = offlineSkinList.skins;
        SaveOfflineData(tEquippedSkinPair, OFFLINE_SKIN_JSON);
    }

    public void DeserializeOfflineData()
    {
        // Deserialize offline chapters.
        ListChapterStagesPair tListChapterStagesPair = LoadOfflineData<ListChapterStagesPair>(OFFLINE_CHAPTER_JSON);
        if (tListChapterStagesPair != null)
        {
            // Load chapters from saved data.
            int tChapterIndex = 1;
            foreach(ChapterStagesPair tPair in tListChapterStagesPair.chapterStagesPairs)
            {
                offlineChapterList.chapters[tChapterIndex].chapterInfo = tPair.chapter;
                offlineChapterList.chapters[tChapterIndex].stages = tPair.stages;
                tChapterIndex++;
            }

            Debug.Log("Load offline chapters from disk success.");
        }

        // Deserialize offline chapters.
        EquippedSkinPair tEquippedSkinPair = LoadOfflineData<EquippedSkinPair>(OFFLINE_SKIN_JSON);
        if (tEquippedSkinPair != null)
        {
            offlineSkinList.equippedSkin = tEquippedSkinPair.equippedSkin;
            offlineSkinList.skins = tEquippedSkinPair.skins;

            GameplayStatics.currentPlayer.equiped_skin = offlineSkinList.equippedSkin;

            Debug.Log("Load offline skins from disk success.");
        }
    }

    private void SaveOfflineData<T>(T obj, string fileName)
    {
        string tJson = JsonUtility.ToJson(obj, true);
        string tFilePath = Path.Combine(Application.persistentDataPath, fileName);

        File.WriteAllText(tFilePath, tJson);
    }

    private T LoadOfflineData<T>(string fileName)
    {
        string tFilePath = Path.Combine(Application.persistentDataPath, fileName);

        if (File.Exists(tFilePath))
        {
            string json = File.ReadAllText(tFilePath);
            return JsonUtility.FromJson<T>(json);
        }

        return default(T);
    }
}