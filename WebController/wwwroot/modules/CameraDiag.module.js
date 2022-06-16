export function showPrompt(message) {
    return prompt(message, 'Type anything here');
}

export async function receiveLiveImage() {
    /** @type { HTMLImageElement } */
    const imgLive = document.querySelector('#imgLive');

    const blob = await (await fetch('https://localhost:7292/api/LiveView/Live')).blob();

    console.log('[blob.size]', blob.size);
    if (blob.size < 100) return;

    const imageObjectURL = URL.createObjectURL(blob);

    imgLive.src = imageObjectURL;
}

var connection = undefined;

export function signalinit() {

    connection = new signalR.HubConnectionBuilder().withUrl("/liveHub").build();

    connection.on("ReceiveMessage", (message) => {
        console.log("[signalr:ReceiveMessage]", { message });
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