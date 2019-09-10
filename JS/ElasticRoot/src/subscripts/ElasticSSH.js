module.exports = {
    assignConfigurationNodesInContainer : (array_nodes) => assignConfigurationNodesInContainer(array_nodes)
}

function assignConfigurationNodesInContainer(array_nodes) {
    for (let i = 0; i < array_nodes.length; i++) {
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
        setNodeOnClickHandler(node, array_nodes[i]);
        document.querySelector("#configurationNodes").append(node);
    }
}

function setNodeOnClickHandler(node,data) {  
    node.addEventListener('click', () => {
        elasticCluster.showToolTip(
            "Nazwa serwera: " + data.name +
            "<br>Adres serwera: " + data.transport_address +
            "<br>PID: " + data.jvm.pid +
            "<br>OS: " + data.os.pretty_name +
            "<br>Typ: " + data.roles,
            node);
    });
}