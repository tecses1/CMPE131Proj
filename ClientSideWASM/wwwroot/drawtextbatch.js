window.drawTextBatch = (canvasId, textData) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas || !textData) return;
    const ctx = canvas.getContext('2d');

    textData.forEach(t => {
        ctx.save();

        const x = t.x + (t.offX || 0);
        const y = t.y + (t.offY || 0);
        const maxWidth = t.sizeX * 0.9;  // 10% padding
        const maxHeight = t.sizeY * 0.8; // 20% padding
        

        // 1. Draw the Background Rect First
        if (t.fillColor) {
            ctx.globalAlpha = t.rectAlpha ?? 1.0;
            ctx.fillStyle = t.fillColor;
            ctx.fillRect(x - t.sizeX / 2, y - t.sizeY / 2, t.sizeX, t.sizeY);
        }
        ctx.globalAlpha = t.textAlpha ?? 1.0;
        // 2. DYNAMIC FONT SCALING
        let fontSize = t.sizeY; // Start big (at the height of the box)
        ctx.font = `${fontSize}px ${t.fontFamily || "Arial"}`;
        
        // Loop: Reduce font size until the width fits the box
        while (ctx.measureText(t.text).width > maxWidth && fontSize > 5) {
            fontSize--;
            ctx.font = `${fontSize}px ${t.fontFamily || "Arial"}`;
        }
        
        // Also ensure height doesn't exceed box height
        if (fontSize > maxHeight) {
            fontSize = maxHeight;
            ctx.font = `${fontSize}px ${t.fontFamily || "Arial"}`;
        }

        // 3. Draw the Text (Now perfectly sized)
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";

        if (t.borderWidth > 0) {
            ctx.strokeStyle = t.borderColor;
            ctx.lineWidth = t.borderWidth;
            ctx.strokeText(t.text, x, y);
        }

        ctx.fillStyle = t.fontColor;
        ctx.fillText(t.text, x, y);

        ctx.restore();
    });
};