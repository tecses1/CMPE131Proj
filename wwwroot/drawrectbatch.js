window.drawRectBatch = (canvasId, rectData) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas || !rectData) return;
    const ctx = canvas.getContext('2d');

    rectData.forEach(r => {
        ctx.save(); // Protect the canvas state
        ctx.globalAlpha = r.alpha ?? 1.0;
        
        // Center the rect
        const drawX = r.x - (r.sizeX / 2);
        const drawY = r.y - (r.sizeY / 2);

        // 1. Draw the Fill (The inside)
        if (r.fillColor) {
            ctx.fillStyle = r.fillColor;
            ctx.fillRect(drawX, drawY, r.sizeX, r.sizeY);
        }

        // 2. Draw the Border (The outline)
        if (r.borderWidth > 0) {
            ctx.strokeStyle = r.borderColor;
            ctx.lineWidth = r.borderWidth;
            ctx.lineJoin = "round";
            ctx.strokeRect(drawX, drawY, r.sizeX, r.sizeY);
        }

        ctx.restore(); // Reset for the next rectangle
    });
};