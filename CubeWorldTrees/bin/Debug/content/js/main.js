var width = 400;
var cid = "canvas-map";
var canvasMinX, canvasMinY;
var blocks = new Array();
var matrix = new Array();
var offsetX = 0;
var offsetY = 0;
var mouseState = new Object();

function Update()
{
	var currentTime = new Date();
	var h = currentTime.getHours();
	var m = currentTime.getMinutes();
	var s = currentTime.getSeconds();
	var ms = currentTime.getMilliseconds();
	var context = document.getElementById(cid).getContext('2d');
	
	$("#debug").text("(Debug) Last request: " + h + ":" + m + ":" + s + "." + ms);	
	context.fillStyle = "rgba(255, 255, 255, 1)";
	context.fillRect(0, 0, width, width);
	
	for(var i = 0; i < matrix.length;i ++) {
		var x = matrix[i].x * 50 + offsetX;
		var y = matrix[i].y * 50 + offsetY;
		context.drawImage(blocks[matrix[i].src], x, y);
	}
}

function mouseMove(event) {
	if (mouseState.pressed) {
		offsetX = event.pageX - mouseState.x;
		offsetY = event.pageY - mouseState.y;
		Update();	
	}
}

$(document).ready(function () {
	$("body").append("<h2 id='debug'></h2>");
	$("body").append("<canvas id='" + cid + "' width='" + width + "' height='" + width + "'></canvas>");
	console.log($("#" + cid).offset());
	canvasMinX = $("#" + cid).offset().left;
	canvasMinY = $("#" + cid).offset().top;
	var context = document.getElementById(cid);
	
	$(document).mousedown(function (event) {
		mouseState.x = event.pageX - offsetX;
		mouseState.y = event.pageY - offsetY;
		mouseState.pressed = true;
	});
	
	$(document).mouseup(function (event) {
		mouseState.pressed = false;
	});
	
	context.addEventListener("mousemove", mouseMove, false);
	
	Update();

	setInterval(function () {
		$.ajax({
			url: "/initialize",
			dataType: 'json',
			success: (function (data, textStatus, jqXHR) {
        		console.log(data);
        		var needToUpdate = false;
				$.each(data, function() {
					var src = "/content/images/" + this.value;
					var match = -1;
					
					for(var i = 0;i < blocks.length;i ++) {
						if (blocks[i].getAttribute("_src") == src) {
							match = i;
							break;
						}
					}
					
					if (match == -1) {
						mapImage = new Image();
						mapImage.src = src;
						mapImage.setAttribute("_src", src);
						mapImage.onload = (function () {
							mapImage.setAttribute("loaded", true);
							Update();
						});
						match = blocks.length;
						blocks[match] = mapImage;
					}
					
					mIndex = -1;
					for(var i = 0;i < matrix.length;i ++) {
						if (matrix[i].x == this.x && matrix[i].y == this.y) {
							mIndex = i;
							break;
						}
					}
					
					var index = matrix.length;
					if (mIndex > -1) {
						index = mIndex;
						if (matrix[index].src != match)
							needToUpdate = true;
					} else {
						matrix[index] = new Object();
						matrix[index].x = this.x;
						matrix[index].y = this.y;
					}
					
					matrix[index].src = match;
			    });
			    
			    if (needToUpdate)
			    	Update();
			}),
			error: (function () {
				console.log("500: Server did not response!");
			})
		});
	}, 500);	
});

