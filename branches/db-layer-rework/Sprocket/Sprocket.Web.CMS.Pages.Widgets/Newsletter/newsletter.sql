IF NOT EXISTS(SELECT id FROM sysobjects WHERE name='NewsletterSubscribers' AND type='U')
CREATE TABLE dbo.NewsletterSubscribers
(
	EmailAddress nvarchar(255) PRIMARY KEY
)

go

IF EXISTS(SELECT id FROM sysobjects WHERE name='Newsletter_Subscribe' AND type='U')
	DROP PROCEDURE Newsletter_Subscribe
go
CREATE PROCEDURE dbo.Newsletter_Subscribe
	@EmailAddress nvarchar(255)
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM NewsletterSubscribers WHERE EmailAddress = @EmailAddress)
	INSERT INTO NewsletterSubscribers VALUES (@EmailAddress)
END

go

IF EXISTS(SELECT id FROM sysobjects WHERE name='Newsletter_Unsubscribe' AND type='U')
	DROP PROCEDURE Newsletter_Unsubscribe
go
CREATE PROCEDURE dbo.Newsletter_Unsubscribe
	@EmailAddress nvarchar(255)
AS
BEGIN
	DELETE FROM NewsletterSubscribers WHERE @EmailAddress = @EmailAddress
END

go

IF EXISTS(SELECT id FROM sysobjects WHERE name='Newsletter_ListSubscribers' AND type='U')
	DROP PROCEDURE Newsletter_ListSubscribers
go
CREATE PROCEDURE dbo.Newsletter_ListSubscribers
AS
BEGIN
	SELECT * FROM NewsletterSubscribers
END

go
