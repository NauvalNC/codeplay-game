using System.Collections.Generic;
using UnityEngine;

#region Testing
public class LandingPageResponse
{
    public bool error { get; set; }
    public string message { get; set; }
}
#endregion

#region Player Info Data Model
public class PublicPlayerInfo
{
    public string player_id { get; set; }
    public string username { get; set; }
    public string email { get; set; }
    public string equiped_skin { get; set; }
}

public class GetPlayerInfoResponse
{
    public bool error { get; set; }
    public string message { get; set; }
    public PublicPlayerInfo player { get; set; }
}

public class SetPlayerInfoResponse
{
    public bool error { get; set; }
    public string message { get; set; }
}
#endregion


#region Chapter Data Model
[System.Serializable]
public class Chapter
{
    public string chapter_id;
    public string title;
    public string banner;
    public int is_solved;
    public int total_stages;
    public int total_stars;
}

public class GetChaptersResponse
{
    public bool error { get; set; }
    public string message { get; set; }
    public List<Chapter> chapters { get; set; }
}
#endregion


#region Stage Data Model
[System.Serializable]
public class Stage
{
    public string stage_id;
    public string chapter_id;
    public int is_solved;
    public int total_codes;
    public int stars;
    public int fastest_time;
    public string title;
    public string banner;
    public TutorialContent tutorialContent = null;
}

public class GetStageResponse
{
    public bool error { get; set; }
    public string message { get; set; }
    public Stage stage { get; set; }
}

public class GetStagesResponse
{
    public bool error { get; set; }
    public string message { get; set; }
    public List<Stage> stages { get; set; }
}

public class SetStageStatisticResponse
{
    public bool error { get; set; }
    public string message { get; set; }
}
#endregion


#region Stage Leaderboard Data Model
public class PlayerStageRank
{
    public int rank { get; set; }
    public string player_id { get; set; }
    public string username { get; set; }
    public int total_codes { get; set; }
    public int fastest_time { get; set; }
    public string stage_id { get; set; }
}

public class GetStageLeaderboardResponse
{
    public bool error { get; set; }
    public string message { get; set; }
    public List<PlayerStageRank> leaderboard { get; set; }
}
#endregion


#region Skin Data Model
[System.Serializable]
public class Skin
{
    public string skin_id;
    public string stage_id;
    public string skin_name;
    public string skin_image;
    public bool is_owned;
}

[System.Serializable]
public class SkinObject
{
    public string skin_id;
    public GameObject skin_obj;
}

public class GetSkinsResponse
{
    public bool error { get; set; }
    public string message { get; set; }
    public List<Skin> skins { get; set; }
}

public class UnlockSkinResponse
{
    public bool error { get; set; }
    public string message { get; set; }
    public Skin unlocked_skin { get; set; }
}

public class EquipSkinResponse
{
    public bool error { get; set; }
    public string message { get; set; }
}
#endregion

#region Helpers
[System.Serializable]
public class ListChapterStagesPair
{
    public List<ChapterStagesPair> chapterStagesPairs = new List<ChapterStagesPair>();
}

[System.Serializable]
public class ChapterStagesPair
{
    public Chapter chapter;
    public List<Stage> stages;
}

[System.Serializable]
public class EquippedSkinPair
{
    public string equippedSkin;
    public List<Skin> skins = new List<Skin>();
}
#endregion