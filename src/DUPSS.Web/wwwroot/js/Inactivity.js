let inactivityTimer;
const timeoutMinutes = 20; // Inactivity duration before logout

function resetInactivityTimer(dotnetHelper) {
    clearTimeout(inactivityTimer);
    inactivityTimer = setTimeout(() => {
        dotnetHelper.invokeMethodAsync('OnUserInactive');
    }, timeoutMinutes * 60 * 1000);
}

window.monitorUserActivity = (dotnetHelper) => {
    const events = ['mousemove', 'keydown', 'click', 'scroll'];
    events.forEach(event =>
        document.addEventListener(event, () => resetInactivityTimer(dotnetHelper))
    );
    resetInactivityTimer(dotnetHelper);
};