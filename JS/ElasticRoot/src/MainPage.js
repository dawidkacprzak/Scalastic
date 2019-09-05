const { Client, Connection } = require('@elastic/elasticsearch');
var client;
var ip;
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

//#region ClusterDataSectionTask
function runClusterDataSection(){
    if(ip!==null && ip!==undefined){
        refreshClusterData();
        clusterDataSectionInterval = setInterval(refreshClusterData,1000);
    }
}

function refreshClusterData(){

    client.cluster.health({}, (e, r) => {
        document.querySelector("#clusterDataClusterName").innerHTML = r.body.cluster_name;
        let status = r.body.status;
        let statusColor = document.querySelector("#clusterStatusColor");
        //#region setStatusColorAndMessage
        if(r.statusCode !== 200){
            statusColor.style.backgroundColor = "red";
            statusColor.innerHTML = "ERROR";
        }else{
        if(status==="green"){
            statusColor.style.backgroundColor = "green";
            statusColor.innerHTML = "OK";
        }else if(status==="yellow"){
            statusColor.style.backgroundColor = "orange";
            statusColor.innerHTML = "WARNING";
        }else{
            statusColor.style.backgroundColor = "red";
            statusColor.innerHTML = "ERROR";
        }
        //#endregion
        let countOfNodes = r.body.number_of_nodes;
        document.querySelector("#clusterDataNodes").innerHTML = null;
        for(let i = 0;i<countOfNodes;i++){
            let node = document.createElement("img");
            node.src = "img/node.png";
            node.classList.add("defaultImageSize");
            document.querySelector("#clusterDataNodes").appendChild(node);
        }
    }
        console.log(JSON.stringify(r));
    })

    client.cluster.stats({}, (e, r) => {
        console.log(JSON.stringify(r));
    })
}

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

function setClusterAsDisconnected(){
    setConnectionStatus(false);
    ip = null;
    hideAllSections();
    uselectAllMenuButtons();
    document.getElementById("connectToClusterButton").disabled = false;
    clearInterval(clusterDataSectionInterval);
}

function setClusterAsConnected(input_ip){
    uselectAllMenuButtons();
    setConnectionStatus(true);
    document.querySelector("#clusterData").style.display = "block";
    document.querySelector("#menu").children[0].classList.add('selectedMenu');
    ip = input_ip;
    document.getElementById("connectToClusterButton").disabled = false;
    runClusterDataSection();
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
                setClusterAsDisconnected();
            } else {
                setStatusBar("Connected to elastic cluster");
                setClusterAsConnected(input_ip);
            }
            document.getElementById("connectToClusterButton").disabled = false;
        });
    } else {
        setStatusBar("Bad cluster ip pattern. It should be like : http://ip:port or https://ip:port");
        document.getElementById("connectToClusterButton").disabled = false;
    }
    
});
//#endregion 
