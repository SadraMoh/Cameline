using EOSDigital.API;
using Microsoft.AspNetCore.SignalR;
using WebController.Services;

namespace WebController.Hubs
{
    public class LiveHub : Hub
    {
        public readonly CameraService camSv;


        public LiveHub(CameraService camSv)
        {
            this.camSv = camSv;
        }

        public async Task SendMessage(string message)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", message);
        }

        public async Task Subscribe(long cameraId)
        {
            var camIndex = camSv.Cameras.FindIndex(cam => cam.ID == cameraId);
            if (camIndex == -1)
            {
                await Clients.Caller.SendAsync("ReceiveMessage", "Camera not found");
                return;
            }

            var subscriptionToken = Guid.NewGuid();

            camSv.Subscriptions.Add((camSv.Cameras[camIndex].ID, subscriptionToken));
            await Clients.Caller.SendAsync("SubscriptionSuccess", subscriptionToken, cameraId);
        }

    }
}