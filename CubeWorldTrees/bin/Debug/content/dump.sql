/*
driver for c# - http://dev.mysql.com/downloads/mirror.php?id=412152

*/

CREATE DATABASE  `cubeworld` DEFAULT CHARACTER SET utf8 COLLATE utf8_bin;

/* `users` table */
CREATE TABLE  `cubeworld`.`players` (
`id` INT NOT NULL AUTO_INCREMENT PRIMARY KEY ,
`username` VARCHAR( 255 ) NOT NULL ,
`password` VARCHAR( 255 ) NOT NULL ,
`salt` VARCHAR( 100 ) NOT NULL ,
`position` POINT NOT NULL,
`ctime` DATETIME NOT NULL,
`mtime` DATETIME NOT NULL,
INDEX (  `username` ,  `ctime` )
) ENGINE = InnoDB CHARSET=utf8 COLLATE=utf8_bin AUTO_INCREMENT=1 ;

/* `users` data */
INSERT INTO `players` (`id`, `username`, `password`, `salt`, `position`, `ctime`, `mtime`) VALUES
(1, 'root', '0E-20-78-36-17-E9-5F-BE-3B-8E-0F-1A-CD-65-41-D9-72-A5-63-F2-DB-B7-0F-CE-83-A0-1B-B3-6F-99-12-6F', 'sadf789svqwerasd35f432vx', POINT(5, 5), '2013-03-21 14:46:40', NOW());


/* `tiles` table */

CREATE TABLE IF NOT EXISTS `tiles` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `coord` point DEFAULT NULL,
  `image` varchar(255) COLLATE utf8_bin DEFAULT NULL,
  `owner` int(11) NULL,
  PRIMARY KEY (`id`),
  KEY `coord` (`coord`(25))
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin AUTO_INCREMENT=1 ;
