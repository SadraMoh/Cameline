export function showPrompt(message) {
    return prompt(message, 'Type anything here');
}

export async function receiveLiveImage() {
    /** @type { HTMLImageElement } */
    const imgLive = document.querySelector('#imgLive');

    const blob = await (await fetch('https://localhost:7292/api/LiveView/Live')).blob();

    const imageObjectURL = URL.createObjectURL(blob);

    imgLive.src = imageObjectURL
}