const { Client, Connection } = require('@elastic/elasticsearch');
var client;
var ip;

function setStatusBar(text) {
    let statusText = document.getElementById("currentStatus");
    statusText.innerText = text;
}
function setConnectionStatus(isOnline) {
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
    document.getElementById("connectToClusterButton").disabled = true;
    setStatusBar("");
    ip = document.getElementById("clusterIpInput").value;
    var regexRes = /^(http|https):\/\/(([0-9]|[0-9][0-9\-]*[0-9])\.)*([0-9]|[0-9][0-9\-]*[0-9])(:[0-9]+)$/
    if (regexRes.test(ip)) {
        client = new Client({ node: ip, requestTimeout: 500 });
        await client.ping({ }, (e) => {
            if (e) {
                setStatusBar("Error occurred during pinging elastic cluster");
                setConnectionStatus(false);
            } else {
                setStatusBar("Connected to elastic cluster");
                setConnectionStatus(true);
            }
            document.getElementById("connectToClusterButton").disabled = false;
        });
    } else {
        setStatusBar("Bad cluster ip pattern. It should be like : http://ip:port or https://ip:port");
        document.getElementById("connectToClusterButton").disabled = false;
    }
});