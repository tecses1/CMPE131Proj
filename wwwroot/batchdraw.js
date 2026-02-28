window.batchDrawMulti = (canvasId, imgArray, data) => {
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d');

    for (let i = 0; i < data.length; i += 6) {
        const x = data[i];
        const y = data[i+1];
        const w = data[i+2];
        const h = data[i+3];
        const rad = data[i+4];
        const imgIndex = data[i+5]; // The magic index

        const img = imgArray[imgIndex]; // Grab the specific image for this object
        console.log("Drawing image:", img, "image array number: ", imgIndex);
        ctx.save();
        ctx.translate(x, y);
        ctx.rotate(rad);


        // Then the original draw image

        ctx.drawImage(img, -w/2, -h/2, w, h);
        ctx.restore();
    }
};