const apiRoute = "http://localhost:3000/hands";
const levelResultsRoute = "http://localhost:3000/data/level-results";
const video = document.getElementById("video");
const canvas = document.getElementById("canvas");
const ctx = canvas.getContext("2d");
const counters = document.getElementById("counter-container");
const config = document.getElementById("level-config");
const beginningTime = Date.now();

const nextLevelRoute = config.dataset.nextLevel;
const expectedTotal = Number(config.dataset.total);
const expectedLeft = Number(config.dataset.left);
const expectedRight = Number(config.dataset.right);
let tries = Number(config.dataset.tries);
let completed = config.dataset.completed === "true";
const isGuest = config.dataset.isGuest === "true"   ;
const sessionId = Number(config.dataset.sessionId);
const token = config.dataset.token;

const countersArr = [];
Array.from(counters.children).forEach(child => {
    countersArr.push(child);
});

let lastResult = null;
let resultStartTime = null;
let actionTriggered = false;

const leftContainer = document.getElementById("left-hand-container");
Array.from(leftContainer.children).forEach(child => {
    child.setAttribute('data-bs-theme', 'dark');
});
const rightContainer = document.getElementById("right-hand-container");
Array.from(rightContainer.children).forEach(child => {
    child.setAttribute('data-bs-theme', 'dark');
});

canvas.width = 320;
canvas.height = 240;

let processing = false;
let cameraReady = false;

async function turnOnCamera() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({
            video: {
                width: 640,
                height: 480
            },
            audio: false
        });

        video.srcObject = stream;
        video.onloadedmetadata = async () => {
            await video.play();
            cameraReady = true;
            startFramesCapture();
        };
    } catch (err) {
        console.error("Camera error:", err);
    }
}

function startFramesCapture() {
    setInterval(async () => {
        if (!cameraReady) return;
        if (processing) return;
        processing = true;
        
        try {
            await captureFrame();
        } catch (err) {
            console.error("Frame capture error:", err);
        } finally {
            processing = false;
        }
    }, 250);
}

async function captureFrame() {
    ctx.drawImage(
        video, 0, 0, canvas.width, canvas.height
    );

    const blob = await new Promise(resolve => {
        canvas.toBlob(
            resolve, "image/jpeg", 0.8
        );
    });

    if (!blob) return;

    const formData = new FormData();

    formData.append(
        "image", blob, "frame.jpg"
    );

    const response = await fetch(apiRoute, {
        method: "POST",
        body: formData
    });

    if (!response.ok) {
        console.error("Backend error");
        return;
    }

    const data = await response.json();

    await validateResult(data);

    updateHands(data);
}

async function validateResult(data) {
    if (data.total === expectedTotal) {

        if (data.left === expectedLeft && data.right === expectedRight) {

            if (!resultStartTime) {
                resultStartTime = Date.now();
            }

            const elapsed = Date.now() - resultStartTime;

            if (elapsed >= 1000) {
                tries++;
                countersArr[0].classList.add("active-counter");
            }

            if (elapsed >= 2000) {
                countersArr[1].classList.add("active-counter");
            }

            if (elapsed >= 3000 && !actionTriggered) {
                actionTriggered = true;
                countersArr[2].classList.add("active-counter");
                completed = true;
                if (!isGuest) {
                    await createLevelResult();
                }

                window.location.href = `/Levels/LevelComplete?nextLevel=${encodeURIComponent(nextLevelRoute)}`;
            }

        } else {
            lastResult = data.total;
            resultStartTime = Date.now();
            actionTriggered = false;
            cleanCounters();
        }
    } else {
        cleanCounters();
    }
}

function cleanCounters() {
    countersArr.forEach(child => {
        child.classList.remove("active-counter");
    });
}

function updateHands(data) {
    let leftFingers = [];
    let rightFingers = [];
    Array.from(leftContainer.children).forEach(child => {
        child.classList.remove("active-finger");
        leftFingers.push(child);
    });
    Array.from(rightContainer.children).forEach(child => {
        child.classList.remove("active-finger");
        rightFingers.push(child);
    });

    for (let i = 0; i < data.left; i++) {
        if (leftFingers[i]) {
            leftFingers[i].classList.add("active-finger");
        }
    }

    for (let i = 0; i < data.right; i++) {
        if (rightFingers[i]) {
            rightFingers[i].classList.add("active-finger");
        }
    }
}

async function createLevelResult() {
    console.log("holaaa");
    const finishingTime = Date.now() - beginningTime;
    const dto = {
        idSession: sessionId,
        idLevel: 1,
        finishingTime: Math.floor(finishingTime),
        attempts: Number(tries),
        fails: Number(tries),
        completed: Boolean(completed)
    };

    const response = await fetch(levelResultsRoute, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`
        },
        body: JSON.stringify(dto)
    });

    const data = await response.json();
}

turnOnCamera();