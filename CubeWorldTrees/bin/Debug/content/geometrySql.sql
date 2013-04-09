/*
inserting polygon structure
*/

INSERT INTO  `cubeworld`.`test` (
  `geometry`,
  `mtime`
)
VALUES (
  PolyFromText( 'POLYGON((1 0, 2 0, 2 1, 1 1, 1 0))' ),
  '2013-04-07 19:53:13'
);

/*
SELECT polygon intersecting with defined polygon
*/

SELECT mtime FROM `test` WHERE INTERSECTS(geometry, PolyFromText( 'POLYGON((1 0, 2 0, 2 1, 1 1, 1 0))' ));

/*
SETTING polygon structure
*/

SET @poly = 'POLYGON((1 0, 2 0, 2 1, 1 1, 1 0))';
SELECT mtime FROM `test` WHERE INTERSECTS(geometry, PolyFromText(@poly));

/*
GET polygon points
*/

SELECT AsText(geometry) FROM `test`;

