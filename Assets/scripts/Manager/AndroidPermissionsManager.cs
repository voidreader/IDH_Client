using System;
using UnityEngine;

public class AndroidPermissionCallback : AndroidJavaProxy
{
    private event Action<string> OnPermissionGrantedAction;
    private event Action<string> OnPermissionDeniedAction;
    private event Action<string> OnPermissionDeniedAndDontAskAgainAction;

    public AndroidPermissionCallback(Action<string> onGrantedCallback, Action<string> onDeniedCallback, Action<string> onDeniedAndDontAskAgainCallback)
        : base("com.unity3d.plugin.UnityAndroidPermissions$IPermissionRequestResult2")
    {
        if (onGrantedCallback != null)
        {
            OnPermissionGrantedAction += onGrantedCallback;
        }
        if (onDeniedCallback != null)
        {
            OnPermissionDeniedAction += onDeniedCallback;
        }
        if (onDeniedAndDontAskAgainCallback != null)
        {
            OnPermissionDeniedAndDontAskAgainAction += onDeniedAndDontAskAgainCallback;
        }
    }

    // Handle permission granted
    public virtual void OnPermissionGranted(string permissionName)
    {
        //Debug.Log("Permission " + permissionName + " GRANTED");
        if (OnPermissionGrantedAction != null)
        {
            OnPermissionGrantedAction(permissionName);
        }
    }

    // Handle permission denied
    public virtual void OnPermissionDenied(string permissionName)
    {
        //Debug.Log("Permission " + permissionName + " DENIED!");
        if (OnPermissionDeniedAction != null)
        {
            OnPermissionDeniedAction(permissionName);
        }
    }

    // Handle permission denied and 'Dont ask again' selected
    // Note: falls back to OnPermissionDenied() if action not registered
    public virtual void OnPermissionDeniedAndDontAskAgain(string permissionName)
    {
        //Debug.Log("Permission " + permissionName + " DENIED and 'Dont ask again' was selected!");
        if (OnPermissionDeniedAndDontAskAgainAction != null)
        {
            OnPermissionDeniedAndDontAskAgainAction(permissionName);
        }
        else if (OnPermissionDeniedAction != null)
        {
            // Fall back to OnPermissionDeniedAction
            OnPermissionDeniedAction(permissionName);
        }
    }
}

public class AndroidPermissionsManager
{
    private static AndroidJavaObject m_Activity;
    private static AndroidJavaObject m_PermissionService;

    private static AndroidJavaObject GetActivity()
    {
        if (m_Activity == null)
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            m_Activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
        return m_Activity;
    }

    private static AndroidJavaObject GetPermissionsService()
    {
        return m_PermissionService ??
            (m_PermissionService = new AndroidJavaObject("com.unity3d.plugin.UnityAndroidPermissions"));
    }

    public static bool IsPermissionGranted(string permissionName)
    {
        return true;
        // return GetPermissionsService().Call<bool>("IsPermissionGranted", GetActivity(), permissionName);
    }

    public static void RequestPermission(string permissionName, AndroidPermissionCallback callback)
    {
        RequestPermission(new[] { permissionName }, callback);
    }

    public static void RequestPermission(string[] permissionNames, AndroidPermissionCallback callback)
    {
        // GetPermissionsService().Call("RequestPermissionAsync", GetActivity(), permissionNames, callback);
    }
}



public class AndroidPermissionsUsageExample : MonoBehaviour
{
    private const string STORAGE_PERMISSION = "android.permission.READ_EXTERNAL_STORAGE";

    // Function to be called first (by UI button)
    // For example, click on Avatar to change it from the device gallery
    public void OnBrowseGalleryButtonPress()
    {
        if (!CheckPermissions())
        {
            Debug.LogWarning("Missing permission to browse device gallery, please grant the permission first");

            // Your code to show in-game pop-up with the explanation why you need this permission (required for Google Featuring program)
            // This pop-up should include a button "Grant Access" linked to the function "OnGrantButtonPress" below
            return;
        }

        // Your code to browse Android Gallery
        Debug.Log("Browsing Gallery...");
    }

    private bool CheckPermissions()
    {
        if (Application.platform != RuntimePlatform.Android)
        {
            return true;
        }

        return AndroidPermissionsManager.IsPermissionGranted(STORAGE_PERMISSION);
    }

    public void OnGrantButtonPress()
    {
        AndroidPermissionsManager.RequestPermission(new[] { STORAGE_PERMISSION }, new AndroidPermissionCallback(
            grantedPermission =>
            {
                // The permission was successfully granted, restart the change avatar routine
                OnBrowseGalleryButtonPress();
            },
            deniedPermission =>
            {
                // The permission was denied
            },
            deniedPermissionAndDontAskAgain =>
            {
                // The permission was denied, and the user has selected "Don't ask again"
                // Show in-game pop-up message stating that the user can change permissions in Android Application Settings
                // if he changes his mind (also required by Google Featuring program)
            }));
    }
}