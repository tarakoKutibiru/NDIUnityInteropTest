using UnityEngine;
using System.Collections.Generic;

public class WifiManager
{
    private static WifiManager instance = new WifiManager();

    public static WifiManager GetInstance()
    {
        return instance;
    }

    private WifiManager() { }

    public void SetupNetwork()
    {
        // The NDI SDK for Android uses NsdManager to search for NDI video sources on the local network.
        // So we need to create and maintain an instance of NSDManager before performing Find, Send and Recv operations.
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            {
                using (AndroidJavaObject nsdManager = context.Call<AndroidJavaObject>("getSystemService", "servicediscovery"))
                {
                    this.nsdManager = nsdManager;
                }
            }
        }
#endif
    }

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
