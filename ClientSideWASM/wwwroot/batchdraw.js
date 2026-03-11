let globalImageCache = []; 

// 2. This function receives the images from Blazor ONCE
window.initializeCache = (imgArray) => {
    globalImageCache = imgArray;
    console.log("Image Cache Initialized with " + globalImageCache.length + " frames.");
};

window.batchDrawMulti = (canvasId, data, count) => {
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d');

    for (let i = 0; i < count * 6; i += 6) {
        const x = data[i];
        const y = data[i+1];
        const w = data[i+2];
        const h = data[i+3];
        const rad = data[i+4];
        const imgIndex = data[i+5]; // The magic index
        
        if (imgIndex != -1){ // if an image exists, we can display it.
            const img = globalImageCache[imgIndex];
            if (rad == 0){
                ctx.drawImage(img, x - w/2, y - h/2, w, h);
                continue;
            }
            ctx.save();
            ctx.translate(x, y);
            ctx.rotate(rad);
            ctx.drawImage(img, -w/2, -h/2, w, h);
            ctx.restore();
        }
        

    }
};
/*
window.batchDrawMulti = (canvasId, data, count) => {
    // Ensure data is treated as a Float32Array for speed
    const view = new Float32Array(data.buffer, data.byteOffset, data.length);
    
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d'); // Optimization: disable alpha on the context if not needed

    for (let i = 0; i < count * 6; i += 6) {
        const x = view[i];
        const y = view[i+1];
        const w = view[i+2];
        const h = view[i+3];
        const rad = view[i+4];
        const imgIndex = view[i+5];
        if (imgIndex == -1) {console.log("Warning, missing image!"); continue;} // Skip if no image to draw
        const img = globalImageCache[imgIndex];
        if (img) {
            if (rad !== 0){
            ctx.save();
            ctx.translate(x, y);
            ctx.rotate(rad);
            ctx.drawImage(img, -w/2, -h/2, w, h);
            ctx.restore();
            } else {
                ctx.drawImage(img, x - w/2, y - h/2, w, h);
            }
        }
    }
};*/