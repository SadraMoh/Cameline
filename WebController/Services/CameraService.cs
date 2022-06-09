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

        List<Camera> Cameras = new List<Camera>();
        Camera? MainCamera { get => this.Cameras.First(); }

        string ImageSaveDirectory = Directory.GetCurrentDirectory();

        bool Error = false;
        ManualResetEvent WaitEvent = new ManualResetEvent(false);

        public CameraService()
        {
            Cameras = APIHandler.GetCameraList();
        }

        ~CameraService()
        {
            Cameras.ForEach(cam =>
            {
                cam.CloseSession();
                cam.Dispose();
            });
        }

        public void Capture(Camera cam)
        {
            cam.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
            cam.SetCapacity(4096, int.MaxValue);
        }

        public bool OpenSession(Camera cam, Func<Camera, DownloadInfo, bool> downloadReady)
        {
            cam.DownloadReady += (Camera sender, DownloadInfo Info) => downloadReady.Invoke(sender, Info);
            cam.OpenSession();

            Console.WriteLine($"Opened session with camera: {cam.DeviceName}");
            return true;
        }
    }
}
