using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using UnityEngine.Android;

public class ScreenshotHandler : MonoBehaviour
{
    public Button screenshotButton;      // Button to take a screenshot
    public Button viewScreenshotsButton; // Button to open gallery
    private string galleryPath;

    void Start()
    {
        screenshotButton.onClick.AddListener(TakeScreenshot);
        viewScreenshotsButton.onClick.AddListener(OpenGallery);

        // Request Storage Permission for Android 11+
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
        }
    }

    void TakeScreenshot()
    {
        StartCoroutine(CaptureScreenshot());
    }

    IEnumerator CaptureScreenshot()
    {
        // Hide UI before screenshot
        screenshotButton.gameObject.SetActive(false);
        viewScreenshotsButton.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame(); // Wait for the frame to render

        string fileName = "Screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
        galleryPath = Path.Combine("/storage/emulated/0/Pictures/", fileName); // Save to Pictures folder

        ScreenCapture.CaptureScreenshot(fileName);
        string tempPath = Path.Combine(Application.persistentDataPath, fileName);

        yield return new WaitForSeconds(1); // Wait for the screenshot to save

        // Show UI after screenshot
        screenshotButton.gameObject.SetActive(true);
        viewScreenshotsButton.gameObject.SetActive(true);

        if (File.Exists(tempPath))
        {
            File.Move(tempPath, galleryPath); // Move file to Pictures
            RefreshGallery(galleryPath);
            Debug.Log("Screenshot saved to Gallery: " + galleryPath);
        }
    }

    void OpenGallery()
    {
        #if UNITY_ANDROID
            AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent");
            intent.Call<AndroidJavaObject>("setAction", "android.intent.action.VIEW");

            AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uri = uriClass.CallStatic<AndroidJavaObject>("parse", "content://media/internal/images/media");
            
            intent.Call<AndroidJavaObject>("setData", uri);
            intent.Call<AndroidJavaObject>("setFlags", 1); // FLAG_ACTIVITY_NEW_TASK

            AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("startActivity", intent);
        #endif
    }

    void RefreshGallery(string path)
    {
        // Refresh Gallery so the screenshot appears
        AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection");
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        mediaScanner.CallStatic("scanFile", activity, new string[] { path }, null, null);
    }
}
