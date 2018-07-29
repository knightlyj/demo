using UnityEngine;
using System.Collections;

public enum HostType
{
    Server,
    Client,
}


public static class GlobalVariables {
    public static bool mobileUIOnPC = false;

    public static Player localPlayer = null;
    public static HostType hostType = HostType.Server;

    public static CameraControl cameraControl = null;
    public static UIManager uiManager = null;

    public static Protocol.InitServerGameInfo clientInitInfo = null;

    public static bool menuOpened = false;
}

static class StringAssets
{
    /// <summary>
    /// scenes
    /// </summary>
    public readonly static string mainMenuSceneName = "Menu";
    public readonly static string loadingSceneName = "Loading";
    public readonly static string gamePlaySceneName = "GamePlay";

    /// <summary>
    /// layers
    /// </summary>
    public readonly static string groundLayerName = "Ground";
    public readonly static string bodyLayerName = "PlayerBody";

    /// <summary>
    /// tags
    /// </summary>
    public readonly static string localPlayerTag = "LocalPlayer";
    public readonly static string AIPlayerTag = "AIPlayer";
    public readonly static string remoteplayerTag = "RemotePlayer";

    /// <summary>
    /// sound
    /// </summary>
    public readonly static string soundPath = "Sound/";
    public readonly static string bgmPath = "Sound/BGM/";

}
