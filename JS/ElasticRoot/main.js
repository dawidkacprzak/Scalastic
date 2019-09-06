const { app, BrowserWindow } = require('electron');
let mainWindow;

function createWindow() {
    mainWindow = new BrowserWindow({
        width: 800,
        height: 600,
        webPreferences: {
            nodeIntegration: true,
            devTools: true
        }
    });
    mainWindow.webContents.openDevTools();
    mainWindow.removeMenu();
    mainWindow.on('closed', () => {
        mainWindow = null;
    });

    mainWindow.loadFile("src/MainPage.html");
}

app.on('ready', () => {
    createWindow();
})

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin')
        app.quit();
})

app.on('activate', () => {
    if (mainWindow === null) {
        createWindow();
    }
})

