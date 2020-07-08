using UnityEngine;
using System.Collections.Generic;

public class WifiManager
{
    private static WifiManager instance = new WifiManager();

    public static WifiManager GetInstance()
    {
        return instance;
    }

    private WifiManager()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                Debug.Log("Context: " + context);

                using (AndroidJavaObject nsdManager = context.Call<AndroidJavaObject>("getSystemService", "servicediscovery"))
                {
                    Debug.Log("NsdManager: " + nsdManager);
                    this.nsdManager = nsdManager;
                }
            }
        }
#endif
    }

    /// <summary>
    /// Multicast機能の有効無効設定
    /// </summary>
    /// <param name="enabled"></param>
    public void SetEnabledMulticast(bool enabled)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
                using (var multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "NDI_LOCK"))
                {

                    if (enabled)
                    {
                        try
                        {
                            multicastLock.Call("release");
                        }
                        catch
                        {

                        }
                    }
                    else
                    {
                        try
                        {
                            multicastLock.Call("acquire");
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
#endif
    }

    public bool IsEnabledMulticast()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
                using (var multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", MULTI_CAST_LOCK_NAME))
                {
                    return multicastLock.Call<bool>("isHeld");
                }
            }
        }
#endif

        return true;
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject nsdManager = null;
    const string MULTI_CAST_LOCK_NAME = "NDI_LOCK";
#endif
}
