using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = System.Object;

public class AuthController : SingletonNew<AuthController>
{
    public Firebase.Auth.FirebaseAuth auth;
    public Firebase.Auth.FirebaseAuth otherAuth;
    public Firebase.Firestore.FirebaseFirestore dbRef;
    public Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
        new Dictionary<string, Firebase.Auth.FirebaseUser>();
    
    public string email = "";
    public string password = "";
    public string displayName = "";
    public string scope1 = "";
    public string scope2 = "";
    public string customParameterKey1 = "";
    public string customParameterValue1 = "";
    public string customParameterKey2 = "";
    public string customParameterValue2 = "";
    
    protected bool signInAndFetchProfile = false;
    private bool fetchingToken = false;
    
    private Firebase.AppOptions otherAuthOptions = new Firebase.AppOptions {
        ApiKey = "",
        AppId = "",
        ProjectId = ""
    };
    
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    
    public virtual void Start() {
        if (FindObjectsOfType<AuthController>().Length > 1)
        {
            Destroy(this);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                InitializeFirebase();
            } else {
                Debug.LogError(
                    "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }
    
    protected void InitializeFirebase() {
        Debug.Log("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        Debug.Log(dbRef);
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        // Specify valid options to construct a secondary authentication object.
        if (otherAuthOptions != null &&
            !(String.IsNullOrEmpty(otherAuthOptions.ApiKey) ||
              String.IsNullOrEmpty(otherAuthOptions.AppId) ||
              String.IsNullOrEmpty(otherAuthOptions.ProjectId))) {
            try {
                otherAuth = Firebase.Auth.FirebaseAuth.GetAuth(Firebase.FirebaseApp.Create(
                    otherAuthOptions, "Secondary"));
                otherAuth.StateChanged += AuthStateChanged;
                otherAuth.IdTokenChanged += IdTokenChanged;
            } catch (Exception) {
                Debug.Log("ERROR: Failed to initialize secondary authentication object.");
            }
        }
        AuthStateChanged(this, null);
        SceneManager.LoadScene(1);
    }
    
    void OnDestroy() {
        if (auth != null) {
            auth.StateChanged -= AuthStateChanged;
            auth.IdTokenChanged -= IdTokenChanged;
            auth = null;
        }
        if (otherAuth != null) {
            otherAuth.StateChanged -= AuthStateChanged;
            otherAuth.IdTokenChanged -= IdTokenChanged;
            otherAuth = null;
        }
    }
    
    protected void DisplayProfile<T>(IDictionary<T, object> profile, int indentLevel) {
        string indent = new String(' ', indentLevel * 2);
        foreach (var kv in profile) {
            var valueDictionary = kv.Value as IDictionary<object, object>;
            if (valueDictionary != null) {
                Debug.Log(String.Format("{0}{1}:", indent, kv.Key));
                DisplayProfile<object>(valueDictionary, indentLevel + 1);
            } else {
                Debug.Log(String.Format("{0}{1}: {2}", indent, kv.Key, kv.Value));
            }
        }
    }
    
    protected void DisplaySignInResult(Firebase.Auth.SignInResult result, int indentLevel) {
        string indent = new String(' ', indentLevel * 2);
        DisplayDetailedUserInfo(result.User, indentLevel);
        var metadata = result.Meta;
        if (metadata != null) {
            Debug.Log(String.Format("{0}Created: {1}", indent, metadata.CreationTimestamp));
            Debug.Log(String.Format("{0}Last Sign-in: {1}", indent, metadata.LastSignInTimestamp));
        }
        var info = result.Info;
        if (info != null) {
            Debug.Log(String.Format("{0}Additional User Info:", indent));
            Debug.Log(String.Format("{0}  User Name: {1}", indent, info.UserName));
            Debug.Log(String.Format("{0}  Provider ID: {1}", indent, info.ProviderId));
            DisplayProfile<string>(info.Profile, indentLevel + 1);
        }
    }
    
    protected void DisplayAuthResult(Firebase.Auth.AuthResult result, int indentLevel) {
        string indent = new String(' ', indentLevel * 2);
        DisplayDetailedUserInfo(result.User, indentLevel);
        var metadata = result.User != null ? result.User.Metadata : null;
        if (metadata != null) {
            Debug.Log(String.Format("{0}Created: {1}", indent, metadata.CreationTimestamp));
            Debug.Log(String.Format("{0}Last Sign-in: {1}", indent, metadata.LastSignInTimestamp));
        }
        var info = result.AdditionalUserInfo;
        if (info != null) {
            Debug.Log(String.Format("{0}Additional User Info:", indent));
            Debug.Log(String.Format("{0}  User Name: {1}", indent, info.UserName));
            Debug.Log(String.Format("{0}  Provider ID: {1}", indent, info.ProviderId));
            DisplayProfile<string>(info.Profile, indentLevel + 1);
        }
        var credential = result.Credential;
        if (credential != null) {
            Debug.Log(String.Format("{0}Credential:", indent));
            Debug.Log(String.Format("{0}  Is Valid?: {1}", indent, credential.IsValid()));
            Debug.Log(String.Format("{0}  Class Type: {1}", indent, credential.GetType()));
            if (credential.IsValid()) {
                Debug.Log(String.Format("{0}  Provider: {1}", indent, credential.Provider));
            }
        }
    }
    
    protected void DisplayUserInfo(Firebase.Auth.IUserInfo userInfo, int indentLevel) {
        string indent = new String(' ', indentLevel * 2);
        var userProperties = new Dictionary<string, string> {
            {"Display Name", userInfo.DisplayName},
            {"Email", userInfo.Email},
            {"Photo URL", userInfo.PhotoUrl != null ? userInfo.PhotoUrl.ToString() : null},
            {"Provider ID", userInfo.ProviderId},
            {"User ID", userInfo.UserId}
        };
        foreach (var property in userProperties) {
            if (!String.IsNullOrEmpty(property.Value)) {
                Debug.Log(String.Format("{0}{1}: {2}", indent, property.Key, property.Value));
            }
        }
    }
    
    protected void DisplayDetailedUserInfo(Firebase.Auth.FirebaseUser user, int indentLevel) {
        string indent = new String(' ', indentLevel * 2);
        DisplayUserInfo(user, indentLevel);
        Debug.Log(String.Format("{0}Anonymous: {1}", indent, user.IsAnonymous));
        Debug.Log(String.Format("{0}Email Verified: {1}", indent, user.IsEmailVerified));
        Debug.Log(String.Format("{0}Phone Number: {1}", indent, user.PhoneNumber));
        var providerDataList = new List<Firebase.Auth.IUserInfo>(user.ProviderData);
        var numberOfProviders = providerDataList.Count;
        if (numberOfProviders > 0) {
            for (int i = 0; i < numberOfProviders; ++i) {
                Debug.Log(String.Format("{0}Provider Data: {1}", indent, i));
                DisplayUserInfo(providerDataList[i], indentLevel + 2);
            }
        }
    }
    
    void AuthStateChanged(object sender, System.EventArgs eventArgs) {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user) {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null) {
                Debug.Log("Signed out " + user.UserId);
            }
            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn) {
                Debug.Log("AuthStateChanged Signed in " + user.UserId);
                displayName = user.DisplayName ?? "";
                DisplayDetailedUserInfo(user, 1);
            }
        }
    }
    
    void IdTokenChanged(object sender, System.EventArgs eventArgs) {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken) {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(
                task => Debug.Log(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
    }
    
    protected bool LogTaskCompletion(Task task, string operation) {
        bool complete = false;
        if (task.IsCanceled) {
            Debug.Log(operation + " canceled.");
        } else if (task.IsFaulted) {
            Debug.Log(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null) {
                    authErrorCode = String.Format("AuthError.{0}: ",
                        ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                Debug.Log(authErrorCode + exception.ToString());
            }
        } else if (task.IsCompleted) {
            Debug.Log(operation + " completed");
            complete = true;
        }
        return complete;
    }

    [Button]
    public void SendPasswordConfirmationMail()
    {
        auth.CurrentUser.SendEmailVerificationAsync().ContinueWithOnMainThread((authTask) =>
        {
            if (LogTaskCompletion(authTask, "Send Email Verification Email"))
            {
                Debug.Log("Email verification mail sent to " + email);
            }
        });
    }
    
    
    [Button]
    public Task CreateUserWithEmailAsync() {
        Debug.Log(String.Format("Attempting to create user {0}...", email));
        // DisableUI();

        // This passes the current displayName through to HandleCreateUserAsync
        // so that it can be passed to UpdateUserProfile().  displayName will be
        // reset by AuthStateChanged() when the new user is created and signed in.
        string newDisplayName = displayName;
        return auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread((task) => {
                // EnableUI();
                if (LogTaskCompletion(task, "User Creation")) {
                    var user = task.Result.User;
                    DisplayDetailedUserInfo(user, 1);
                    return UpdateUserProfileAsync(newDisplayName: newDisplayName);
                }
                return task;
            }).Unwrap();
    }

    [Button]
    public void SetData()
    {
        dbRef = FirebaseFirestore.DefaultInstance;
        Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
        var entryValues = new Dictionary<string, string>();
        entryValues["email"] = auth.CurrentUser.Email;
        entryValues["fullname"] = displayName;
        entryValues["groupid"] = "1";
        entryValues["role"] = "student";
        dbRef.Collection("Users").Document(auth.CurrentUser.UserId).SetAsync(entryValues).ContinueWithOnMainThread(
            task =>
            {
                if (task.IsCanceled) {
                    Debug.LogError("AddDataAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("AddDataAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("Data added successfully.");
            });
    }
    
    [Button]
    public Task UpdateUserProfileAsync(string newDisplayName = null) {
        if (auth.CurrentUser == null) {
            Debug.Log("Not signed in, unable to update user profile");
            return Task.FromResult(0);
        }
        displayName = newDisplayName ?? displayName;
        Debug.Log("Updating user profile");
        // DisableUI();
        return auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile {
            DisplayName = displayName,
            PhotoUrl = auth.CurrentUser.PhotoUrl,
        }).ContinueWithOnMainThread(task => {
            // EnableUI();
            if (LogTaskCompletion(task, "User profile")) {
                DisplayDetailedUserInfo(auth.CurrentUser, 1);
            }
        });
    }

    public Task LoginAttempt(string newMail, string newPassword, Action<Task<Firebase.Auth.AuthResult>> onComplete)
    {
        if (signInAndFetchProfile) {
            return auth.SignInAndRetrieveDataWithCredentialAsync(
                Firebase.Auth.EmailAuthProvider.GetCredential(newMail, newPassword)).ContinueWithOnMainThread(
                onComplete);
        } else {
            return auth.SignInWithEmailAndPasswordAsync(newMail, newPassword)
                .ContinueWithOnMainThread(onComplete);
        }
    }
    
    [Button]
    public Task SigninWithEmailAsync() {
        Debug.Log(String.Format("Attempting to sign in as {0}...", email));
        // DisableUI();
        if (signInAndFetchProfile) {
            return auth.SignInAndRetrieveDataWithCredentialAsync(
                Firebase.Auth.EmailAuthProvider.GetCredential(email, password)).ContinueWithOnMainThread(
                HandleSignInWithAuthResult);
        } else {
            return auth.SignInWithEmailAndPasswordAsync(email, password)
                .ContinueWithOnMainThread(HandleSignInWithAuthResult);
        }
    }
    
    [Button]
    public Task SigninWithEmailCredentialAsync() {
        Debug.Log(String.Format("Attempting to sign in as {0}...", email));
        // DisableUI();
        if (signInAndFetchProfile) {
            return auth.SignInAndRetrieveDataWithCredentialAsync(
                Firebase.Auth.EmailAuthProvider.GetCredential(email, password)).ContinueWithOnMainThread(
                HandleSignInWithAuthResult);
        } else {
            return auth.SignInWithCredentialAsync(
                Firebase.Auth.EmailAuthProvider.GetCredential(email, password)).ContinueWithOnMainThread(
                HandleSignInWithUser);
        }
    }
    
    void HandleSignInWithUser(Task<Firebase.Auth.FirebaseUser> task) {
        // EnableUI();
        if (LogTaskCompletion(task, "Sign-in")) {
            Debug.Log(String.Format("{0} signed in", task.Result.DisplayName));
        }
    }

    // Called when a sign-in without fetching profile data completes.
    void HandleSignInWithAuthResult(Task<Firebase.Auth.AuthResult> task) {
        // EnableUI();
        if (LogTaskCompletion(task, "Sign-in")) {
            if(task.Result.User != null && task.Result.User.IsValid()) {
                DisplayAuthResult(task.Result, 1);
                Debug.Log(String.Format("{0} signed in", task.Result.User.DisplayName));
            } else {
                Debug.Log("Signed in but User is either null or invalid");
            }
        }
    }
    
    void HandleSignInWithSignInResult(Task<Firebase.Auth.SignInResult> task) {
        // EnableUI();
        if (LogTaskCompletion(task, "Sign-in")) {
            DisplaySignInResult(task.Result, 1);
        }
    }
    
    [Button]
    protected Task LinkWithEmailCredentialAsync() {
        if (auth.CurrentUser == null) {
            Debug.Log("Not signed in, unable to link credential to user.");
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(new Exception("Not signed in"));
            return tcs.Task;
        }
        Debug.Log("Attempting to link credential to user...");
        Firebase.Auth.Credential cred =
            Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
        return auth.CurrentUser.LinkWithCredentialAsync(cred).ContinueWithOnMainThread(task => {
            if (LogTaskCompletion(task, "Link Credential")) {
                DisplayDetailedUserInfo(task.Result.User, 1);
            }
        });
    }
    
    [Button]
    protected Task ReauthenticateAsync() {
        var user = auth.CurrentUser;
        if (user == null) {
            Debug.Log("Not signed in, unable to reauthenticate user.");
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(new Exception("Not signed in"));
            return tcs.Task;
        }
        Debug.Log("Reauthenticating...");
        // DisableUI();
        Firebase.Auth.Credential cred = Firebase.Auth.EmailAuthProvider.GetCredential(email, password);
        if (signInAndFetchProfile) {
            return user.ReauthenticateAndRetrieveDataAsync(cred).ContinueWithOnMainThread(task => {
                // EnableUI();
                if (LogTaskCompletion(task, "Reauthentication")) {
                    DisplayAuthResult(task.Result, 1);
                }
            });
        } else {
            return user.ReauthenticateAsync(cred).ContinueWithOnMainThread(task => {
                // EnableUI();
                if (LogTaskCompletion(task, "Reauthentication")) {
                    DisplayDetailedUserInfo(auth.CurrentUser, 1);
                }
            });
        }
    }
    
    [Button]
    public void ReloadUser() {
        if (auth.CurrentUser == null) {
            Debug.Log("Not signed in, unable to reload user.");
            return;
        }
        Debug.Log("Reload User Data");
        auth.CurrentUser.ReloadAsync().ContinueWithOnMainThread(task => {
            if (LogTaskCompletion(task, "Reload")) {
                DisplayDetailedUserInfo(auth.CurrentUser, 1);
            }
        });
    }

    // Fetch and display current user's auth token.
    [Button]
    public void GetUserToken() {
        if (auth.CurrentUser == null) {
            Debug.Log("Not signed in, unable to get token.");
            return;
        }
        Debug.Log("Fetching user token");
        fetchingToken = true;
        auth.CurrentUser.TokenAsync(false).ContinueWithOnMainThread(task => {
            fetchingToken = false;
            if (LogTaskCompletion(task, "User token fetch")) {
                Debug.Log("Token = " + task.Result);
            }
        });
    }

    // Display information about the currently logged in user.
    [Button]
    void GetUserInfo() {
        if (auth.CurrentUser == null) {
            Debug.Log("Not signed in, unable to get info.");
        } else {
            Debug.Log("Current user info:");
            DisplayDetailedUserInfo(auth.CurrentUser, 1);
        }
    }
    
    [Button]
    protected Task UnlinkEmailAsync() {
        if (auth.CurrentUser == null) {
            Debug.Log("Not signed in, unable to unlink");
            var tcs = new TaskCompletionSource<bool>();
            tcs.SetException(new Exception("Not signed in"));
            return tcs.Task;
        }
        Debug.Log("Unlinking email credential");
        // DisableUI();
        return auth.CurrentUser.UnlinkAsync(
                Firebase.Auth.EmailAuthProvider.GetCredential(email, password).Provider)
            .ContinueWithOnMainThread(task => {
                // EnableUI();
                LogTaskCompletion(task, "Unlinking");
            });
    }
    
    [Button]
    protected void SignOut() {
        Debug.Log("Signing out.");
        auth.SignOut();
    }

    // Delete the currently logged in user.
    [Button]
    protected Task DeleteUserAsync() {
        if (auth.CurrentUser != null) {
            Debug.Log(String.Format("Attempting to delete user {0}...", auth.CurrentUser.UserId));
            // DisableUI();
            return auth.CurrentUser.DeleteAsync().ContinueWithOnMainThread(task => {
                // EnableUI();
                LogTaskCompletion(task, "Delete user");
            });
        } else {
            Debug.Log("Sign-in before deleting user.");
            // Return a finished task.
            return Task.FromResult(0);
        }
    }

    // Show the providers for the current email address.
    [Button]
    protected void DisplayProvidersForEmail() {
        auth.FetchProvidersForEmailAsync(email).ContinueWithOnMainThread((authTask) => {
            if (LogTaskCompletion(authTask, "Fetch Providers")) {
                Debug.Log(String.Format("Email Providers for '{0}':", email));
                foreach (string provider in authTask.Result) {
                    Debug.Log(provider);
                }
            }
        });
    }

    // Send a password reset email to the current email address.
    [Button]
    protected void SendPasswordResetEmail() {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread((authTask) => {
            if (LogTaskCompletion(authTask, "Send Password Reset Email")) {
                Debug.Log("Password reset email sent to " + email);
            }
        });
    }
    
    protected Firebase.Auth.FederatedOAuthProvider BuildFederatedOAuthProvider(string providerId) {
        Firebase.Auth.FederatedOAuthProviderData data = new Firebase.Auth.FederatedOAuthProviderData();
        data.ProviderId = providerId;
        List<string> scopes = new List<string>();
        if(scope1 != "" ) {
            scopes.Add(scope1);
        }
        if(scope2 != "" ) {
            scopes.Add(scope2);
        }
        data.Scopes = scopes;

        data.CustomParameters = new Dictionary<string, string>();
        if(customParameterKey1 != "" && customParameterValue1 != "") {
            data.CustomParameters.Add(customParameterKey1, customParameterValue1);
        }
        if(customParameterKey2 != "" && customParameterValue2 != "") {
            data.CustomParameters.Add(customParameterKey2, customParameterValue2);
        }

        return  new Firebase.Auth.FederatedOAuthProvider(data);
    }
    
    protected bool HasOtherAuth { get { return auth != otherAuth && otherAuth != null; } }

    // Swap the authentication object currently being controlled by the application.
    protected void SwapAuthFocus() {
        if (!HasOtherAuth) return;
        var swapAuth = otherAuth;
        otherAuth = auth;
        auth = swapAuth;
        Debug.Log(String.Format("Changed auth from {0} to {1}",
            otherAuth.App.Name, auth.App.Name));
    }
}
