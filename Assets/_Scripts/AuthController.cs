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

    public bool isStudent = true;
    
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
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
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

        if (auth.CurrentUser != null)
        {
            auth.CurrentUser.ReloadAsync().ContinueWithOnMainThread(task =>
            {
                AuthStateChanged(this, null);
                SceneManager.LoadScene(1);
            });
        }
        else
        {
            AuthStateChanged(this, null);
            SceneManager.LoadScene(1);
        }
        
        
    }

    public void SendResult(SavedCarProps newProps, PartEffectController.GroundType newGroundType, float duration, float speed, float mass, Action<Task<DocumentReference>> onComplete)
    {
        if (!isStudent)
        {
            return;
        }

        var combinationMap = new Dictionary<string, object>
        {
            {"bodywork", newProps.kaportaId},
            {"engine", newProps.motorId},
            {"road", (int)newGroundType},
            {"seat", newProps.koltukId},
            {"spoiler", newProps.ruzgarlikId},
            {"tire", newProps.lastikId},
        };

        var outcomeMap = new Dictionary<string, object>
        {
            {"consumption", 0},
            {"duration", duration},
            {"friction", 0},
            {"mass", mass},
            {"speed", speed},
        };

        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            {"appversion", Application.version},
            {"combination", combinationMap},
            {"outcome", outcomeMap},
            {"timestamp", Timestamp.GetCurrentTimestamp()},
            {"userid", auth.CurrentUser.UserId},
        };
        dbRef.Collection("Results").AddAsync(dict).ContinueWithOnMainThread(onComplete);
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

    public Task DeleteAccountAttempt()
    {
        UIControl.I.SetWaitBG(true);

        if (auth.CurrentUser != null) {
            
            dbRef = FirebaseFirestore.DefaultInstance;
            return dbRef.Collection("Users").Document(auth.CurrentUser.UserId).GetSnapshotAsync()
                .ContinueWithOnMainThread(
                    task1 =>
                    {
                        if (task1.IsCanceled)
                        {
                            UIControl.I.SetWaitBG(false);
                            return task1;
                        }
                        if (task1.IsFaulted)
                        {
                            UIControl.I.SetWaitBG(false);
                            return task1;
                        }
                        if (task1.IsCompleted)
                        {
                            dbRef.Collection("Groups").Document(task1.Result.GetValue<string>("groupid"))
                                .GetSnapshotAsync().ContinueWithOnMainThread(
                                    task2 =>
                                    {
                                        if (task2.IsCanceled)
                                        {
                                            UIControl.I.SetWaitBG(false);
                                            return task2;
                                        }
                                        if (task2.IsFaulted)
                                        {
                                            UIControl.I.SetWaitBG(false);
                                            return task2;
                                        }
                                        if (task2.IsCompleted)
                                        {
                                            task2.Result.Reference.UpdateAsync("members", FieldValue.ArrayRemove(auth.CurrentUser.UserId));
                                            dbRef.Collection("Users").Document(auth.CurrentUser.UserId).DeleteAsync();
                                            auth.CurrentUser.DeleteAsync().ContinueWithOnMainThread(task => {
                                                if (task.IsCanceled)
                                                {
                                                    UIControl.I.SetWaitBG(false);
                                                    return task;
                                                }
                                                if (task.IsFaulted)
                                                {
                                                    UIControl.I.SetWaitBG(false);
                                                    return task;
                                                }
                                                if (task.IsCompleted)
                                                {
                                                    UIControl.I.SetWaitBG(false);
                                                    UIControl.I.LoginScreen();
                                                    return task;
                                                }
                                                return task;
                                            });
                                            return task2;
                                        }
                                        return task2;
                                    });
                        }
                        
                        return task1;
                    });
        } else {
            UIControl.I.SetWaitBG(false);
            return Task.FromResult(0);
        }
    }
    
    public Task RegisterAttempt(string newMail, string newPassword, string newName, string newRef)
    {
        dbRef = FirebaseFirestore.DefaultInstance;
        var query = dbRef.Collection("Groups").WhereEqualTo("referral", newRef);
        var snapshot = query.GetSnapshotAsync();
        return snapshot.ContinueWithOnMainThread((task =>
        {
            if (task.IsCanceled)
            {
                UIControl.I.RegisterCanceled("Referans kodu bulunamadı.");
                return task;
            }
            if (task.IsFaulted)
            {
                UIControl.I.RegisterFaulted("Referans kodu bulunamadı.");
                return task;
            }
            if (task.Result.Count != 1)
            {
                UIControl.I.RegisterFaulted("Referans kodu bulunamadı.");
                return task;
            }
            if (!task.Result[0].GetValue<bool>("isOpen"))
            {
                UIControl.I.RegisterFaulted("Bu gruba kayıt şu an açık değildir.");
                return task;
            }
            if (task.Result[0].GetValue<int>("maxMemberCount") <= task.Result[0].GetValue<List<string>>("members").Count)
            {
                UIControl.I.RegisterFaulted("Bu grup maksimum sayıya ulaşmıştır.");
                return task;
            }


            return auth.CreateUserWithEmailAndPasswordAsync(newMail, newPassword).ContinueWithOnMainThread(taskk =>
            {
                if (taskk.IsCanceled)
                {
                    UIControl.I.RegisterCanceled("Bir şeyler ters gitti.");
                    return taskk;
                }
                else if (taskk.IsFaulted)
                {
                    var errMsg = "Kayit başarısız:\n";
                    if (task.Exception != null)
                    {
                        foreach (Exception exception in task.Exception.Flatten().InnerExceptions) {
                            string authErrorCode = "";
                            Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                            if (firebaseEx != null) {
                                authErrorCode = String.Format("AuthError.{0}: ",
                                    ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                            }

                            errMsg += authErrorCode + exception.ToString() + "\n";
                        }
                    }
                    UIControl.I.RegisterFaulted(errMsg);
                    return taskk;
                }
                else if (taskk.IsCompleted)
                {
                    return UpdateUserProfileAttempt(newName, task5 => {}).ContinueWithOnMainThread((task1 =>
                    {
                        if (task1.IsCanceled)
                        {
                            Debug.Log("1");
                            UIControl.I.RegisterCanceled("Bir şeyler ters gitti.");
                            taskk.Result.User.DeleteAsync();
                            return task1;
                        }

                        if (task1.IsFaulted)
                        {
                            Debug.Log("2");
                            UIControl.I.RegisterFaulted("Bir şeyler ters gitti.");
                            taskk.Result.User.DeleteAsync();
                            return task1;
                        }

                        if (task1.IsCompleted)
                        {
                            Debug.Log("3");

                            lastRef = task.Result[0].Reference;
                            
                            var entryValues = new Dictionary<string, string>();
                            entryValues["email"] = newMail;
                            entryValues["fullname"] = newName;
                            entryValues["groupid"] = task.Result[0].Id;
                            entryValues["role"] = "student";
                            dbRef.Collection("Users").Document(taskk.Result.User.UserId).SetAsync(entryValues).ContinueWithOnMainThread(UserUpdateComplete);
                            
                            
                            
                            task.Result[0].Reference.UpdateAsync("members",
                                Firebase.Firestore.FieldValue.ArrayUnion(taskk.Result.User.UserId)).ContinueWithOnMainThread(MemberUpdateComplete);
                            Dictionary<string, Object> childUpdates = new Dictionary<string, Object>();
                        }

                        return task1;
                    })).Unwrap();
                }
                return taskk;
            }).Unwrap();
        }));
    }

    public void EditCar(string newRef, SavedCarProps newProps, Action<Task> onComplete)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            {"name", newProps.name},
            {"bodywork", newProps.kaportaId},
            {"tire", newProps.lastikId},
            {"engine", newProps.motorId},
            {"seat", newProps.koltukId},
            {"spoiler", newProps.ruzgarlikId},
        };
        dbRef.Collection("Users").Document(auth.CurrentUser.UserId).Collection("cars").Document(newRef).UpdateAsync(dict).ContinueWithOnMainThread(onComplete);
    }
    
    public void AddCar(SavedCarProps newProps, Action<Task<DocumentReference>> onComplete)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            {"name", newProps.name},
            {"bodywork", newProps.kaportaId},
            {"tire", newProps.lastikId},
            {"engine", newProps.motorId},
            {"seat", newProps.koltukId},
            {"spoiler", newProps.ruzgarlikId},
        };

        dbRef.Collection("Users").Document(auth.CurrentUser.UserId).Collection("cars").AddAsync(dict).ContinueWithOnMainThread(onComplete);

    }

    public void RemoveCar(string newRef, Action<Task> onComplete)
    {
        dbRef.Collection("Users").Document(auth.CurrentUser.UserId).Collection("cars").Document(newRef).DeleteAsync().ContinueWithOnMainThread(onComplete);
    }

    public bool DidLoadCars { get; set; } = false;
    
    public Task LoadCars(Action<Task> onComplete)
    {
        UIControl.I.SetWaitBG(true);
        dbRef = FirebaseFirestore.DefaultInstance;
        return dbRef.Collection("Users").Document(auth.CurrentUser.UserId).Collection("cars").GetSnapshotAsync()
            .ContinueWithOnMainThread(
        task =>
                {
                    if (task.IsCanceled)
                    {
                        UIControl.I.SetWaitBG(false);
                        return task;
                    }

                    if (task.IsFaulted)
                    {
                        UIControl.I.SetWaitBG(false);
                        return task;
                    }

                    if (task.IsCompleted)
                    {
                        dbRef.Collection("Users").Document(auth.CurrentUser.UserId).GetSnapshotAsync()
                            .ContinueWithOnMainThread(task1 =>
                            {
                                if (task1.IsCanceled)
                                {
                                    UIControl.I.SetWaitBG(false);
                                    return task1;
                                }

                                if (task1.IsFaulted)
                                {
                                    UIControl.I.SetWaitBG(false);
                                    return task1;
                                }

                                if (task1.IsCompleted)
                                {
                                    isStudent = task1.Result.GetValue<string>("role") == "student";
                                    SaveCarController.I.ClearCars();
                                    SaveCarController.I.SetCarCount(task.Result.Count);
                                    foreach (var documentSnapshot in task.Result.Documents)
                                    {
                                        Debug.Log(documentSnapshot.Id);
                                        SaveCarController.I.AddCar(new SavedCarProps
                                        {
                                            docPath = documentSnapshot.Id,
                                            name = documentSnapshot.GetValue<string>("name"),
                                            kaportaId = documentSnapshot.GetValue<int>("bodywork"),
                                            lastikId = documentSnapshot.GetValue<int>("tire"),
                                            motorId = documentSnapshot.GetValue<int>("engine"),
                                            koltukId = documentSnapshot.GetValue<int>("seat"),
                                            ruzgarlikId = documentSnapshot.GetValue<int>("spoiler"),
                                        });
                                    }

                                    DidLoadCars = true;
                                    onComplete?.Invoke(task);
                                    return task1;
                                }
                                return task1;
                            }).Unwrap();

                        return task;
                    }

                    return task;
                }).Unwrap();
    }
    
    private DocumentReference lastRef;
    private bool memberUpdateCompleted = false;
    private bool userUpdateCompleted = false;

    public void UserUpdateComplete(Task task)
    {
        if (task.IsCanceled)
        {
            lastRef.UpdateAsync("members", FieldValue.ArrayRemove(auth.CurrentUser.UserId));
            dbRef.Collection("Users").Document(auth.CurrentUser.UserId).DeleteAsync();
            auth.CurrentUser.DeleteAsync();
            UIControl.I.RegisterCanceled("Bir şeyler ters gitti.");
        }
        else if (task.IsFaulted)
        {
            lastRef.UpdateAsync("members", FieldValue.ArrayRemove(auth.CurrentUser.UserId));
            dbRef.Collection("Users").Document(auth.CurrentUser.UserId).DeleteAsync();
            auth.CurrentUser.DeleteAsync();
            UIControl.I.RegisterFaulted("Bir şeyler ters gitti.");
        }
        else if (task.IsCompleted)
        {
            userUpdateCompleted = true;
            if (memberUpdateCompleted)
            {
                userUpdateCompleted = false;
                memberUpdateCompleted = false;
                UIControl.I.RegisterEnd();
            }
        }
    }
    
    public void MemberUpdateComplete(Task task)
    {
        if (task.IsCanceled)
        {
            lastRef.UpdateAsync("members", FieldValue.ArrayRemove(auth.CurrentUser.UserId));
            dbRef.Collection("Users").Document(auth.CurrentUser.UserId).DeleteAsync();
            auth.CurrentUser.DeleteAsync();
            UIControl.I.RegisterCanceled("Bir şeyler ters gitti.");
        }
        else if (task.IsFaulted)
        {
            lastRef.UpdateAsync("members", FieldValue.ArrayRemove(auth.CurrentUser.UserId));
            dbRef.Collection("Users").Document(auth.CurrentUser.UserId).DeleteAsync();
            auth.CurrentUser.DeleteAsync();
            UIControl.I.RegisterFaulted("Bir şeyler ters gitti.");
        }
        else if (task.IsCompleted)
        {
            memberUpdateCompleted = true;
            if (userUpdateCompleted)
            {
                userUpdateCompleted = false;
                memberUpdateCompleted = false;
                UIControl.I.RegisterEnd();
            }
        }
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


    public Task UpdateUserProfileAttempt(string newDisplayName, Action<Task> onComplete)
    {
        if (auth.CurrentUser == null)
        {
            Debug.Log("WTF");
            return Task.FromResult(0);
        }

        return auth.CurrentUser.UpdateUserProfileAsync(new Firebase.Auth.UserProfile
        {
            DisplayName = newDisplayName
        }).ContinueWithOnMainThread(onComplete);
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
