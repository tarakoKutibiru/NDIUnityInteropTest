using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using NDI;
using System;
using UnityEngine.Android;
using System.Collections;

public class SourceSelector : MonoBehaviour
{
    [SerializeField] Dropdown _dropdown = null;

    NdiReceiver _receiver = null;
    List<string> _sourceNames;
    bool _disableCallback;

    // HACK: Assuming that the dropdown has more than
    // three child objects only while it's opened.
    bool IsOpened => _dropdown.transform.childCount > 3;

    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(this.SetUpPermission());
#else
        _receiver = GetComponent<NdiReceiver>();
#endif
    }

    void SetUpWifi()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        {
            using (var wifiManager = activity.Call<AndroidJavaObject>("getSystemService", "wifi"))
            {
                Debug.Log(wifiManager);

                using (var multicastLock = wifiManager.Call<AndroidJavaObject>("createMulticastLock", "NDI_LOCK"))
                {
                    Debug.Log(multicastLock);

                    if (multicastLock.Call<bool>("isHeld"))
                    {
                        Debug.Log("Wifi Multicast is Lock.");
                    }
                    else
                    {
                        Debug.Log("Wifi Multicast is Unlock.");
                    }

                    try
                    {
                        multicastLock.Call("release");
                    }
                    catch
                    {
                        // ignoring this exception, probably wakeLock was already released
                    }
                    finally
                    {
                        if (multicastLock.Call<bool>("isHeld"))
                        {
                            Debug.Log("Wifi Multicast is Lock.");
                        }
                        else
                        {
                            Debug.Log("Wifi Multicast is Unlock.");
                        }
                    }
                }
            }
        }
#endif
    }

    IEnumerator SetUpPermission()
    {

#if UNITY_ANDROID&& !UNITY_EDITOR
        Debug.Log("SetUpPermission");

        List<string> permissions = new List<string>();
        permissions.Add(Permission.Camera);
        permissions.Add("android.permission.ACCESS_WIFI_STATE");
        permissions.Add("android.permission.CHANGE_WIFI_MULTICAST_STATE");
        permissions.Add("android.permission.ACCESS_NETWORK_STATE");

        foreach (var permission in permissions)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                // 権限が無いので、カメラパーミッションのリクエストをする
                yield return RequestUserPermission(permission);
            }
        }

        // 権限が許可されたので、権限が必要なAPIを使用する処理へ進む
        this.SetUpWifi();
        _receiver = GetComponent<NdiReceiver>();
#endif

        yield break;
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    bool isRequesting = false;
    IEnumerator RequestUserPermission(string permission)
    {
        Debug.Log("RequestUserPermission");

        isRequesting = true;
        Permission.RequestUserPermission(permission);
        // Androidでは「今後表示しない」をチェックされた状態だとダイアログは表示されないが、フォーカスイベントは通常通り発生する模様。
        // したがってタイムアウト処理は本来必要ないが、万が一の保険のために一応やっとく。

        // アプリフォーカスが戻るまで待機する
        float timeElapsed = 0;
        while (isRequesting)
        {
            if (timeElapsed > 0.5f)
            {
                isRequesting = false;
                Debug.Log("RequestUserPermission:Start");
                yield break;
            }
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        yield break;
    }
#endif

    void Update()
    {
        if (this._receiver == null) return;

        // Do nothing if the menu is opened.
        if (IsOpened) return;

        // NDI source name retrieval
        _sourceNames = NdiFinder.sourceNames.ToList();

        // Currect selection
        var index = _sourceNames.IndexOf(_receiver.ndiName);

        // Append the current name to the list if it's not found.
        if (index < 0)
        {
            index = _sourceNames.Count;
            _sourceNames.Add(_receiver.ndiName);
        }

        // Disable the callback while updating the menu options.
        _disableCallback = true;

        // Menu option update
        _dropdown.ClearOptions();
        _dropdown.AddOptions(_sourceNames);
        _dropdown.value = index;
        _dropdown.RefreshShownValue();

        // Resume the callback.
        _disableCallback = false;
    }

    public void OnChangeValue(int value)
    {
        if (_disableCallback) return;
        _receiver.ndiName = _sourceNames[value];
    }
}
