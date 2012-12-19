function Update()
{
	var currentTime = new Date();
	var h = currentTime.getHours();
	var m = currentTime.getMinutes();
	var s = currentTime.getSeconds();
	var ms = currentTime.getMilliseconds();
	$("#debug").text("(Debug) Last request: " + h + ":" + m + ":" + s + "." + ms);
}

$(document).ready(function () {
	var width = 400;
	var cid = "canvas-map";
	
	
	$("body").append("<h2 id='debug'></h2>");
	$("body").append("<canvas id='" + cid + "' width='" + width + "' height='" + width + "'></canvas>");
	Update();
	
	var context = document.getElementById(cid).getContext('2d');
	
	setInterval(function () {
		$.ajax({
			url: "/initialize",
			dataType: 'json',
			success: (function (data, textStatus, jqXHR) {
				$.each(data, function() {
			        mapImage = new Image();
			        mapImage.src = "/content/images/" + this.value;
			        mapImage.setAttribute("_x", this.x);
			        mapImage.setAttribute("_y", this.y);
			        mapImage.onload = function () {
		        		console.log(this);
						context.drawImage(this, this.getAttribute("_x") * 50, this.getAttribute("_y") * 50);
			        }
			    });
			    Update();
			}),
			error: (function () {
				alert("Somethings wrong! :D");
			})
		});
	}, 500);	
});

