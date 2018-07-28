-- Database

CREATE DATABASE PhoneBook;
GO

-- Use

USE PhoneBook;

-- Baza danych 

CREATE TABLE People(
ID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
FirstName VARCHAR(32) NOT NULL,
LastName VARCHAR(64) NOT NULL,
Phone VARCHAR(24) NOT NULL,
Email VARCHAR(64) NOT NULL,
Created DATETIME NOT NULL,
Updated DATETIME NULL
); 

-- Log

CREATE TABLE LOGPeople(
LogDate DATETIME NOT NULL,
CommandType VARCHAR(7) NOT NULL,
ID INT NOT NULL,
FirstName VARCHAR(32) NULL,
LastName VARCHAR(64) NULL,
Phone VARCHAR(24) NULL,
Email VARCHAR(64) NULL,
); 

-- Index

CREATE NONCLUSTERED INDEX LastName_Index ON People (LastName);

-- Trigger
GO

CREATE TRIGGER PeopleDateInsert
ON People
AFTER INSERT
AS
	INSERT INTO LOGPeople VALUES 
	(GETDATE(), 
	'INSERT',
	(SELECT ID FROM inserted), 
	(SELECT FirstName FROM inserted), 
	(SELECT LastName FROM inserted), 
	(SELECT Phone FROM inserted), 
	(SELECT Email FROM inserted));
GO

CREATE TRIGGER PeopleDateUpdate
ON People
AFTER UPDATE
AS
	UPDATE People SET Updated = GETDATE() WHERE ID = (SELECT ID FROM inserted);
	
	INSERT INTO LOGPeople VALUES 
	(GETDATE(), 
	'UPDATE',
	(SELECT ID FROM inserted), 
	(SELECT FirstName FROM inserted), 
	(SELECT LastName FROM inserted), 
	(SELECT Phone FROM inserted), 
	(SELECT Email FROM inserted));
GO

CREATE TRIGGER PeopleDateDelete
ON People
AFTER DELETE
AS
	INSERT INTO LOGPeople (LogDate, CommandType, ID) VALUES (GETDATE(), 'DELETE', (SELECT ID FROM deleted));
GO

