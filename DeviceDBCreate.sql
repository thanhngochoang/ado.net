/*
Run this script on:

        winweb01.kloon.vn.huyhihi_Customer    -  This database will be modified

to synchronize it with:

        FX570ES.Device

You are recommended to back up your database before running this script

Script created by SQL Compare version 12.4.12.5042 from Red Gate Software Ltd at 3/6/2020 10:59:24 AM

*/
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL Serializable
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[Confirmation]'
GO
CREATE TABLE [dbo].[Confirmation]
(
[InstallationId] [uniqueidentifier] NOT NULL,
[Phone] [nvarchar] (40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[ConfirmationCode] [nvarchar] (40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CreatedOn] [datetime] NOT NULL,
[ConfirmedOn] [datetime] NULL,
[RevokedOn] [datetime] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Confirmation] on [dbo].[Confirmation]'
GO
ALTER TABLE [dbo].[Confirmation] ADD CONSTRAINT [PK_Confirmation] PRIMARY KEY CLUSTERED  ([InstallationId])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[Code]'
GO
CREATE TABLE [dbo].[Code]
(
[Id] [int] NOT NULL IDENTITY(1, 1),
[Code] [nvarchar] (40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[Phone] [nvarchar] (40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CreatedOn] [datetime] NOT NULL,
[SentOn] [datetime] NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Code] on [dbo].[Code]'
GO
ALTER TABLE [dbo].[Code] ADD CONSTRAINT [PK_Code] PRIMARY KEY CLUSTERED  ([Id])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[ConfirmDeviceRegistration]'
GO
SET ANSI_NULLS OFF
GO
/****** Object:  StoredProcedure [dbo].[ConfirmDeviceRegistration]    Script Date: 04.03.2020 10:41:36 ******/

CREATE PROCEDURE [dbo].[ConfirmDeviceRegistration](@installationid varchar(max), @confirmationcode varchar(max))
AS
BEGIN
	IF (exists (select * from dbo.Confirmation AS C where installationid=@installationid and confirmedon is null and confirmationcode != @confirmationcode))
		begin
			select 403;
			return;
		end
	if (not exists (select * from dbo.Confirmation AS C where installationid=@installationid))
		begin
			select 404;
			return;
		end
	if (exists (select * from dbo.Confirmation AS C where installationid=@installationid and confirmedon is null and revokedon is null and confirmationcode=@confirmationcode))
	BEGIN
		DECLARE @phone NVARCHAR(40);
		SELECT  @phone = C.Phone FROM dbo.Confirmation AS C WHERE installationid=@installationid
		-- set installationid as confirmed
		UPDATE dbo.Confirmation set confirmedon=CURRENT_TIMESTAMP WHERE installationid=@installationid
		-- revoke all installationid for the same phone number (for old phones)
		UPDATE dbo.Confirmation set revokedon= CURRENT_TIMESTAMP WHERE phone = @phone and installationid <> @installationid
		
		INSERT INTO dbo.Code (Code, Phone, CreatedOn)
		VALUES (@confirmationcode, @phone,  GETDATE())
		select 200;
	end
	else
		select 500

END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[Phone]'
GO
SET ANSI_NULLS ON
GO
CREATE TABLE [dbo].[Phone]
(
[Phone] [nvarchar] (40) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
[CreatedOn] [datetime] NOT NULL,
[ChangedOn] [datetime] NOT NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_Phone] on [dbo].[Phone]'
GO
ALTER TABLE [dbo].[Phone] ADD CONSTRAINT [PK_Phone] PRIMARY KEY CLUSTERED  ([Phone])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[RegisterDevice]'
GO
CREATE PROCEDURE [dbo].[RegisterDevice](@phone varchar(max), @confirmationcode varchar(max))
AS
BEGIN
	INSERT INTO dbo.Phone (Phone, CreatedOn, ChangedOn)
	SELECT @phone, current_timestamp, current_timestamp
	WHERE not exists (select * from phone where phone=@phone)

	declare @installationid uniqueidentifier = NEWID()

	INSERT INTO dbo.Confirmation (installationid, phone, ConfirmationCode, createdon)
	SELECT @installationid, @phone, @confirmationcode, current_timestamp

	SELECT @installationId
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[RequestDeviceCode]'
GO
CREATE PROCEDURE [dbo].[RequestDeviceCode](@installationid uniqueidentifier)
AS
BEGIN
	SET NOCOUNT ON;
	declare @code varchar(max)
	declare @codeid int
	select top 1
		@code=C.Code, @codeid=C.Id 
	from 
		dbo.Code AS C 
		inner join dbo.Phone AS P on C.phone=P.phone 
		inner join dbo.Confirmation AS CF on P.phone=CF.phone 
	where 
		CF.InstallationId=@installationid
		and
		CF.ConfirmedOn is not null
		and
		CF.RevokedOn is null
		and
		C.SentOn is null
	order by
		C.createdon desc

	update dbo.Code set SentOn=current_timestamp where id=@codeid
	select @code
END
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[UpdateDeviceCode]'
GO
CREATE PROCEDURE [dbo].[UpdateDeviceCode](@phone varchar(max), @code varchar(max))
AS
BEGIN
	SET NOCOUNT ON;
	INSERT INTO Code(Phone, Code, CreatedOn)
	SELECT @phone, @code, CURRENT_TIMESTAMP
	WHERE EXISTS (select * from phone where phone=@phone)
END


SET ANSI_NULLS ON
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Adding foreign keys to [dbo].[Code]'
GO
ALTER TABLE [dbo].[Code] ADD CONSTRAINT [FK_Code_Phone] FOREIGN KEY ([Phone]) REFERENCES [dbo].[Phone] ([Phone])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO
