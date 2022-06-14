class Cropper {

    /** @type{HTMLElement} */
    boundry;

    /** @type {HTMLDivElement} */
    cropper;

    /** @type{number} */
    cropperX = 0;

    /** @type{number} */
    cropperY = 0;

    /** @type{number} */
    cropperWidth = 54;

    /** @type{number} */
    cropperHeight = 54;

    /**
     * @param {HTMLElement} element
     */
    constructor(element) {

        window.addEventListener('mouseup', (e) => this.windowMouseup(e));

        this.boundry ??= document.body;

        this.cropper = element;

    }

    windowMouseup(e) {

        // Stop resizing if user stopped holding mousedown anywhere in the page
        window.removeEventListener('mousemove', (e) => this.mouseMove(e));


        this.mouseX = null;
        this.mouseY = null;

        this.grabbedCorner = "none";
    }

    // #region Corners

    /** The corner that is currently grabbed */
    grabbedCorner = "none";

    /** @type {number} */
    mouseX;

    /** @type {number} */
    mouseY;

    /** @type {number} */
    calibrator = 4;

    mouseMove(e, sides = ["none"]) {

        // Initialize cached mouse lcoation
        this.mouseX ??= e.clientX;
        this.mouseY ??= e.clientY;

        // Get precise boundries and locations of items
        const el = this.cropper;
        const rect = el.getBoundingClientRect();
        const bound = this.boundry.getBoundingClientRect();

        // Restrict movement and scaling to the boundry
        if (e.clientX < bound.left) return;
        if (e.clientX > bound.right) return;
        if (e.clientY < bound.top) return;
        if (e.clientY > bound.bottom) return;

        // Calculate mouse disposition
        const deltaX = e.clientX - this.mouseX;
        const deltaY = e.clientY - this.mouseY;

        // Cache current mouse position for later calculation of disposition
        this.mouseX = e.clientX;
        this.mouseY = e.clientY;

        // Current left and top values
        // const cleft = Number.parseInt(el.style.left.substring(0, el.style.left.length - 2));
        // const ctop = Number.parseInt(el.style.top.substring(0, el.style.top.length - 2));
        // const cwidth = Number.parseInt(el.style.width.substring(0, el.style.width.length - 2));
        // const cheight = Number.parseInt(el.style.height.substring(0, el.style.height.length - 2));

        if (sides.includes("top")) {
            el.style.top = (rect.y + deltaY) - bound.y + 'px';
            el.style.height = rect.height - this.calibrator - deltaY + 'px';
        }

        if (sides.includes("right")) {
            el.style.width = rect.width - this.calibrator + deltaX + 'px';
        }

        if (sides.includes("bottom")) {
            el.style.height = rect.height - this.calibrator + deltaY + 'px';
        }

        if (sides.includes("left")) {
            el.style.left = (rect.x + deltaX) - bound.x + 'px';
            el.style.width = rect.width - this.calibrator - deltaX + 'px';
        }

        if (sides.includes("center")) {
            el.style.left = (rect.x + deltaX) - bound.x + 'px';
            el.style.top = (rect.y + deltaY) - bound.y + 'px';
        }

        // For depiction in the UI
        this.cropperWidth = rect.width;
        this.cropperHeight = rect.height;
    }

    tlDown() { this.sub2MouseMove(["top", "left"]); this.grabbedCorner = "topLeft" }

    trDown() { this.sub2MouseMove(["top", "right"]); this.grabbedCorner = "topRight" }

    blDown() { this.sub2MouseMove(["bottom", "left"]); this.grabbedCorner = "bottomLeft" }

    brDown() { this.sub2MouseMove(["bottom", "right"]); this.grabbedCorner = "bottomRight" }

    //-

    topDown() { this.sub2MouseMove(["top"]); this.grabbedCorner = "top" }

    rightDown() { this.sub2MouseMove(["right"]); this.grabbedCorner = "right" }

    bottomDown() { this.sub2MouseMove(["bottom"]); this.grabbedCorner = "bottom" }

    leftDown() { this.sub2MouseMove(["left"]); this.grabbedCorner = "left" }

    //-

    centerDown() { this.sub2MouseMove(["center"]); this.grabbedCorner = "center"; }

    sub2MouseMove(sides = ["none"]) {
        window.addEventListener('mousemove', (evt) => this.mouseMove(evt, sides));
    }

    // #endregion Corners

}

const CROPPER_SELECTOR = ".cropper";
const CROPPER_KEY_ATTR = "cropper-id";

/** @type { Cropper[] } */
let croppers = [];

document.addEventListener('DOMContentLoaded', () => {

    const cropperElements = document.querySelectorAll(CROPPER_SELECTOR);

    let i = 0;
    for (const el of cropperElements) {

        el.setAttribute(CROPPER_KEY_ATTR, i++);

        const instance = new Cropper(el);
        croppers = [...croppers, instance];

        // bind events

        el.querySelector('.cropper-info').addEventListener('mousemove', instance.centerDown);

        el.querySelector('.center').addEventListener('mousemove', instance.centerDown);

    }

});

/**
 * cropper surrogate
 * @param {HTMLElement} element
 * @param {string} func
 * @param {any} input
 */
function cropper(element, func, input) {

    /** @type {HTMLElement} */
    let caller = element;

    let p = caller;
    while (!p.classList.contains("cropper")) {
        p = p.parentElement;
    }

    const cropper = croppers.filter(cropper => cropper.cropper.id == (p))[0];

    cropper[func]?.(input);

}