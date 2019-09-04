const { Client } = require('@elastic/elasticsearch');
var client : typeof Client;
var ip : string;

function setStatusBar(text : string) {
    let statusText = document.getElementById("currentStatus");
    statusText.innerText = text;
}
function setConnectionStatus(isOnline : boolean) {
    let statusLabel = document.getElementById("clusterStatusLabel");
    if (isOnline) {
        statusLabel.innerText = "Online";
        statusLabel.style.color = "green";
    } else {
        statusLabel.innerText = "Offline";
        statusLabel.style.color = "gray";  
    }
}

document.getElementById("connectToClusterButton").addEventListener('click', async () => {
    (<HTMLInputElement>document.getElementById("connectToClusterButton")).disabled = true;
    setStatusBar("");
    ip = (<HTMLInputElement>document.getElementById("clusterIpInput")).value;
    var regexRes = /^(http|https):\/\/(([0-9]|[0-9][0-9\-]*[0-9])\.)*([0-9]|[0-9][0-9\-]*[0-9])(:[0-9]+)$/
    if (regexRes.test(ip)) {
        client = new Client({ node: ip, requestTimeout: 500 });
        await client.ping({ }, (e : Error) => {
            if (e) {
                setStatusBar("Error occurred during pinging elastic cluster");
                setConnectionStatus(false);
            } else {
                setStatusBar("Connected to elastic cluster");
                setConnectionStatus(true);
            }
            (<HTMLInputElement>document.getElementById("connectToClusterButton")).disabled = false;
        });
    } else {
        setStatusBar("Bad cluster ip pattern. It should be like : http://ip:port or https://ip:port");
        (<HTMLInputElement>document.getElementById("connectToClusterButton")).disabled = false;
    }
});