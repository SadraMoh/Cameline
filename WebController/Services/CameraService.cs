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
        public Camera MainCamera { get => this.Cameras.First(); }

        public string ImageSaveDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "RemotePhoto");


        CanonAPI APIHandler = new CanonAPI();

        bool Error = false;

        //ManualResetEvent WaitEvent = new ManualResetEvent(false);

        public CameraService()
        {
            Cameras = APIHandler.GetCameraList();

            if (!OpenSession())
            {
                Console.WriteLine("No camera found. Please plug in camera");
                APIHandler.CameraAdded += APIHandler_CameraAdded;
                
            }
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
            try
            {
                //WaitEvent.WaitOne();
                //WaitEvent.Reset();

                if (!Error)
                {
                    MainCamera.SetSetting(PropertyID.SaveTo, (int)SaveTo.Host);
                    MainCamera.SetCapacity(4096, int.MaxValue);
                    Console.WriteLine($"Set image output path to: {ImageSaveDirectory}");

                    Console.WriteLine("Taking photo with current settings...");
                    CameraValue tv = TvValues.GetValue(MainCamera.GetInt32Setting(PropertyID.Tv));
                    if (tv == TvValues.Bulb) MainCamera.TakePhotoBulb(2);
                    else MainCamera.TakePhoto();
                    //WaitEvent.WaitOne();

                    if (!Error) Console.WriteLine("Photo taken and saved");
                }
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
            finally
            {
                //MainCamera?.Dispose();
                //APIHandler.Dispose();
                //Console.WriteLine("Good bye! (press any key to close)");
                //Console.ReadKey();
            }
        }

        //public bool OpenSession(Camera cam, Func<Camera, DownloadInfo, bool>? downloadReady = null)
        //{
        //    cam.DownloadReady += (Camera sender, DownloadInfo Info) => downloadReady?.Invoke(sender, Info);
        //    cam.OpenSession();

        //    Console.WriteLine($"Opened session with camera: {cam.DeviceName}");
        //    return true;
        //}

        private bool OpenSession()
        {
            Cameras = APIHandler.GetCameraList();
            if (Cameras.Count > 0)
            {
                MainCamera.DownloadReady += MainCamera_DownloadReady;
                MainCamera.OpenSession();
                Console.WriteLine($"Opened session with camera: {MainCamera.DeviceName}");
                return true;
            }
            else return false;
        }

        #region event handlers

        private void MainCamera_DownloadReady(Camera sender, DownloadInfo Info)
        {
            try
            {
                Console.WriteLine("Starting image download...");
                sender.DownloadFile(Info, ImageSaveDirectory);
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); Error = true; }
            finally {
                //WaitEvent.Set();
            }
        }

        private void APIHandler_CameraAdded(CanonAPI sender)
        {
            try
            {
                Console.WriteLine("Camera added event received");
                if (!OpenSession()) { Console.WriteLine("Sorry, something went wrong. No camera"); Error = true; }
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); Error = true; }
            finally {
                //WaitEvent.Set();
            }
        }

        #endregion

    }
}
