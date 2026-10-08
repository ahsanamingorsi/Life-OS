if ('serviceWorker' in navigator) {
    window.addEventListener('load', () => navigator.serviceWorker.register('/sw.js', { updateViaCache: 'none' }).catch(error => console.warn('Life OS offline support could not start.', error)));
}
let installPrompt;
const installButton = document.getElementById('installApp');
window.addEventListener('beforeinstallprompt', event => {
    event.preventDefault();
    installPrompt = event;
    if (!matchMedia('(display-mode: standalone)').matches) installButton.hidden = false;
});
installButton?.addEventListener('click', async () => {
    if (!installPrompt) return;
    await installPrompt.prompt();
    await installPrompt.userChoice;
    installPrompt = null;
    installButton.hidden = true;
});
window.addEventListener('appinstalled', () => { installButton.hidden = true; installPrompt = null; });
