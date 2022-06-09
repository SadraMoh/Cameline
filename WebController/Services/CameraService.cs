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
        public List<Camera> Cameras = new List<Camera>();
        public Camera? MainCamera { get => this.Cameras.First(); }

        public string ImageSaveDirectory = Directory.GetCurrentDirectory();


        CanonAPI APIHandler = new CanonAPI();

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

        public bool Capture(Camera cam)
        {
            cam.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
            cam.SetCapacity(4096, int.MaxValue);

            Console.WriteLine("Taking photo with current settings...");

            CameraValue tv = TvValues.GetValue(cam.GetInt32Setting(PropertyID.Tv));
            if (tv == TvValues.Bulb)
                cam.TakePhotoBulb(2);
            else
                cam.TakePhoto();

            WaitEvent.WaitOne();

            if (!Error)
            {
                Console.WriteLine("Photo taken and saved");
                return true;
            }

            return false;
        }

        public bool OpenSession(Camera cam, Func<Camera, DownloadInfo, bool>? downloadReady = null)
        {
            cam.DownloadReady += (Camera sender, DownloadInfo Info) => downloadReady?.Invoke(sender, Info);
            cam.OpenSession();

            Console.WriteLine($"Opened session with camera: {cam.DeviceName}");
            return true;
        }
    }
}
