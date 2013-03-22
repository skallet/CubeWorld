var width = 400;
var cid = "canvas-map";
var canvasMinX, canvasMinY;
var blocks = new Array();
var matrix = new Array();
var offsetX = 0;
var offsetY = 0;
var positionX = 0;
var positionY = 0;
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
	$("#debug").append("<br />(Position) X: " + positionX + ", Y: " + positionY);
	context.fillStyle = "rgba(255, 255, 255, 1)";
	context.fillRect(0, 0, width, width);
	
	var playerX = (4 - Math.round(offsetX /50));
	if (playerX < 0) {
		playerX = 0;
	}
	
	var playerY = (4 - Math.round(offsetY /50));
	if (playerY < 0) {
		playerY = 0;
	}
	
	if (positionX != playerX || positionY != playerY) {
		$.ajax({
			url: "/position",
			dataType: 'json',
			data: {
				"x": playerX,
				"y": playerY
			},
			success: (function (data, textStatus, jqXHR) {
				console.log("Position set!");
			}),
			error: (function () {
				console.log("Error while setting position!");
			})
		});	
	}
	
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
        		var needToUpdate = false;
				$.each(data, function( index ) {
					if (index == "position") {
						positionX = this.x;
						positionY = this.y;
					} else {
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
					}
			    });
			    
			    for (var i = 0; i < matrix.length; i ++) {
					if (matrix[i].x < (playerX - 8) || matrix[i].x > (playerX + 8)) {
						if (matrix[i].y < (playerY - 8) || matrix[i].y > (playerY + 8)) {
							delete matrix[i];
						}	
					}
			    }
			    
			    if (needToUpdate)
			    	Update();
			}),
			error: (function () {
				console.log("500: Server did not response!");
			})
		});
	}, 1000);	
});

