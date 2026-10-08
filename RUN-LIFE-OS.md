# Life OS desktop window and PWA

Double-click the Life OS desktop shortcut or `Start Life OS.cmd`. The launcher starts the local server and opens a separate Edge app window, with no browser tabs or address bar. Chrome is used if Edge is unavailable.

## Installable PWA

Life OS includes a standalone web app manifest, 192/512 PNG icons, a service worker, and a reconnect page. To register it as an installed Windows PWA, open http://127.0.0.1:5199 in Edge once and choose **Install app** when shown, or Edge's **Apps → Install this site as an app**. This allows Windows Start/taskbar integration. Browser installation is separate from the existing launcher’s app-window mode.

Use the original Life OS desktop launcher after a restart: it starts the local server before opening the window. An installed PWA shortcut alone does not start the SQL-backed local server.

- `Stop Life OS.cmd` stops the background server. Closing the app window leaves the server running.
- `Update Life OS.cmd` publishes source changes; close the app window before updating.

Keep `DesktopApp` and `launcher` beside the launchers. This computer's .NET 10 runtime and SQL Express / LifeOSDb are required. Visual Studio is not required to run the published application.

Personal pages, financial data, and form tokens are never stored by the service worker. If the local server is stopped, the app displays a reconnect page; records remain in SQL Server. The app listens only at http://127.0.0.1:5199 and does not start automatically with Windows.
