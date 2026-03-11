const bitBuffer = new ArrayBuffer(4);
const floatView = new Float32Array(bitBuffer);
const intView = new Uint32Array(bitBuffer);
const colorConverterBuffer = new ArrayBuffer(4);
const colorFloatView = new Float32Array(colorConverterBuffer);
const colorUintView = new Uint32Array(colorConverterBuffer);

function unpackColor(floatVal) {
    colorFloatView[0] = floatVal;
    const packed = colorUintView[0];
    
    return {
        a: ((packed >>> 24) & 0xFF) / 255,
        r: (packed >> 16) & 0xFF,
        g: (packed >> 8) & 0xFF,
        b: packed & 0xFF
    };
}

window.combinedRender = (canvasId, bgColor, buffer, labels) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;
    const ctx = canvas.getContext('2d');

    //ensure canvas bounds.
    if (canvas.width !== canvas.clientWidth || canvas.height !== canvas.clientHeight) {
            canvas.width = canvas.clientWidth;
            canvas.height = canvas.clientHeight;
        }
    const cw = canvas.width;
    const ch = canvas.height;
    // 1. Clear
    ctx.fillStyle = bgColor;
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    // 2. Backgrounds & Sprites (The Buffer)
    let i = 0;
    while (i < buffer.length) {
        const type = buffer[i++];
        if (type === 0) { // SPRITE (Stride 6)
            const x = buffer[i++]/100, y = buffer[i++]/100, w = buffer[i++]/100, 
                  h = buffer[i++]/100, rad = buffer[i++]/100, idx = buffer[i++];
            if (x + w/2 < 0 || x - w/2 > cw || y + h/2 < 0 || y - h/2 > ch) continue; // Simple culling
            const img = globalImageCache[idx];
            if (img) {
                if (rad === 0) ctx.drawImage(img, x - w/2, y - h/2, w, h);
                else {
                    ctx.save(); ctx.translate(x, y); ctx.rotate(rad);
                    ctx.drawImage(img, -w/2, -h/2, w, h); ctx.restore();
                }
            }
        } else if (type === 1) { // RECT/TEXT-BOX (Stride 7)
            const x = buffer[i++]/100, y = buffer[i++]/100, w = buffer[i++]/100, h = buffer[i++]/100;
            if (x + w/2 < 0 || x - w/2 > cw || y + h/2 < 0 || y - h/2 > ch) continue;
            const fillP = buffer[i++], bordP = buffer[i++], bW = buffer[i++]/100;
            
            if (fillP !== 0) {
                const a = ((fillP >>> 24) & 0xFF) / 255;
                ctx.fillStyle = `rgba(${(fillP>>16)&0xFF}, ${(fillP>>8)&0xFF}, ${fillP&0xFF}, ${a})`;
                ctx.fillRect(x - w/2, y - h/2, w, h);
            }
            if (bW > 0 && bordP !== 0) {
                const a = ((bordP >>> 24) & 0xFF) / 255;
                ctx.strokeStyle = `rgba(${(bordP>>16)&0xFF}, ${(bordP>>8)&0xFF}, ${bordP&0xFF}, ${a})`;
                ctx.lineWidth = bW;
                ctx.strokeRect(x - w/2, y - h/2, w, h);
            }
        }
    }

    // 3. Text Labels (The JSON Array)
    labels.forEach(t => {
        ctx.save();

        // 1. Initial Styles
        ctx.globalAlpha = t.tAlp;
        ctx.fillStyle = t.tCol;
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";

        // 2. Initial Size Calculation
        // Using t.h for auto-scaling or t.fontSize as fallback
        let fontSize = t.fillToSize ? (t.h * 0.85) : (t.fontSize || 20);
        
        // 3. Set Font and Measure once
        // We use Math.floor because fractional font sizes can cause blurring
        ctx.font = `bold ${Math.floor(fontSize)}px ${t.fnt}`;
        
        const maxWidth = t.w * 0.95;
        const metrics = ctx.measureText(t.text);

        // 4. Single-Pass Scaling (The "Fast" Way)
        if (metrics.width > maxWidth && metrics.width > 0) {
            const ratio = maxWidth / metrics.width;
            fontSize *= ratio;
            ctx.font = `bold ${Math.floor(fontSize)}px ${t.fnt}`;
        }

        // 5. Draw
        ctx.fillText(t.text, t.x, t.y);
        ctx.restore();
    });
};