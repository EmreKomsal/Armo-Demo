using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

[FirestoreData]
public class SessionTabData
{
    [FirestoreProperty]
    public string ScreenName { get; set; }

    [FirestoreProperty]
    public double Duration { get; set; }
}


[FirestoreData]
public class SessionGeneratedCarData
{
    [FirestoreProperty]
    public string CarId { get; set; }

    [FirestoreProperty]
    public double Duration { get; set; }
}


[FirestoreData]
public class SessionEditedCarData
{
    [FirestoreProperty]
    public string CarId { get; set; }

    [FirestoreProperty(ConverterType = typeof(int))]
    public object EditCount { get; set; }
}




public enum ScreenName
{
    NONE,
    MainPanel,
    ProfilePanel,
    GaragePanel,
    PlayPanel,
    NewCarPanel,
    CarEditPanel,
    ARInfoPanel,
    ARPistSizePanel,
    ARStartPanel,
    ARRacePanel,
    AREndRacePanel,
    ARLeaderboardPanel,
}













public class SessionLogger : SingletonNew<SessionLogger>
{
    private FirebaseFirestore dbRef;

    public bool DidCreateFile { get; private set; } = false;



    public bool IsRecordingTime { get; private set; } = false;
    public bool IsGeneratingCar { get; private set; } = false;
    public bool IsEditingCar { get; private set; } = false;


    public string CurrentSessionDocumentID { get; private set; } = "";

    public ScreenName CurrentRecordedScreenName { get; private set; } = ScreenName.NONE;


    public DateTime startTime;
    public DateTime startTimeGenerateCar;

    public void ResetSession()
    {
        DidCreateFile = false;
        IsRecordingTime = false;
        CurrentSessionDocumentID = "";
    }


    public void StartRecording(ScreenName screenName)
    {
        if (CurrentRecordedScreenName != ScreenName.NONE)
        {
            if (CurrentRecordedScreenName == ScreenName.NewCarPanel)
            {
                StopRecording();
            }
            else if (CurrentRecordedScreenName == ScreenName.CarEditPanel)
            {
                StopRecording();
            }
            else
            {
                StopRecording();
            }
        }
        // else
        // {
        //     StopRecording();
        //     return;
        // }

        IsRecordingTime = true;
        CurrentRecordedScreenName = screenName;
        startTime = DateTime.Now;
        startTimeGenerateCar = DateTime.Now;
    }
    
    
    public async void StopRecording(string arg0 = "")
    {
        if (!IsRecordingTime)
        {
            return;
        }

        IsRecordingTime = false;
        dbRef = FirebaseFirestore.DefaultInstance;

        var sn = CurrentRecordedScreenName;
        var s = startTime;

        var span = DateTime.Now - s;
        var secs = span.TotalSeconds;

        
        var dict = new Dictionary<string, object>
        {
            {"timeline", FieldValue.ArrayUnion(new SessionTabData { ScreenName = sn.ToString(), Duration = secs }) },
            {"totalDuration", FieldValue.Increment(secs)},
        };
        
        
        if (!String.IsNullOrWhiteSpace(arg0) && sn == ScreenName.NewCarPanel)
        {
            dict["generatedCars"] =
                FieldValue.ArrayUnion(new SessionGeneratedCarData { CarId = arg0, Duration = secs });
        }
        else if (!String.IsNullOrWhiteSpace(arg0) && sn == ScreenName.CarEditPanel)
        {
            dict[$"editedCars.{arg0}"] = FieldValue.Increment(1);
            
        }
        
        


        dbRef.Collection("Users").Document(AuthController.I.auth.CurrentUser.UserId).Collection("sessions")
            .Document(CurrentSessionDocumentID).UpdateAsync(dict).ContinueWithOnMainThread(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                }
                else
                {
                }
            });
    }


    public void StartGenerateCar()
    {
        IsGeneratingCar = true;
        IsEditingCar = false;
    }

    public void EndGenerateCar(string carId)
    {
        dbRef = FirebaseFirestore.DefaultInstance;
        
        var s = startTimeGenerateCar;

        var span = DateTime.Now - s;
        var secs = span.TotalSeconds;

        var dict = new Dictionary<string, object>
        {
            {"generatedCars", FieldValue.ArrayUnion(new SessionGeneratedCarData { CarId = carId, Duration = secs }) },
        };
        
        dbRef.Collection("Users").Document(AuthController.I.auth.CurrentUser.UserId).Collection("sessions")
            .Document(CurrentSessionDocumentID).UpdateAsync(dict);
        
        
        IsGeneratingCar = false;
        IsEditingCar = false;
    }



    public void StartEditCar()
    {
        IsGeneratingCar = false;
        IsEditingCar = false;
        
        
        
        
    }


    public void EndEditCar(string carId)
    {
        IsGeneratingCar = false;
        IsEditingCar = false;
    }
    
    
    
    public void CreateSessionFile()
    {
        
        dbRef = FirebaseFirestore.DefaultInstance;

        // var snap = await dbRef.Collection("Users").Document(AuthController.I.auth.CurrentUser.UserId).GetSnapshotAsync();

        Dictionary<string, object> dict = new Dictionary<string, object>
        {
            {"appStartedAt", DateTime.Now},
            {"timeline", new List<SessionTabData>()},
            {"totalDuration", 0d},
            {"generatedCars", new List<SessionGeneratedCarData>()},
            {"editedCars", new Dictionary<string, int>()},

        };

        dbRef.Collection("Users").Document(AuthController.I.auth.CurrentUser.UserId).Collection("sessions")
            .AddAsync(dict).ContinueWithOnMainThread(
                task =>
                {
                    if (task.IsFaulted)
                    {
                        CreateSessionFile();
                    }
                    else if (task.IsCanceled)
                    {
                        CreateSessionFile();
                    }
                    else if (task.IsCompletedSuccessfully)
                    {
                        Debug.LogError("CREATEDFILE");
                        CurrentSessionDocumentID = task.Result.Id;
                        DidCreateFile = true;
                        return;
                    }
                    CreateSessionFile();
                });
    }
}
