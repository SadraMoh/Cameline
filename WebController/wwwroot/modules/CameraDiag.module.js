export function showPrompt(message) {
    return prompt(message, 'Type anything here');
}

export async function receiveLiveImage(cameraId) {
    /** @type { HTMLImageElement } */
    const imgLive = document.querySelector('#imgLive');

    const blob = await (await fetch('https://localhost:7292/api/LiveView/Live?cameraId=' + cameraId)).blob();

    console.log('[blob.size]', blob.size);
    if (blob.size < 100) return;

    const imageObjectURL = URL.createObjectURL(blob);

    imgLive.src = imageObjectURL;
}

var connection = undefined;

/** 
 * @typedef {object} Subscription
 * @property {number} cameraId
 * @property {string} token
 * */

/** @type {Subscription} */
var sub = undefined;

export function signalinit() {

    connection = new signalR.HubConnectionBuilder().withUrl("/liveHub").build();

    connection.on("ReceiveMessage", (message) => {
        console.log("[signalr:ReceiveMessage]", { message });
    });

    connection.on("LiveUpdate", (cameraId, token) => {
        console.log(`[signalr:LiveUpdate]`, { cameraId, token});

        if (sub.cameraId == cameraId)
            receiveLiveImage(cameraId);
    });

    connection.on("SubscriptionSuccess", (token, cameraId) => {
        console.log(`[signalr:SubscriptionSuccess]`, { cameraId, token });
        sub = {
            cameraId,
            token
        }
    });

    connection.start()
        .then(() => {
            connection.invoke("SendMessage", "PING");
        })

}

export function ping() {

    connection.invoke("SendMessage", "PING");

}

export function subscribe(cameraId) {

    connection.invoke("Subscribe", cameraId);

}