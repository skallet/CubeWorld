var width = 400;
var cid = "canvas-map";
var canvasMinX, canvasMinY;
var blocks = new Array();
var matrix = new Array();
var users = new Array();
var userImg = new Image();
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
  
  offsetX = -(positionX - 5) * 50 + 1;
  offsetY = -(positionY - 5) * 50 + 1;
	
	var playerX = (positionX - Math.round(offsetX /50));
	if (playerX < 0) {
		playerX = 0;
	}
	
	var playerY = (positionY - Math.round(offsetY /50));
	if (playerY < 0) {
		playerY = 0;
	}
	
	for(var i = 0; i < matrix.length;i ++) {
		var x = matrix[i].x * 50 + offsetX;
		var y = matrix[i].y * 50 + offsetY;
		context.drawImage(blocks[matrix[i].src], x, y);
	}
  
  for(var i = 0; i < users.length;i ++) {
    var x = users[i].x * 50 + offsetX;
		var y = users[i].y * 50 + offsetY;
		context.drawImage(userImg, x, y);    
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
  
  var us = "/content/images/player.png";
  userImg.src = us;
  userImg.setAttribute("_src", us);
  
	
  /*
	$(document).mousedown(function (event) {
		mouseState.x = event.pageX - offsetX;
		mouseState.y = event.pageY - offsetY;
		mouseState.pressed = true;
	});
	
	$(document).mouseup(function (event) {
		mouseState.pressed = false;
	});
  context.addEventListener("mousemove", mouseMove, false);
  */
  
  $(document).keypress(function (event) {
    console.log("setting" + event.which);
    var x = positionX;
    var y = positionY;
    
    switch(event.which)
    {
      case 87, 119: y--;
      break;
      case 83, 115: y++;
      break;
      case 65, 97: x--;
      break;
      case 68, 100: x++;
      break;
      
      default:
        return;
    }
    
    $.ajax({
			url: "/position",
			dataType: 'json',
			data: {
				"x": x,
				"y": y
			},
			success: (function (data, textStatus, jqXHR) {
        if (data["status"] == "ok")
        {
          positionX = x;
          positionY = y;        
        }
			}),
			error: (function () {
			})
		});	
  });
  
  Update();
  setInterval(function () {
    Update();
  }, 500);

	setInterval(function () {
		$.ajax({
			url: "/initialize",
			dataType: 'json',
			success: (function (data, textStatus, jqXHR) {
        users = new Array();
        
				$.each(data, function( index ) {
					if (index == "position") {
						positionX = this.x;
						positionY = this.y;
          } else if (index.indexOf("user") != -1) {
            var index = users.length;
            users[index] = new Object();
            users[index].x = this.x;
            users[index].y = this.y;          
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
						} else {
							matrix[index] = new Object();
							matrix[index].x = this.x;
							matrix[index].y = this.y;
						}
						
						matrix[index].src = match;	
					}
			    });
			    
			    for (var i = 0; i < matrix.length; i ++) {
  					if (matrix[i].x < (positionX - 8) || matrix[i].x > (positionX + 8)) {
  						if (matrix[i].y < (positionY - 8) || matrix[i].y > (positionY + 8)) {
  							delete matrix[i];
  						}	
  					}
			    }
			}),
			error: (function () {
				console.log("500: Server did not response!");
			})
		});
	}, 5000);	
});

