const { Client, Connection } = require('@elastic/elasticsearch');
var client;
var ip;

function setStatusBar(text) {
    let statusText = document.getElementById("currentStatus");
    statusText.innerText = text;
}

document.getElementById("connectToClusterButton").addEventListener('click', async () => {
    ip = document.getElementById("clusterIpInput").value;
    var regexRes = /^(http|https):\/\/(([0-9]|[0-9][0-9\-]*[0-9])\.)*([0-9]|[0-9][0-9\-]*[0-9])(:[0-9]+)$/
    if (regexRes.test(ip)) {
        setStatusBar("Pinging: " + ip)
        client = new Client({ node: ip, requestTimeout: 3000 });
        await client.ping({ requestTimeout: 3 }, (e) => {
            if (e) {
                setStatusBar("Error occurred during pinging elastic cluster");
            } else {
                setStatusBar("Connected to elastic cluster");
            }
        });
    } else {
        setStatusBar("Bad cluster ip pattern. It should be like : http://ip:port or https://ip:port");
    }
});