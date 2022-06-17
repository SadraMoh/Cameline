let connection = undefined;

let sub = undefined;

export function signalinit() {

    connection = new signalR.HubConnectionBuilder().withUrl("/liveHub").build();

    connection.start()
        .then(() => {
            connection.invoke("SendMessage", "PING");
        })

    // Receive any message
    connection.on("ReceiveMessage", (message) => {
        console.log("[signalr:ReceiveMessage]", { message });
    });

    // New image is available
    connection.on("LiveUpdate", (cameraId, token) => {
        console.log(`[signalr:LiveUpdate]`, { cameraId, token });

        if (sub.cameraId == cameraId)
            receiveLiveImage(cameraId);
    });

    // Subscription accepted
    connection.on("SubscriptionSuccess", (token, cameraId) => {
        console.log(`[signalr:SubscriptionSuccess]`, { cameraId, token });
        debugger
        sub = {
            cameraId,
            token
        }
    });

}

export function subscribeToLiveView(cameraId) {
    connection.invoke("Subscribe", cameraId);
}

export function unsubscribeFromLiveView() {
    connection.invoke("Unsubscribe", sub.token);
}

export async function receiveLiveImage(cameraId) {

    /** @type { HTMLImageElement } */
    const imgLive = document.querySelector('#imgLive');

    try {
        const blob = await (await fetch('/api/LiveView/Live?cameraId=' + cameraId)).blob();

        console.log('[blob.size]', blob.size);
        if (blob.size < 100) return;

        const imageObjectURL = URL.createObjectURL(blob);

        imgLive.src = imageObjectURL;
    }
    catch { }
}