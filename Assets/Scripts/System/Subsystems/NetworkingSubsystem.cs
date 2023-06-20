using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkingSubsystem : MonoBehaviour
{
    private static NetworkingSubsystem instance;
    public static NetworkingSubsystem Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<NetworkingSubsystem>();
            }
            return instance;
        }
    }

    private static readonly bool USE_LOCALHOST = false;
    private static readonly string BASE_URL = "<AWS-API-BASE-URL>";
    private static readonly string LOCALHOST_URL = "http://localhost:8080";


    #region Generic Delegate
    private struct RequestCallback
    {
        public bool isSuccessful;
        public DownloadHandler result;
    }
    private delegate void OnSendingRequestComplete(RequestCallback callback);

    public delegate void OnGetImageFromURLComplete(bool isSuccessful, Sprite image);
    #endregion

    #region Player Info Delegates
    public delegate void OnSignPlayerComplete(bool isSuccessful, string message);
    public delegate void OnGetPlayerInfoComplete(bool isSuccessful, string message, GetPlayerInfoResponse result);
    public delegate void OnSetPlayerInfoComplete(bool isSuccessful, string message, SetPlayerInfoResponse result);
    #endregion

    #region Chapter Delegates
    public delegate void OnGetChaptersComplete(bool isSuccessful, string message, GetChaptersResponse result);
    #endregion

    #region Stage Delegates
    public delegate void OnGetStageComplete(bool isSuccessful, string message, GetStageResponse result);
    public delegate void OnGetStagesComplete(bool isSuccessful, string message, GetStagesResponse result);
    public delegate void OnSetStageStatisticComplete(bool isSuccessful, string message, SetStageStatisticResponse result);
    #endregion

    #region Stage Leaderboard Delegates
    public delegate void OnGetStageLeaderboardComplete(bool isSuccessful, string message, GetStageLeaderboardResponse result);
    #endregion

    #region Skins Delegates
    public delegate void OnGetSkinsResponseComplete(bool isSuccessful, string message, GetSkinsResponse result);
    public delegate void OnUnlockSkinComplete(bool isSuccessful, string message, UnlockSkinResponse result);
    public delegate void OnEquipSkinComplete(bool isSuccessful, string message);
    #endregion


    #region Generic Endpoints
    private static string GetFullApiUrl(string endPoint)
    {
        return (USE_LOCALHOST ? LOCALHOST_URL : BASE_URL) + endPoint;
    }

    private static IEnumerator SendGetRequest(string endPoint, OnSendingRequestComplete onComplete)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(GetFullApiUrl(endPoint)))
        {
            // Set authorization token from AWS Cognito.
            request.SetRequestHeader("Authorization", AWSAuthSubsystem.Instance.GetCurrentAccessToken());

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(request.error);
                onComplete.Invoke(new RequestCallback() { isSuccessful = false, result = null });
            }
            else
            {
                onComplete.Invoke(new RequestCallback() { isSuccessful = true, result = request.downloadHandler });
            }
        }
    }

    private static IEnumerator SendPostRequest(string endPoint, OnSendingRequestComplete onComplete)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(GetFullApiUrl(endPoint), string.Empty))
        {
            // Set authorization token from AWS Cognito.
            request.SetRequestHeader("Authorization", AWSAuthSubsystem.Instance.GetCurrentAccessToken());

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning(request.error);
                onComplete.Invoke(new RequestCallback() { isSuccessful = false, result = null });
            }
            else
            {
                onComplete.Invoke(new RequestCallback() { isSuccessful = true, result = request.downloadHandler });
            }
        }
    }

    public void GetImageFromUrl(string url, OnGetImageFromURLComplete onComplete)
    {
        StartCoroutine(GetImageFromUrl_Internal(url, onComplete));
    }

    private static IEnumerator GetImageFromUrl_Internal(string url, OnGetImageFromURLComplete onComplete)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogWarning($"Failed to get image from URL: {url}. Error: {request.error}");
                onComplete.Invoke(false, null);
            }
            else
            {
                Texture2D tTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
                Sprite tImage = Sprite.Create(tTexture, new Rect(0, 0, tTexture.width, tTexture.height), Vector2.zero);
                onComplete.Invoke(true, tImage);
            }
        }
    }

    public void CheckAPIConnection()
    {
        string tEndpoint = $"/";

        StartCoroutine(SendGetRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                LandingPageResponse tResponse = JsonConvert.DeserializeObject<LandingPageResponse>(callback.result.text);
                Debug.Log($"Success to check API connection. Hello from API: {tResponse.message}");
            }
            else
            {
                Debug.Log("Failed to check API connection.");
            }
        }));
    }
    #endregion

    #region Player Info Endpoints
    public void SignPlayer(string playerId, string email, OnSignPlayerComplete onComplete)
    {
        string tEndpoint = $"/sign?player_id={playerId}&email={email}";

        StartCoroutine(SendPostRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                GameplayStatics.SetGameplayOnlineMode(true);
                onComplete.Invoke(true, "Success to sign player.");
            }
            else
            {
                onComplete.Invoke(false, "Failed to sign player. Network error.");
            }
        }));
    }

    public void GetPlayerInfo(string playerId, OnGetPlayerInfoComplete onComplete)
    {
        // If offline, then return offline player info.
        if (!GameplayStatics.IsOnline)
        {
            GetPlayerInfoResponse tResult = new GetPlayerInfoResponse()
            {
                error = false,
                message = "Success to get offline player info.",
                player = GameplayStatics.currentPlayer
            };
            onComplete.Invoke(true, tResult.message, tResult);

            return;
        }


        // If online, the get player info from backend.
        string tEndpoint = $"/players/{playerId}";
        StartCoroutine(SendGetRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                GetPlayerInfoResponse tResult = JsonConvert.DeserializeObject<GetPlayerInfoResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Failed to get player info. Network error.", null);
            }
        }));
    }

    public void SetPlayerInfo(string playerId, string username, OnSetPlayerInfoComplete onComplete)
    {
        // Cannot set player info in offline mode.
        if (!GameplayStatics.IsOnline)
        {
            onComplete.Invoke(false, "Tidak dapat mengatur informasi player pada mode offline.", null);
            return;
        }


        // If online, then set player info from backend.
        string tEndpoint = $"/set_player_info?player_id={playerId}&username={username}";
        StartCoroutine(SendPostRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                SetPlayerInfoResponse tResult = JsonConvert.DeserializeObject<SetPlayerInfoResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Gagal mengatur informasi player. Terjadi masalah koneksi internet.", null);
            }
        }));
    }
    #endregion

    #region Chapter Endpoints
    private List<Chapter> GetOfflineChapters()
    {
        OfflineChapterList tOfflineChapterList = OfflineGameplaySubsystem.Instance.offlineChapterList;
        List<Chapter> tOfflineChapters = new List<Chapter>();

        foreach (OfflineChapter tChapter in tOfflineChapterList.chapters)
        {
            // Exclude tutorial chapter from the chapter list.
            if (tChapter.chapterInfo.chapter_id != OfflineGameplaySubsystem.TUTORIAL_CHAPTER_CODENAME)
            {
                Chapter tOfflineChapter = tChapter.chapterInfo;
                tOfflineChapter.total_stages = tChapter.stages.Count;
                tOfflineChapter.total_stars = 0;
                foreach (Stage tStage in tChapter.stages) tOfflineChapter.total_stars += tStage.stars;

                tOfflineChapters.Add(tChapter.chapterInfo);
            }
        }

        return tOfflineChapters;
    }

    public void GetChapters(string playerId, OnGetChaptersComplete onComplete)
    {
        // If offline, then return offline chapters.
        if (!GameplayStatics.IsOnline)
        {
            GetChaptersResponse tOfflineResult = new GetChaptersResponse()
            {
                error = false,
                message = "Sukses mengambil data bab offline.",
                chapters = GetOfflineChapters()
            };

            foreach(Chapter tChapter in tOfflineResult.chapters)
            {
                bool isSolved = true;
                foreach (Stage tStage in GetOfflineStages(tChapter.chapter_id))
                {
                    if (tStage.is_solved == 0)
                    {
                        isSolved = false;
                        break;
                    }
                }

                tChapter.is_solved = isSolved ? 1 : 0;
            }

            onComplete.Invoke(true, tOfflineResult.message, tOfflineResult);

            return;
        }


        // If online, fetch chapters from backend.
        string tEndpoint = $"/chapters?player_id={playerId}";
        StartCoroutine(SendGetRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                GetChaptersResponse tResult = JsonConvert.DeserializeObject<GetChaptersResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Gagal mengambil data bab permainan. Terjadi masalah koneksi internet.", null);
            }
        }));
    }
    #endregion

    #region Stage Endpoints
    private Stage GetOfflineStage(string stageId)
    {
        OfflineChapterList tOfflineChapterList = OfflineGameplaySubsystem.Instance.offlineChapterList;
        foreach (OfflineChapter tChapter in tOfflineChapterList.chapters)
        {
            foreach (Stage tStage in tChapter.stages)
            {
                if (tStage.stage_id == stageId)
                {
                    return tStage;
                }
            }
        }

        return null;
    }

    private List<Stage> GetOfflineStages(string chapterId)
    {
        OfflineChapterList tOfflineChapterList = OfflineGameplaySubsystem.Instance.offlineChapterList;

        foreach (OfflineChapter tChapter in tOfflineChapterList.chapters)
        {
            if (tChapter.chapterInfo.chapter_id == chapterId)
            {
                return tChapter.stages;
            }
        }

        return null;
    }

    public void GetStage(string playerId, string stageId, OnGetStageComplete onComplete)
    {
        // If offline, then return offline stage.
        if (!GameplayStatics.IsOnline)
        {
            Stage tOfflineStage = GetOfflineStage(stageId);

            if (tOfflineStage != null)
            {
                GetStageResponse tOfflineResult = new GetStageResponse()
                {
                    error = false,
                    message = "Sukses mendapatkan data level offline.",
                    stage = tOfflineStage
                };
                onComplete.Invoke(true, tOfflineResult.message, tOfflineResult);
            }
            else
            {
                onComplete.Invoke(true, "Gagal mendapatkan data level offline.", null);
            }

            return;
        }


        // If online, fetch stage from backend.
        string tEndpoint = $"/stage?player_id={playerId}&stage_id={stageId}";
        StartCoroutine(SendGetRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                GetStageResponse tResult = JsonConvert.DeserializeObject<GetStageResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Gagal mendapatkan data level. Terjadi masalah koneksi internet.", null);
            }
        }));
    }

    public void GetStages(string playerId, string chapterId, OnGetStagesComplete onComplete)
    {
        // If offline, then return offline stages.
        if (!GameplayStatics.IsOnline)
        {
            List<Stage> tOfflineStages = GetOfflineStages(chapterId);

            if (tOfflineStages != null)
            {
                GetStagesResponse tOfflineResult = new GetStagesResponse()
                {
                    error = false,
                    message = "Sukses mengambil list data level offline.",
                    stages = tOfflineStages
                };
                onComplete.Invoke(true, tOfflineResult.message, tOfflineResult);
            }
            else
            {
                onComplete.Invoke(true, "Gagal mengambil list data level offline.", null);
            }

            return;
        }


        // If online, fetch stages from backend.
        string tEndpoint = $"/stages?player_id={playerId}&chapter_id={chapterId}";
        StartCoroutine(SendGetRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                GetStagesResponse tResult = JsonConvert.DeserializeObject<GetStagesResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Gagal mengambil list data level. Terjadi masalah koneksi internet.", null);
            }
        }));
    }

    public void SetStageStatistic(string playerId, Stage stage, OnSetStageStatisticComplete onComplete)
    {
        // If offline, then set offline stage statistic.
        if (!GameplayStatics.IsOnline)
        {
            Stage tStage = GetOfflineStage(stage.stage_id);

            if (tStage == null)
            {
                onComplete.Invoke(false, "Gagal mengatur statistik offline.", null);
                return;
            }

            tStage.stars = tStage.stars > stage.stars ? tStage.stars : stage.stars;
            tStage.total_codes = tStage.total_codes < stage.total_codes ? tStage.total_codes : stage.total_codes;
            tStage.fastest_time = tStage.fastest_time < stage.fastest_time ? tStage.fastest_time : stage.fastest_time;
            tStage.is_solved = tStage.is_solved != 0 ? tStage.is_solved : stage.is_solved;

            onComplete.Invoke(true, "Sukses mengatur statistik offline.", null);

            // Serialize offline data to disk.
            OfflineGameplaySubsystem.Instance.SerializeOfflineData();

            return;
        }


        // If online, set stage statistic to backend.
        string tEndpoint = $"/set_stage_stat?player_id={playerId}&stage_id={stage.stage_id}&is_solved={stage.is_solved}" +
            $"&total_codes={stage.total_codes}&stars={stage.stars}&fastest_time={stage.fastest_time}";

        StartCoroutine(SendPostRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                SetStageStatisticResponse tResult = JsonConvert.DeserializeObject<SetStageStatisticResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Gagal mengatur level statistik. Terjadi masalah koneksi internet.", null);
            }
        }));
    }
    #endregion

    #region Stage Leaderboard Endpoints
    public void GetStageLeaderboard(string playerId, string stageId, OnGetStageLeaderboardComplete onComplete)
    {
        // If offline, cannot get leaderboard.
        if (!GameplayStatics.IsOnline)
        {
            GetStageLeaderboardResponse tOfflineResult = new GetStageLeaderboardResponse()
            {
                error = false,
                message = "Sukses mengambil data leaderboard offline.",
                leaderboard = new List<PlayerStageRank>()
            };

            onComplete.Invoke(true, tOfflineResult.message, tOfflineResult);
            return;
        }

        // If online, get leaderboard from backend.
        string tEndpoint = $"/stage_leaderboard?player_id={playerId}&stage_id={stageId}";
        StartCoroutine(SendGetRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                GetStageLeaderboardResponse tResult = JsonConvert.DeserializeObject<GetStageLeaderboardResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Gagal mengambil data leaderboard. Terjadi masalah koneksi internet.", null);
            }
        }));
    }
    #endregion

    #region Skin Endpoints
    private List<Skin> GetOfflineSkins()
    {
        OfflineSkinList tOfflineSkin = OfflineGameplaySubsystem.Instance.offlineSkinList;
        return tOfflineSkin.skins;
    }

    public void GetSkins(string playerId, OnGetSkinsResponseComplete onComplete)
    {
        // If offline, then return offline skins.
        if (!GameplayStatics.IsOnline)
        {
            GetSkinsResponse tResult = new GetSkinsResponse()
            {
                error = false,
                message = "Sukses mendapatkan daftar kostum offline",
                skins = GetOfflineSkins()
            };

            onComplete(true, tResult.message, tResult);

            return;
        }

        // If online, get skins list from backend.
        string tEndpoint = $"/skins?player_id={playerId}";
        StartCoroutine(SendGetRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                GetSkinsResponse tResult = JsonConvert.DeserializeObject<GetSkinsResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Gagal mengambil data kostum. Terjadi masalah koneksi internet.", null);
            }
        }));
    }

    public void UnlockSkin(string playerId, string stageId, OnUnlockSkinComplete onComplete)
    {
        // If offline, then unlock offline skin.
        if (!GameplayStatics.IsOnline)
        {
            Skin tUnlockedSkin = null;
            foreach(Skin tSkin in GetOfflineSkins())
            {
                if (tSkin.stage_id == stageId)
                {
                    if (!tSkin.is_owned)
                    {
                        tSkin.is_owned = true;
                        tUnlockedSkin = tSkin;
                    }
                    break;
                }
            }

            UnlockSkinResponse tResult = new UnlockSkinResponse()
            {
                error = tUnlockedSkin == null,
                message = $"{(tUnlockedSkin != null ? "Sukses" : "Gagal")} membuka kostum offline",
                unlocked_skin = tUnlockedSkin
            };

            onComplete(!tResult.error, tResult.message, tResult);

            // Serialize offline data to disk.
            OfflineGameplaySubsystem.Instance.SerializeOfflineData();

            return;
        }

        // If online, unlock skin to backend.
        string tEndpoint = $"/unlock_skin?player_id={playerId}&stage_id={stageId}";
        StartCoroutine(SendPostRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                UnlockSkinResponse tResult = JsonConvert.DeserializeObject<UnlockSkinResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message, tResult);
            }
            else
            {
                onComplete.Invoke(false, "Tidak ada kostum baru yang dapat diberikan.", null);
            }
        }));
    }

    public void EquipSkin(string playerId, string skinId, OnEquipSkinComplete onComplete)
    {
        // If offline, then equip offline skins.
        if (!GameplayStatics.IsOnline)
        {
            GameplayStatics.currentPlayer.equiped_skin = skinId;
            OfflineGameplaySubsystem.Instance.offlineSkinList.equippedSkin = skinId;

            EquipSkinResponse tResult = new EquipSkinResponse()
            {
                error = false,
                message = "Sukses mengenakan kostum offline"
            };

            onComplete(true, tResult.message);

            // Serialize offline data to disk.
            OfflineGameplaySubsystem.Instance.SerializeOfflineData();

            return;
        }

        // If online, equip skin to backend.
        string tEndpoint = $"/equip_skin?player_id={playerId}&skin_id={skinId}";
        StartCoroutine(SendPostRequest(tEndpoint, (RequestCallback callback) =>
        {
            if (callback.isSuccessful)
            {
                EquipSkinResponse tResult = JsonConvert.DeserializeObject<EquipSkinResponse>(callback.result.text);
                onComplete.Invoke(true, tResult.message);
            }
            else
            {
                onComplete.Invoke(false, "Gagal mengenakan kostum baru. Terjadi masalah koneksi internet.");
            }
        }));
    }
    #endregion
}