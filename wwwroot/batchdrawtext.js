window.drawTextBatch = (canvasId, textData) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    textData.forEach(t => {
        ctx.font = t.font;
        ctx.textAlign = "center"; 
        ctx.textBaseline = "middle";

        const x = t.x + t.offX;
        const y = t.y + t.offY;

        if (t.fillColor) {
            ctx.fillStyle = t.fillColor;
            ctx.fillRect(t.x - t.sizeX / 2, t.y - t.sizeY / 2, t.sizeX,t.sizeY);
        }
        // 1. Draw the Border (Stroke)
        if (t.borderWidth > 0) {
            ctx.strokeStyle = t.borderColor;
            ctx.lineWidth = t.borderWidth;
            ctx.lineJoin = "round"; // Prevents sharp spikes on thick fonts
            ctx.strokeText(t.text, x, y);
        }

        // 2. Draw the Main Text (Fill)
        ctx.fillStyle = t.fontColor;
        ctx.fillText(t.text, x, y);
    });
};