﻿using Android.App;
using Android.OS;
using System.Threading.Tasks;
using Android;
using AndroidX.Core.App;
using Google.Android.Material.Snackbar;

namespace DeafHearingAID.Droid.Services
{
    class MicrophoneService : IMicrophoneService
    {
        public const int REQUEST_MIC = 1;
        private string[] permissions = { Manifest.Permission.RecordAudio, Manifest.Permission.ReadExternalStorage };
        private TaskCompletionSource<bool> tcsPermissions;

        public Task<bool> GetPermissionsAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();

            // Permissions are required only for Marshmallow and up
            if ((int)Build.VERSION.SdkInt < 23)
            {
                tcsPermissions.TrySetResult(true);
            }
            else
            {
                var currentActivity = MainActivity.Instance;
                if ((ActivityCompat.CheckSelfPermission(currentActivity, Manifest.Permission.RecordAudio) != (int)Android.Content.PM.Permission.Granted) ||
                    ActivityCompat.CheckSelfPermission(currentActivity, Manifest.Permission.ReadExternalStorage) != (int)Android.Content.PM.Permission.Granted)
                {
                    RequestMicPermission();
                }
                else
                {
                    tcsPermissions.TrySetResult(true);
                }
            }
            return tcsPermissions.Task;
        }

        private void RequestMicPermission()
        {
            var currentActivity = MainActivity.Instance;
            if (ActivityCompat.ShouldShowRequestPermissionRationale(currentActivity, Manifest.Permission.RecordAudio))
            {
                Snackbar.Make(currentActivity.FindViewById((Android.Resource.Id.Content)), "App requires microphone permission.", Snackbar.LengthIndefinite).SetAction("Ok", v =>
                {
                    ((Activity)currentActivity).RequestPermissions(permissions, REQUEST_MIC);

                }).Show();
            }
            else
            {
                ActivityCompat.RequestPermissions(((Activity)currentActivity), permissions, REQUEST_MIC);
            }
        }

        public void OnRequestPermissionsResult(bool isGranted)
        {
            tcsPermissions.TrySetResult(isGranted);
        }
    }
}
