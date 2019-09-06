module.ip;
module.exports = {
    setClusterAsDisconnected : () => {
        setConnectionStatus(false);
        ip = null;
        hideAllSections();
        uselectAllMenuButtons();
        document.getElementById("connectToClusterButton").disabled = false;
        clearInterval(clusterDataSectionInterval);
    },
    setClusterAsConnected : (input_ip) => {
        uselectAllMenuButtons();
        setConnectionStatus(true);
        document.querySelector("#clusterData").style.display = "block";
        document.querySelector("#menu").children[0].classList.add('selectedMenu');
        ip = input_ip;
        document.getElementById("connectToClusterButton").disabled = false;
        runClusterDataSection();
    },
    showToolTip :(content, element) => {
        let tooltip = document.querySelector("#tooltip");
        tooltip.style.display = "flex";
        let clickedElementTopPosition = element.getBoundingClientRect().top
        let clickedElementHeight = element.clientHeight;
        let clickedElementWidth = element.clientWidth;
        let clickedElementLeftPosition = element.getBoundingClientRect().left;
        tooltip.style.top = clickedElementTopPosition+clickedElementHeight/2 + "px";
        tooltip.style.left = clickedElementLeftPosition-clickedElementWidth/2 + "px";
        document.querySelector("#tooltipContent").innerHTML = content;
    }
}

let statusColor = document.querySelector("#clusterStatusColor");

function runClusterDataSection(){
    if(ip!==null && ip!==undefined){
        refreshClusterData();
        clusterDataSectionInterval = setInterval(refreshClusterData,4000);
    }
}

function refreshClusterData(){
    client.cluster.health({}, (e, r) => {
        if (e) {
            console.log("<IMPLEMENT CLUSTER REQUEST ERROR HERE>")
            statusColor.style.backgroundColor = "gray";
            statusColor.innerHTML = "RIP?";
        } else {
            let responseBody = r.body;
            document.querySelector("#clusterDataClusterName").innerHTML = responseBody.cluster_name;
            let status = responseBody.status;
            if (r.statusCode !== 200) {
                statusColor.style.backgroundColor = "red";
                statusColor.innerHTML = "ERROR";
            } else {
                if (status === "green") {
                    statusColor.style.backgroundColor = "green";
                    statusColor.innerHTML = "OK";
                } else if (status === "yellow") {
                    statusColor.style.backgroundColor = "orange";
                    statusColor.innerHTML = "WARNING";
                } else {
                    statusColor.style.backgroundColor = "red";
                    statusColor.innerHTML = "ERROR";
                }
                document.querySelector("#clusterDataElasticLogo").addEventListener('click', (e) => {
                    let uwagi = responseBody.unassigned_shards != - "0" ?
                        "<br><hr/>Uwagi:<br>Nieprzypisane shardy: " + responseBody.unassigned_shards : "";
                    elsticCluster.showToolTip(
                        "Nazwa klastra: " + responseBody.cluster_name +
                        "<br>Liczba serwerów: " + responseBody.number_of_nodes +
                        "<br>Liczba serwerów(dane): " + responseBody.number_of_data_nodes +
                        "<br><hr/>Aktywne:" +
                        "<br>Główne shardy: " + responseBody.active_primary_shards +
                        "<br>Wszystkie shardy: " + responseBody.active_shards +
                        + uwagi,
                        e.srcElement
                    )
                })
                setNodeSection();
            }
        }
    })

    function compareNodes(a, b) {
        return a - b;
    }
    function setNodeSection() {
        client.nodes.info({}, (e, r) => {
            document.querySelector("#clusterDataNodes").innerHTML = null;

            let nodes = r.body.nodes;
            var array_nodes = Object.values(nodes);
            //console.log(JSON.stringify(array_nodes));
            array_nodes.sort(function(a, b) {
                var nameA = a.transport_address.toUpperCase(); // ignore upper and lowercase
                var nameB = b.transport_address.toUpperCase(); // ignore upper and lowercase
                if (nameA < nameB) {
                  return -1;
                }
                if (nameA > nameB) {
                  return 1;
                }
              
                // names must be equal
                return 0;
              });
            for (let i = 0; i < array_nodes.length; i++){
                console.log(array_nodes[i])
                let node = document.createElement("div");
                node.innerHTML = HTMLElements.node;
                node.style.display = "flex";
                node.style.flexDirection = "column";
                node.style.alignItems = "center";
                node.style.textAlign = "center";
                node.style.justifyContent = "center"; 
                node.style.padding = "5px";
                node.querySelector("img").src = 'img/node.png';
                let paragraphs = node.querySelectorAll("p");
                paragraphs.forEach((e) => e.style.margin = "0px")
                if (array_nodes[i].roles.includes("master")) {
                    var crown = document.createElement("p");
                    crown.style.padding = "0px";
                    crown.style.height = "25px";
                    crown.style.margin = "0px";
                    crown.innerHTML = "👑";
                    node.prepend(crown)
                } else {
                    var emptyP = document.createElement("p");
                    emptyP.style.height = "25px";
                    emptyP.style.padding = "0px";
                    emptyP.style.margin = "0px";
                    node.prepend(emptyP)
                }
                paragraphs[0].innerText = array_nodes[i].name;
                paragraphs[1].innerText = array_nodes[i].ip;
                document.querySelector("#clusterDataNodes").append(node)
            }
        })
    }
}