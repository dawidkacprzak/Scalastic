const { Client, Connection } = require('@elastic/elasticsearch');
const elsticCluster = require('./subscripts/ElasticCluster')
const HTMLElements = require('./subscripts/HTMLElements');
var client;
var clusterDataSectionInterval;

//#region preconfig
hideAllSections();
document.querySelectorAll("#menu div").forEach((e)=>{
    e.addEventListener('click',()=>{
        if(ip!==undefined && ip!==null){    
            hideAllSections();
            uselectAllMenuButtons();
            e.classList.add('selectedMenu');
            document.querySelectorAll("#"+e.dataset.section).forEach((e)=>{
                e.style.display = "block";
            })
        }   
    })
})
//#endregion
//#region helperMethods
function hideAllSections(){
    document.querySelectorAll("section").forEach((e)=>{
        e.style.display = "none"
    })
}

function uselectAllMenuButtons(){
    document.querySelectorAll("#menu div").forEach((e)=>{
        e.classList.remove('selectedMenu');
    })
}

function setStatusBar(text) {
    let statusText = document.getElementById("currentStatus");
    statusText.innerText = text;
}

function setConnectionStatus(isOnline) {
    let statusLabel = document.getElementById("clusterStatusLabel");
    if (isOnline) {
        statusLabel.innerText = "Online";
        statusLabel.style.color = "green";
        document.querySelector("#clusterData").style.display = "block";
    } else {
        statusLabel.innerText = "Offline";
        statusLabel.style.color = "gray";  
    }
}
//#endregion
//#region handlers
document.getElementById("connectToClusterButton").addEventListener('click', async () => {
    document.getElementById("connectToClusterButton").disabled = true;
    clearInterval(clusterDataSectionInterval);
    setStatusBar("");
    let input_ip = document.getElementById("clusterIpInput").value;
    var regexRes = /^(http|https):\/\/(([0-9]|[0-9][0-9\-]*[0-9])\.)*([0-9]|[0-9][0-9\-]*[0-9])(:[0-9]+)$/
    if (regexRes.test(input_ip)) {
        client = new Client({ node: input_ip, requestTimeout: 500 });
        await client.ping({ }, (e) => {
            if (e) {
                setStatusBar("Error occurred during pinging elastic cluster");
                elsticCluster.setClusterAsDisconnected();
            } else {
                setStatusBar("Connected to elastic cluster");
                elsticCluster.setClusterAsConnected(input_ip);
            }
            document.getElementById("connectToClusterButton").disabled = false;
        });
    } else {
        setStatusBar("Bad cluster ip pattern. It should be like : http://ip:port or https://ip:port");
        document.getElementById("connectToClusterButton").disabled = false;
    } 
});

document.querySelector("#tooltipCloseSpan").addEventListener('click', (e) => {
    document.querySelector("#tooltip").style.display = "none";
})
//#endregion 
