window.dashboardDnD = {
    init: function (dotNetHelper) {
        const container = document.getElementById('dashboard-cards');
        if (!container) return;

        let dragSrcIndex = null;

        container.querySelectorAll('.dashboard-card[draggable="true"]').forEach((card, idx) => {
            card.addEventListener('dragstart', function (e) {
                dragSrcIndex = parseInt(card.getAttribute('data-index'));
                card.classList.add('dragging');
            });

            card.addEventListener('dragend', function (e) {
                card.classList.remove('dragging');
                container.querySelectorAll('.dashboard-card').forEach(c => c.classList.remove('dragover'));
            });

            card.addEventListener('dragover', function (e) {
                e.preventDefault();
            });

            card.addEventListener('dragenter', function (e) {
                if (parseInt(card.getAttribute('data-index')) !== dragSrcIndex) {
                    card.classList.add('dragover');
                }
            });

            card.addEventListener('dragleave', function (e) {
                card.classList.remove('dragover');
            });

            card.addEventListener('drop', function (e) {
                e.preventDefault();
                const dropIndex = parseInt(card.getAttribute('data-index'));
                if (dragSrcIndex !== null && dragSrcIndex !== dropIndex) {
                    // Swap DOM elements visually
                    const cards = Array.from(container.querySelectorAll('.dashboard-card[draggable="true"]'));
                    if (cards[dragSrcIndex] && cards[dropIndex]) {
                        if (dragSrcIndex < dropIndex) {
                            container.insertBefore(cards[dragSrcIndex], cards[dropIndex].nextSibling);
                        } else {
                            container.insertBefore(cards[dragSrcIndex], cards[dropIndex]);
                        }
                    }
                    // Notify Blazor of new order
                    const newOrder = Array.from(container.querySelectorAll('.dashboard-card[draggable="true"]'))
                        .map(c => parseInt(c.getAttribute('data-index')));
                    dotNetHelper.invokeMethodAsync('UpdateCardOrder', newOrder);
                }
                dragSrcIndex = null;
                container.querySelectorAll('.dashboard-card').forEach(c => c.classList.remove('dragover'));
            });
        });
    }
};