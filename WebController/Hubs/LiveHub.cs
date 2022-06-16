using EOSDigital.API;
using Microsoft.AspNetCore.SignalR;
using WebController.Services;

namespace WebController.Hubs
{
    public class LiveHub : Hub
    {
        public readonly CameraService camSv;

        readonly List<(Camera cam, IClientProxy subscriber)> Subscriptions = new List<(Camera cam, IClientProxy subscriber)>();

        public LiveHub(CameraService camSv)
        {
            this.camSv = camSv;

            this.camSv.Broadcast += CamSv_Broadcast;
        }

        private async void CamSv_Broadcast(Camera sender, Stream img)
        {
            var cameraSubs = Subscriptions.Where(sub => sub.cam.ID == sender.ID);

            var stream = img;

            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                bytes = memoryStream.ToArray();
            }

            string base64 = Convert.ToBase64String(bytes);
            //new MemoryStream(System.Text.Encoding.UTF8.GetBytes(base64));

            var tasks = new List<Task>();

            foreach (var sub in cameraSubs)
                tasks.Add(sub.subscriber.SendAsync(base64));

            Task.WaitAll(tasks.ToArray());

        }

        public async Task SendMessage(string message)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        public async Task Subscribe(long cameraId)
        {
            Camera? cam = camSv.Cameras.Find(cam => cam.ID == cameraId);
            if (cam == null)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "Camera not found");
                return;
            }

            var existingSubs = Subscriptions.FindIndex(sub => sub.cam.ID == cameraId && sub.subscriber == Clients.Caller);
            if (existingSubs >= 0)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", $"Subscription between Camera: {cam.ID} and Client already exists");
                return;
            }

            Subscriptions.Add((cam, Clients.Caller));
            await Clients.Caller.SendAsync("ReceiveMessage", $"Subscription added");
        }

    }
}