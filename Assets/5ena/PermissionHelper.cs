using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;

//
// Permissionのヘルパークラス
//
public static class PermissionHelper
{
    public static IEnumerator RequestPermissions(List<string> permissions, UnityAction callback)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("SetUpPermission");

        foreach (var permission in permissions)
        {
            if (!Permission.HasUserAuthorizedPermission(permission))
            {
                yield return RequestUserPermission(permission);
            }
        }
#endif
        callback.Invoke();

        yield break;
    }

    static bool isRequesting = false;
    public static IEnumerator RequestUserPermission(string permission)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
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
                yield break;
            }
            timeElapsed += Time.deltaTime;

            yield return null;
        }
#endif
        yield break;
    }

}
