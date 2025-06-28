window.startRain = function (canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Set canvas size to match footer width
    function resizeCanvas() {
        canvas.width = canvas.parentElement.offsetWidth;
        canvas.height = 120; // Adjust height as needed
    }

    resizeCanvas();
    window.addEventListener('resize', resizeCanvas);

    const ctx = canvas.getContext('2d');
    const raindrops = [];
    const numDrops = Math.floor(canvas.width / 6);

    function createRaindrop() {
        return {
            x: Math.random() * canvas.width,
            y: Math.random() * -canvas.height,
            length: 10 + Math.random() * 15,
            speed: 2 + Math.random() * 4,
            opacity: 0.2 + Math.random() * 0.4
        };
    }

    for (let i = 0; i < numDrops; i++) {
        raindrops.push(createRaindrop());
    }

    function draw() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        ctx.strokeStyle = 'rgba(173,216,230,0.6)';
        ctx.lineWidth = 1.2;

        for (let drop of raindrops) {
            ctx.globalAlpha = drop.opacity;
            ctx.beginPath();
            ctx.moveTo(drop.x, drop.y);
            ctx.lineTo(drop.x, drop.y + drop.length);
            ctx.stroke();

            drop.y += drop.speed;
            if (drop.y > canvas.height) {
                Object.assign(drop, createRaindrop());
                drop.y = 0;
            }
        }
        ctx.globalAlpha = 1.0;
        requestAnimationFrame(draw);
    }

    draw();
};