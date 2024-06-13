using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

public class RequestPermissionScript : MonoBehaviour
{
    public void AskForPermissionsNEI()
    {
        StartCoroutine(AskForPermissions());
    }
    private IEnumerator AskForPermissions()
    {
#if UNITY_ANDROID
        List<bool> permissions = new() { false, false, false, false };
        List<bool> permissionsAsked = new() { false, false, false, false };
        List<Action> actions = new()
    {
        new Action(() => {
            permissions[0] = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN");
            if (!permissions[0] && !permissionsAsked[0])
            {
                Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
                permissionsAsked[0] = true;
                return;             
            }
        }),
        new Action(() => {
            permissions[1] = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT");
            if (!permissions[1] && !permissionsAsked[1])
            {
                Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
                permissionsAsked[1] = true;
                return;
            }
        }),
        new Action(() => {
            permissions[2] = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH");
            if (!permissions[2] && !permissionsAsked[2])
            {
                Permission.RequestUserPermission("android.permission.BLUETOOTH");
                permissionsAsked[2] = true;
                return;
            }
        }),
        new Action(() => {
            permissions[3] = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADMIN");
            if (!permissions[3] && !permissionsAsked[3])
            {
                Permission.RequestUserPermission("android.permission.BLUETOOTH_ADMIN");
                permissionsAsked[3] = true;
                return;
            }
        })
    }; 
        for (int i = 0; i < permissionsAsked.Count;)
        {
            actions[i].Invoke();
            if (permissions[i])
            {
                ++i;
            }
            yield return new WaitForEndOfFrame();
        }
#endif
    }
}
