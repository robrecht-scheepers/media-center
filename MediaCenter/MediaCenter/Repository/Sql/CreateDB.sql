BEGIN TRANSACTION;
DROP TABLE IF EXISTS `MediaInfo`;
CREATE TABLE IF NOT EXISTS `MediaInfo` (
	`Id`	INTEGER,
	`Name`	TEXT NOT NULL UNIQUE,
	`Type`	INTEGER NOT NULL,
	`Filename`	INTEGER NOT NULL UNIQUE,
	`DateTaken`	NUMERIC NOT NULL,
	`DateAdded`	NUMERIC NOT NULL,
	`Favorite`	INTEGER NOT NULL,
	`Private`	INTEGER NOT NULL,
	`Rotation`	INTEGER NOT NULL,
	`Tags`	TEXT NOT NULL DEFAULT '',
	PRIMARY KEY(`Id`)
);
DROP INDEX IF EXISTS `MediaInfo_Favorite`;
CREATE INDEX IF NOT EXISTS `MediaInfo_Favorite` ON `MediaInfo` (
	`Favorite`	ASC,
	`Private`	ASC
);
DROP INDEX IF EXISTS `MediaInfo_DateTaken`;
CREATE INDEX IF NOT EXISTS `MediaInfo_DateTaken` ON `MediaInfo` (
	`DateTaken`	ASC,
	`Private`	ASC
);
DROP INDEX IF EXISTS `MediaInfo_DateAdded`;
CREATE INDEX IF NOT EXISTS `MediaInfo_DateAdded` ON `MediaInfo` (
	`DateAdded`	ASC,
	`Private`	ASC
);
COMMIT;
