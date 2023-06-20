using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using Amazon.Extensions.CognitoAuthentication;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;

public class AWSAuthSubsystem : MonoBehaviour
{
    [System.Serializable]
    public class AWSSessionCache
    {
        public string idToken;
        public string accessToken;
        public string refreshToken;
        public string userId;
    }

    private static AWSAuthSubsystem instance;
    public static AWSAuthSubsystem Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<AWSAuthSubsystem>();
            }
            return instance;
        }
    }

    private static string CACHED_PATH;
    private static readonly string CACHED_AUTH_DATA = "aws_cached_auth_data.json";

    private static readonly string IDENTITY_POOL = "<AWS-COGNITO-IDENTITY-POOL>";
    private static readonly string APP_CLIENT_ID = "<AWS-COGNITO-CLIENT-ID>";
    private static readonly string USER_POOL_ID = "<AWS-COGNITO-USER-POOL-ID>";

    public delegate void OnAWSRegisterComplete(bool isSuccessful, string message);
    public delegate void OnAWSLoginComplete(bool isSuccessful, string message, CognitoUser user);
    public delegate void OnAWSLogoutComplete(bool isSuccessful, string message);
    public delegate void OnAWSRequestForgotPasswordVerificationCodeComplete(bool isSuccessful, string message);
    public delegate void OnAWSRefreshSessionComplete(bool isSuccessful, string message);
    public delegate void OnAWSChangePasswordComplete(bool isSuccessful, string message);

    private AmazonCognitoIdentityProviderClient provider;
    private Amazon.RegionEndpoint region = Amazon.RegionEndpoint.USEast1;

    private CognitoAWSCredentials cognitoCreds;
    private CognitoUser cognitoUser;
    private string cognitoUserId = string.Empty;

    private void Awake()
    {
        // Set cached path to save any cached data.
        CACHED_PATH = Application.persistentDataPath;
        Debug.Log("AWS Auth Cached Path: " + CACHED_PATH);

        // Initialize AWS Cognito provider.
        provider = new AmazonCognitoIdentityProviderClient(new Amazon.Runtime.AnonymousAWSCredentials(), region);
        if (provider != null)
        {
            Debug.Log("AWS Authentication is initialized.");
        } 
        else
        {
            Debug.LogError("Failed to initialize AWS Authentication.");
        }
    }

    public void LoginWithEmail(string email, string password, OnAWSLoginComplete onComplete)
    {
        if (!IsValidEmail(email))
        {
            onComplete(false, "Gagal untuk login. Format email yang diberikan tidak valid.", null);
            return;
        }

        CognitoUserPool tUserPool = new CognitoUserPool(USER_POOL_ID, APP_CLIENT_ID, provider);
        CognitoUser tUser = new CognitoUser(email, APP_CLIENT_ID, tUserPool, provider);

        InitiateSrpAuthRequest tAuthRequest = new InitiateSrpAuthRequest()
        {
            Password = password
        };

        // Login the user.
        tUser.StartWithSrpAuthAsync(tAuthRequest).ContinueWith(tAuthFlowResponse =>
        {
            string tErrorMessage = "Gagal untuk login. Email atau password yang diberikan salah. Silakan coba lagi.";
            string tSuccessMessage = "Login sukses.";

            if (tAuthFlowResponse.IsCanceled || tAuthFlowResponse.IsFaulted)
            {
                Debug.LogError("StartWithSrpAuthAsync was canceled or faulted.");
                onComplete.Invoke(false, tErrorMessage, null);
                return;
            }

            // After authenticated, get the user id.
            AuthenticationResultType tAuthResultType = tAuthFlowResponse.Result.AuthenticationResult;
            GetUserIdFromProvider(tAuthFlowResponse.Result.AuthenticationResult.AccessToken).ContinueWith(tUserIdResponse =>
            {
                if (tUserIdResponse.IsCanceled || tUserIdResponse.IsFaulted)
                {
                    Debug.LogError("GetUserIdFromProvider was canceled or faulted.");
                    onComplete.Invoke(false, tErrorMessage, null);
                    return;
                }

                cognitoUserId = tUserIdResponse.Result;
                cognitoCreds = tUser.GetCognitoAWSCredentials(IDENTITY_POOL, region);
                cognitoUser = tUser;

                // Cache session.
                AWSSessionCache tTempSession = new AWSSessionCache()
                {
                    idToken = tAuthResultType.IdToken,
                    accessToken = tAuthResultType.AccessToken,
                    refreshToken = tAuthResultType.RefreshToken,
                    userId = cognitoUserId
                };
                SaveAuthData(tTempSession, CACHED_AUTH_DATA);

                Debug.LogFormat($"AWS user logged in successfully: {tUser.UserID}");

                GameplayStatics.isLoggedIn = true;
                onComplete.Invoke(true, tSuccessMessage, tUser);
            });
        });
    }

    public void RegisterWithEmail(string email, string password, OnAWSRegisterComplete onComplete)
    {
        if (!IsValidEmail(email))
        {
            onComplete(false, "Gagal mendaftarkan akun baru. Format email yang diberikan tidak valid.");
            return;
        }

        if (!IsValidPassword(password))
        {
            onComplete(false, "Gagal mendaftarkan akun baru. Password harus lebih dari 5 karakter.");
            return;
        }

        List<AttributeType> tAttributes = new List<AttributeType>()
        {
            new AttributeType() { Name = "email", Value = email }
        };

        SignUpRequest tSignUpRequest = new SignUpRequest()
        {
            ClientId = APP_CLIENT_ID,
            Username = email,
            Password = password,
            UserAttributes = tAttributes
        };

        provider.SignUpAsync(tSignUpRequest).ContinueWith(tSignUpResponse =>
        {
            string tErrorMessage = "Gagal mendaftarkan akun baru. Akun sudah terdaftar.";
            string tSuccessMessage = "Silahkan cek e-mail untuk menverifikasi akun. Kemudian, login kembali.";

            if (tSignUpResponse.IsCanceled || tSignUpResponse.IsFaulted)
            {
                Debug.LogError("SignUpAsync was canceled or faulted.");
                onComplete.Invoke(false, tErrorMessage);
                return;
            }

            Debug.LogFormat($"AWS user created successfully.");

            onComplete.Invoke(true, tSuccessMessage);
        });
    }

    public async void Logout(OnAWSLogoutComplete onComplete = null)
    {
        if (cognitoUser == null)
        {
            onComplete?.Invoke(false, "Tidak bisa logout. Akun belum login.");
            return;
        }

        // Logout from AWS Cognito
        await cognitoUser.GlobalSignOutAsync();

        // Clear cognito user.
        cognitoUser = null;
        cognitoUserId = string.Empty;

        // Clear cached session.
        AWSSessionCache tSession = new AWSSessionCache()
        {
            idToken = string.Empty,
            accessToken = string.Empty,
            refreshToken = string.Empty,
            userId = string.Empty
        };
        SaveAuthData(tSession, CACHED_AUTH_DATA);

        GameplayStatics.isLoggedIn = false;
        onComplete?.Invoke(true, "Logout berhasil.");
    }

    public void RequestForgotPasswordVerificationCode(string email, OnAWSRequestForgotPasswordVerificationCodeComplete onComplete)
    {
        if (!IsValidEmail(email))
        {
            onComplete(false, "Gagal mengirim kode verifikasi. Format email yang diberikan tidak valid.");
            return;
        }

        ForgotPasswordRequest tForgotPasswordRequest = new ForgotPasswordRequest()
        {
            ClientId = APP_CLIENT_ID,
            Username = email
        };

        provider.ForgotPasswordAsync(tForgotPasswordRequest).ContinueWith(tForgotPasswordResponse =>
        {
            string tErrorMessage = "Gagal mengirim kode verifikasi. Email yang diberikan tidak terdaftar atau maksimum permintaan kode sudah terlewati.";
            string tSuccessMessage = "Berhasil mengirim kode verifikasi. Silakan periksa email kamu.";

            if (tForgotPasswordResponse.IsCanceled || tForgotPasswordResponse.IsFaulted)
            {
                Debug.LogError("ForgotPasswordAsync was canceled or faulted.");
                onComplete.Invoke(false, tErrorMessage);
                return;
            }

            Debug.LogFormat($"Success to request password change verification code.");
            onComplete.Invoke(true, tSuccessMessage);
        });
    }

    public void ChangePassword(string email, string verificationCode, string newPassword, OnAWSChangePasswordComplete onComplete)
    {
        if (!IsValidEmail(email))
        {
            onComplete(false, "Gagal mengganti password. Format email yang diberikan tidak valid.");
            return;
        }

        if (!IsValidPassword(newPassword))
        {
            onComplete(false, "Gagal mengganti password. Password harus lebih dari 5 karakter.");
            return;
        }

        ConfirmForgotPasswordRequest tConfirmChangePasswordRequest = new ConfirmForgotPasswordRequest()
        {
            ClientId = APP_CLIENT_ID,
            Username = email,
            ConfirmationCode = verificationCode,
            Password = newPassword
        };

        provider.ConfirmForgotPasswordAsync(tConfirmChangePasswordRequest).ContinueWith(tConfirmChangePasswordResponse =>
        {
            string tErrorMessage = "Gagal mengganti password. Kode verifikasi salah.";
            string tSuccessMessage = "Berhasil mengganti password. Silakan periksa login kembali.";

            if (tConfirmChangePasswordResponse.IsCanceled || tConfirmChangePasswordResponse.IsFaulted)
            {
                Debug.LogError("ForgotPasswordAsync was canceled or faulted.");
                onComplete.Invoke(false, tErrorMessage);
                return;
            }

            Debug.LogFormat($"Success to change password.");
            onComplete.Invoke(true, tSuccessMessage);
        });
    }

    public void RefreshSession(OnAWSRefreshSessionComplete onComplete)
    {
        DateTime tIssued = DateTime.Now;

        AWSSessionCache tCachedSession = LoadAuthData<AWSSessionCache>(CACHED_AUTH_DATA);
        if (tCachedSession == null || 
            string.IsNullOrEmpty(tCachedSession.idToken) || 
            string.IsNullOrEmpty(tCachedSession.accessToken) || 
            string.IsNullOrEmpty(tCachedSession.refreshToken) || 
            string.IsNullOrEmpty(tCachedSession.userId))
        {
            Debug.Log("Cannot refresh session. No session available.");
            onComplete.Invoke(false, "Tidak bisa menyegarkan sesi. Sesi tidak ada.");
            return;
        }

        CognitoUserPool tUserPool = new CognitoUserPool(USER_POOL_ID, APP_CLIENT_ID, provider);

        CognitoUser tUser = new CognitoUser("", APP_CLIENT_ID, tUserPool, provider);
        tUser.SessionTokens = new CognitoUserSession(
           tCachedSession.idToken,
           tCachedSession.accessToken,
           tCachedSession.refreshToken,
           tIssued,
           DateTime.Now.AddDays(1000)
        );

        // Refresh token.
        tUser.StartWithRefreshTokenAuthAsync(new InitiateRefreshTokenAuthRequest { AuthFlowType = AuthFlowType.REFRESH_TOKEN_AUTH }).ContinueWith(tAuthFlowResponse =>
        {
            if (tAuthFlowResponse.IsCanceled || tAuthFlowResponse.IsFaulted)
            {
                Debug.LogError("StartWithRefreshTokenAuthAsync was canceled or faulted.");
                onComplete.Invoke(false, "Tidak bisa menyegarkan sesi. Terjadi kesalahan.");
                return;
            }

            // Update credentials.
            cognitoCreds = tUser.GetCognitoAWSCredentials(IDENTITY_POOL, region);
            cognitoUser = tUser;
            cognitoUserId = tCachedSession.userId;

            // Cache session.
            AuthenticationResultType tAuthResultType = tAuthFlowResponse.Result.AuthenticationResult;
            AWSSessionCache tTempSession = new AWSSessionCache()
            {
                idToken = tAuthResultType.IdToken,
                accessToken = tAuthResultType.AccessToken,
                refreshToken = tAuthResultType.RefreshToken,
                userId = tCachedSession.userId
            };
            SaveAuthData(tTempSession, CACHED_AUTH_DATA);

            GameplayStatics.isLoggedIn = true;
            onComplete.Invoke(true, "Sesi berhasil disegarkan.");
        });
    }

    private static bool IsValidEmail(string email)
    {
        string tRegex = @"^[^@\s]+@[^@\s]+\.(com|net|org|gov)$";
        return Regex.IsMatch(email, tRegex, RegexOptions.IgnoreCase);
    }

    private static bool IsValidPassword(string password)
    {
        return password.Length >= 6;
    }

    private async Task<string> GetUserIdFromProvider(string accessToken)
    {
        string tUserId = "";
        Task<GetUserResponse> tTask = provider.GetUserAsync(new GetUserRequest { AccessToken = accessToken });
        GetUserResponse tResponse = await tTask;

        // Get the user id.
        foreach (AttributeType attribute in tResponse.UserAttributes)
        {
            if (attribute.Name == "sub")
            {
                tUserId = attribute.Value;
                break;
            }
        }

        return tUserId;
    }

    public string GetCurrentUserId()
    {
        if (string.IsNullOrEmpty(cognitoUserId))
        {
            AWSSessionCache tCachedSession = LoadAuthData<AWSSessionCache>(CACHED_AUTH_DATA);
            if (tCachedSession != null)
            {
                cognitoUserId = tCachedSession.userId;
                return cognitoUserId;
            }

            return null;
        }

        return cognitoUserId;
    }

    public string GetCurrentAccessToken()
    {
        AWSSessionCache tCachedSession = LoadAuthData<AWSSessionCache>(CACHED_AUTH_DATA);
        return (tCachedSession != null) ? tCachedSession.accessToken : string.Empty;
    }

    public bool HasCachedSession()
    {
        string tFilePath = Path.Combine(Application.persistentDataPath, CACHED_AUTH_DATA);
        return File.Exists(tFilePath);
    }

    private void SaveAuthData<T>(T obj, string fileName)
    {
        string tJson = JsonUtility.ToJson(obj, true);
        string tFilePath = Path.Combine(CACHED_PATH, fileName);

        File.WriteAllText(tFilePath, tJson);
    }

    private T LoadAuthData<T>(string fileName)
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