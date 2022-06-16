using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EOSDigital.API;
using EOSDigital.SDK;

namespace WebController.Services
{
    public class CameraService
    {
        CanonAPI APIHandler = new CanonAPI();

        public List<Camera> Cameras = new List<Camera>();

        public string ImageSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");

        private readonly AlertService alertSv;

        private bool isInit = false;

        public Stream? LiveImageStream;

        public static readonly string LIVE_KEY = "live";

        const int TIMEOUTMILISECONDS = 5000;

        public CameraService(AlertService alertSv)
        {
            GetCameras();

            APIHandler.CameraAdded += APIHandler_CameraAdded;

            this.alertSv = alertSv;

            isInit = true;

            alertSv.Notify("Found Devices", $"[{string.Join(", ", Cameras.ConvertAll(cam => cam.DeviceName))}]");
        }

        ~CameraService()
        {
            Cameras.ForEach(cam =>
            {
                cam.CloseSession();
                cam.Dispose();
            });
        }

        /// <summary>
        /// Check for available cameras and update <see cref="Cameras"/>
        /// </summary>
        public List<Camera> GetCameras() => this.Cameras = APIHandler.GetCameraList();

        /// <summary>
        /// Opens a session with the camera and binds camera events
        /// </summary>
        /// <param name="cam"></param>
        public void OpenSession(Camera cam)
        {
            if (cam.SessionOpen) return;

            cam.OpenSession();
            cam.LiveViewUpdated += Camera_LiveViewUpdated;
            cam.ProgressChanged += Camera_ProgressChanged;
            cam.StateChanged += Camera_StateChanged;
            cam.DownloadReady += Camera_DownloadReady;

            cam.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
            cam.SetCapacity(4096, int.MaxValue);

            alertSv.Notify("Session established", cam.DeviceName);
        }

        /// <summary>
        /// Closes session with the camera and disposes of events
        /// </summary>
        /// <param name="cam"></param>
        public void CloseSession(Camera cam)
        {
            if (!cam.SessionOpen) return;

            cam.CloseSession();
            cam.LiveViewUpdated -= Camera_LiveViewUpdated;
            cam.ProgressChanged -= Camera_ProgressChanged;
            cam.StateChanged -= Camera_StateChanged;
            cam.DownloadReady -= Camera_DownloadReady;

            alertSv.Notify("Session closed", cam.DeviceName);
        }

        /// <summary>
        /// Sends a capture command to the <paramref name="cam"/> and save it to <see cref="ImageSaveDirectory"/>
        /// Captures in [Bulb Mode] if <paramref name="bulbUpVal"/> is set to a value greater than -1
        /// </summary>
        /// <param name="cam"></param>
        /// <param name="bulbUpVal"></param>
        public async Task<DownloadInfo> Capture(Camera cam, int bulbUpVal = -1)
        {
            try
            {
                if (bulbUpVal >= 0)
                {
                    alertSv.Notify("Capturing in [BulbMode]...", "bulbUpVal: " + bulbUpVal);
                    cam.TakePhotoBulbAsync(bulbUpVal);
                }
                else
                {
                    alertSv.Notify("Capturing...");
                    cam.TakePhotoAsync();
                }

                var promise = new TaskCompletionSource<DownloadInfo>();

                DownloadHandler stopWaiting = (Camera sender, DownloadInfo Info) => promise.SetResult(Info);
                cam.DownloadReady += stopWaiting;

                DownloadInfo info = await promise.Task.WaitAsync(TimeSpan.FromMilliseconds(TIMEOUTMILISECONDS));

                cam.DownloadReady -= stopWaiting;

                return info;

            }
            catch (Exception ex)
            {
                alertSv.Error(ex);
                throw;
            }
        }

        #region event handlers

        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            GetCameras();
        }

        // Camera

        private void Camera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            alertSv.Event(nameof(Camera_DownloadReady), sender.DeviceName, Info.FileName);

            sender.DownloadFile(Info, ImageSaveDirectory);
        }

        private void Camera_StateChanged(Camera sender, StateEventID eventID, int parameter)
        {
            alertSv.Event(nameof(Camera_StateChanged), sender.DeviceName, eventID.ToString(), parameter.ToString());

            if (eventID == StateEventID.Shutdown && isInit)
            {
                // close session if the camera has been disconnected/shutdown
                CloseSession(sender);
            }
        }

        private void Camera_ProgressChanged(object sender, int progress)
        {
            Camera? cam = sender as Camera;
            alertSv.Event(nameof(Camera_ProgressChanged), cam?.DeviceName ?? "[UNKNOWN]", progress.ToString());
        }

        private void Camera_LiveViewUpdated(Camera sender, Stream img)
        {
            // Heavy
            LiveImageStream = img;
            alertSv.Event(nameof(Camera_LiveViewUpdated), "length: " + img.Length);
        }

        private void Camera_LiveViewStopped(Camera sender)
        {
            alertSv.Event(nameof(Camera_LiveViewStopped), sender.DeviceName, "IsLiveViewOn: " + sender.IsLiveViewOn);
        }

        #endregion

        public async Task LiveView_Start(Camera cam)
        {
            if (cam.IsLiveViewOn) return;

            var promise = new TaskCompletionSource();

            LiveViewUpdate stopWaiting = (Camera sender, Stream img) => promise.SetResult();

            cam.StartLiveView();
            cam.LiveViewUpdated += stopWaiting;

            await promise.Task.WaitAsync(TimeSpan.FromMilliseconds(TIMEOUTMILISECONDS));

            cam.LiveViewUpdated -= stopWaiting;
            cam.LiveViewStopped += Camera_LiveViewStopped;

            alertSv.Notify("LiveView started", cam.DeviceName);
        }

        public async Task LiveView_Stop(Camera cam)
        {
            if (!cam.IsLiveViewOn) return;

            var promise = new TaskCompletionSource();

            CameraUpdateHandler stopWaiting = (Camera sender) => promise.SetResult();

            cam.StopLiveView();
            cam.LiveViewStopped += stopWaiting;

            await promise.Task.WaitAsync(TimeSpan.FromMilliseconds(TIMEOUTMILISECONDS));

            cam.LiveViewStopped -= stopWaiting;
            cam.LiveViewStopped -= Camera_LiveViewStopped;

            alertSv.Notify("LiveView stopped", cam.DeviceName);
        }

    }
}
