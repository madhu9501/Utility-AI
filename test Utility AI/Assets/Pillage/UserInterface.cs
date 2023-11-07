using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Linq;
// using Firebase.Database;
// using ConstantVariables;
using DG.Tweening;
/// <summary>
/// This class controls some screens logic(homescreen, loginscreen etc)
/// Aslo contains playersavedata logic saves and loads data from playerprefs
/// </summary>
public class UserInterface : SingletonBaseClass<UserInterface>
{
   
    public List<GraphicRaycaster> RaycastBlocker = new List<GraphicRaycaster>();

    public GameObject commonBackground;

    [Header("[ Screen ]")]
    // public ScreenHolder screenData;
    public UnityAction<ScreenNames> OnScreenSwitched;

    // public BaseScreen currentScreen = null;
    public Transform screenParent;

    [Header("[ Popups ]")]
    public Transform popupParent;
    public GameObject popupBlocker;

    // public CommonSpriteDirectiory spriteDirectory;
    // public LevelSystem levelSystemData;
    // public CountryData countryData;
    public GameObject directionalLight;
    public GameObject eventSystemUI;

    //for saving the previous saved records
    public PlayerSaveData preSavedPlayerData;

    // @Divesh - Reference to PlayerSaveData
    public PlayerSaveData playerSaveData
    {
        get
        {
            var _data = PlayerPrefs.GetString("SaveData", JsonUtility.ToJson(this));
            // Logger.Log("get save data " + _data);
            return JsonUtility.FromJson<PlayerSaveData>(_data);
        }
        set
        {
            // Logger.Log("Try saving data on server");
            var _data = JsonUtility.ToJson(value);
            //@Divesh - Save Data on local
            PlayerPrefs.SetString("SaveData", _data);
            //@Divesh - Save Data on Server
            SaveDataOnServer(_data,value);
        }
    }

    public void Start()
    {
        if (!string.IsNullOrEmpty(playerSaveData._name))
        {
            // Logger.Log("Save data Found");
            // SwitchScreen(ScreenNames.Home);
        }
        else
        {
            // Logger.Log("No save data Found");
            // SwitchScreen(ScreenNames.Login);
        }
    }



    public void SaveDataOnServer(string json, PlayerSaveData playerData)
    {
#if UNITY_EDITOR
        return;
#endif
       if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            return;
            // Logger.Log("Cannot Save no internet");
        }

        // @Divesh - Get reference to firebase db
        // // // DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        //@Divesh - Check if user has created a uid if yes save data on server 
        // if (playerData.userId != ConstantVars.STR_NOTAPPLICABLE)
        // {
        //     // Logger.Log("uid = " + playerData.userId);
        //     // reference.Child("users").Child(playerData.userId).SetRawJsonValueAsync(json);
        //     // Logger.Log("Save data on server");
        //     // LoginManager.instance.InitiateGooglePlayGames();
        // }
        //@Divesh - Authenticate user and save data on server
        else
        {
            // Logger.Log("Logging in as guest");
            // LoginManager.instance.OnAnonumyousLoginBackend();
        }
        
    }

    /// <param name="message">Message string to show in the toast.</param>
    public void _ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }


    // public BaseScreen GetCurrentScreen(ScreenNames _screen)
    // {
    // //    var currentScreens =  screenParent.GetComponentsInChildren<BaseScreen>();

    //     // foreach(BaseScreen screen in currentScreens)
    //     {
    //         if(screen.screenName == _screen)
    //         {
    //             // currentScreen = screen;
    //         }
    //     }

    //     // return currentScreen;
    // }
    // public void SwitchScreen(ScreenNames _screen, object screenParams = null)
    // {
    //     // Logger.Log($"Switch SCreen - [{_screen}]");

    //     // // currentScreen = GetCurrentScreen(_screen);
    //     if (_screen == ScreenNames.GameOver)
    //         Time.timeScale = 0;
    //     else
    //         Time.timeScale = 1;

    //     if (_screen == ScreenNames.Home)
    //     {
    //         UserData.ResetData();
    //         SetCommonBG(true);
    //     }

    //     EnableDisableTouchInput(false);
    //     // screenData.GetScreen(_screen, screenParams, screenParent, () => EnableDisableTouchInput(true));
    //     OnScreenSwitched?.Invoke(_screen);
    // }


    public void ShowPopup(ScreenNames popupName, object screenParams = null)
    {
        // Logger.Log($"ShowPopup - {popupName}");
        // EnableDisableTouchInput(false);
        popupBlocker.SetActive(true); // todo - overllapping popups
        // screenData.ShowPopup(popupName, screenParams, popupParent, () => EnableDisableTouchInput(true));
    }

    // public void DestryPopup(BaseScreen _popup)
    // {
    //     EnableDisableTouchInput(false);
        
    //     // screenData.DestroyPopup(_popup, () => {
    //         popupBlocker.SetActive(false);
    //         EnableDisableTouchInput(true);
    //     });
    // }

    // public void EnableDisableTouchInput(bool isEnable)
    // {
    //     RaycastBlocker.ForEach(r => r.enabled = isEnable);
    // }

    // public void SetCommonBG(bool isEnable)
    // {
    //     if(isEnable == false)
    //     {
    //         commonBackground.GetComponent<RawImage>().DOFade(0, ConstantVars.VAL_QUATER).OnComplete (() => commonBackground.SetActive(isEnable));   
    //     }
    //     else
    //     {
    //         commonBackground.GetComponent<RawImage>().DOFade(1, ConstantVars.VAL_QUATER).OnComplete(() => commonBackground.SetActive(isEnable));
    //     }
      
    //     directionalLight.SetActive(isEnable);
    //     eventSystemUI.SetActive(isEnable);
    // }

    // public void SetCanvasRenderMode(RenderMode renderMode)
    // {
    //     screenParent.GetComponent<Canvas>().renderMode = renderMode;
    // }

    // public void ChangedCameraMode(bool orthoMode)
    // {
    //     Camera.main.orthographic = orthoMode;
    // }

 
 
}

public enum ScreenNames
{
    None = 0,
    Login = 1,
    Profile = 2,
    Home= 3,
    ChooseWarrior = 4,
    Loading = 5,
    GameScreen = 6,
    ProfileEdit = 7,
    CreateProfile = 8,
    PauseScreen = 9,
    GameOver = 10,
    RateUS = 11,
    DisableAds = 12,
    Onboarding = 13,
    MenuTutorial = 14,
    HelpScreen = 15,
    //@Divesh - added new popups
    GoogleSignIn = 16,
    ProfileLoginScreen = 17,
    GoogleSignInLeaderBoards = 18,
    SettingPopup = 19
}

public class PlayerSaveData
{
    #region STRUCT -------------------------------->>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
    
    public enum GENDER
    {
        NA,
        MALE,
        FEMALE,
        OTHER
    }

    [System.Serializable]
    public class WarriorData
    {
        public WarriorType warriorType;
        public int warriorSelected;

        public WarriorData(WarriorType _type, int _value)
        {
            warriorType = _type;
            warriorSelected = _value;
        }
    }

    [System.Serializable]
    public class PowerupData
    {
        public PowerType powerType;
        public int powerupUsed;

        public PowerupData(PowerType _type, int _value)
        {
            powerType = _type;
            powerupUsed = _value;
        }
    }

    [System.Serializable]
    public class LevelModeData
    {
        public LevelMode mode;
        public int levelSelected;

        public LevelModeData(LevelMode _type, int _value)
        {
            mode = _type;
            levelSelected = _value;
        }
    }

    public enum GameResult
    {
        Win,
        Lost
    }

    [System.Serializable]
    public class LevelDataPerMode
    {
        public LevelMode levelMode;
        public int _gameWon; // level complete
        public int _gameLost; // level complete
        public int _warirorsCaptured; // on ccapture
        public int _warriorsLost; // on get capture
       

        public LevelDataPerMode(LevelMode _mode, int won, int lost, int wCap, int wLost)
        {
            levelMode = _mode;
            _gameWon = won;
            _gameLost = lost;
            _warirorsCaptured = wCap;
            _warriorsLost = wLost;
        }
    }

    #endregion

    public string _name;  // store at profile screens
    public int _country; // profile screen
    public GENDER _gender; // profile screen
    public string _avatarId = "0"; // profile screen
    public string userId = "N/A";
    public bool isGooglePlayAuthValid;

    public bool canAdsShow = true;
    public bool musicMuted;
    public bool sfxMuted;
    public bool isCameraToggled;
    public bool isVibrationActive = true;

    public List<LevelModeData> _levelmode = new List<LevelModeData>(); // on level start
    public List<WarriorData> _warriorSelected = new List<WarriorData>(); // on level sart
    public List<PowerupData> _powerData = new List<PowerupData>(); // on power used gameplay

    public int _xpScore = 0;
    public int _levelNumber = 1;

    public List<LevelDataPerMode> _levelPerModeData = new List<LevelDataPerMode>();

    public GameResult _lastGameResult;


    public void UpdateProfile(string _name = "", int _country = 0
        ,GENDER _gender = GENDER.NA, string _avatarId = "0")
    {
        if (!string.IsNullOrEmpty(_name))
            this._name = _name;
        //if (_country > 0)
            this._country = _country;
        if (_gender != GENDER.NA)
            this._gender = _gender;
        //if (_avatarId != "0")
            this._avatarId = string.IsNullOrEmpty(_avatarId) ? "0" : _avatarId;
    }

    public void UpdateFavData(LevelMode _levelSelected = LevelMode.NA,
                WarriorType _warriorUsed = WarriorType.NA,
                PowerType _powerUsed = PowerType.NA)
    {
        if (_levelSelected != LevelMode.NA)
        {
            if (_levelmode.Exists(x => x.mode == _levelSelected))
                _levelmode.Find(x => x.mode == _levelSelected).levelSelected++;
            else
                _levelmode.Add(new LevelModeData(_levelSelected, 1));
        }

        if (_warriorUsed != WarriorType.NA)
        {
            if (_warriorSelected.Exists(x => x.warriorType == _warriorUsed))
                _warriorSelected.Find(x => x.warriorType == _warriorUsed).warriorSelected++;
            else
                _warriorSelected.Add(new WarriorData(_warriorUsed, 1));
        }

        if (_powerUsed != PowerType.NA)
        {
            // Logger.Log("Adding power data");
            if (_powerData.Exists(x => x.powerType == _powerUsed))
                _powerData.Find(x => x.powerType == _powerUsed).powerupUsed++;
            else
                _powerData.Add(new PowerupData(_powerUsed, 1));
        }
    }

    public void AddXP(int xpVal)
    {
        // var _lvlXpData = UserInterface.instance.levelSystemData;

        for (int i = 0; i < xpVal; i++)
        {
            _xpScore++;
            // if(_lvlXpData.GetXPForNextLevel(_levelNumber) <= (_lvlXpData.GetXPForLevel(_levelNumber) + _xpScore) )
            // {
            //     // upgrade the level 
            //     _levelNumber++;
            //     _xpScore = 0;

            //     // EngagementManager.instance.LogFirebaseAnalyticsWithParams("level_increase_count","level_number",_levelNumber);
            // }
        }
    }

    public void UpdateGameLevelData(LevelMode _mode, int WCap = 0, int WLost = 0, int matchWon = 0, int matchLost = 0)
    {
        if(_levelPerModeData.Exists(l => l.levelMode == _mode))
        {
            // update the data
            var _data = (_levelPerModeData.Find(l => l.levelMode == _mode));
            _data._gameWon += matchWon;
            _data._gameLost += matchLost;
            _data._warirorsCaptured += WCap;
            _data._warriorsLost += WLost;
        }
        else
        {
            // add first time
            _levelPerModeData.Add(new LevelDataPerMode(_mode, matchWon, matchLost, WCap, WLost));
        }
    }

    public void SetAvatarAndLevel(ref Image _icon,ref TextMeshProUGUI _text, ref Image _countryFlag)
    {
        // _icon.sprite = UserInterface.instance.spriteDirectory.GetAvatar(_avatarId);
        // _countryFlag.sprite = UserInterface.instance.countryData.GetFlag(_country);
        _text.text = _levelNumber.ToString();
    }

    public LevelMode GetFavLevelMode() => _levelmode.Count > 0 ? _levelmode.OrderByDescending(x => x.levelSelected).First().mode : LevelMode.FourPlayer;
    public WarriorType GetFavWarriore() => _warriorSelected.Count > 0 ? _warriorSelected.OrderByDescending(x => x.warriorSelected).First().warriorType : WarriorType.Red;
    public PowerType GetFavPower() => _powerData.Count > 0 ? _powerData.OrderByDescending(x => x.powerupUsed).First().powerType : PowerType.Release;

    public string GetFavPowerText()
    {
        return GetFavPower().ToString();
    }
}


