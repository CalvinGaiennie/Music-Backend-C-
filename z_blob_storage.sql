-- Updated stored procedures for Azure Blob Storage integration
-- Replace SongData (VARBINARY) with SongBlobUrl (NVARCHAR)

-- 1. Update the AudioTracks table structure (run this first)
-- ALTER TABLE dbo.AudioTracks DROP COLUMN SongData;
-- ALTER TABLE dbo.AudioTracks ADD SongBlobUrl NVARCHAR(500);

-- 2. Updated Delete procedure (no changes needed)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[spAudioTracks_Delete]
    @AudioTrackId INT
    , @UserId INT 
AS
BEGIN
    DELETE FROM dbo.AudioTracks 
        WHERE AudioTrackId = @AudioTrackId
            AND UserId = @UserId
END
GO

-- 3. Updated Get procedure (returns blob URL instead of binary data)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[spAudioTracks_Get]
/*EXEC dbo.spAudioTracks_Get @UserId = 1003, @SearchValue='Second'*/
/*EXEC dbo.spAudioTracks_Get @AudioTrackId = 2*/
    @UserId INT = NULL
    , @SearchValue NVARCHAR(MAX) = NULL
    , @AudioTrackId INT = NULL
AS
BEGIN
    SELECT 
        AudioTrackId,
        UserId,
        SongName,
        SongTip,
        SongKey,
        SongChords,
        SongInstrument,
        SongDifficulty,
        SongBlobUrl,
        CreatedAt,
        UpdatedAt
    FROM dbo.AudioTracks AS AudioTracks
        WHERE AudioTracks.UserId = ISNULL(@UserId, AudioTracks.UserId)
            AND AudioTracks.AudioTrackId = ISNULL(@AudioTrackId, AudioTracks.AudioTrackId)
            AND (@SearchValue IS NULL
                OR AudioTracks.SongName LIKE '%' + @SearchValue + '%')
END
GO

-- 4. GetList procedure (no changes needed - already excludes SongData)
CREATE OR ALTER PROCEDURE [dbo].[spAudioTracks_GetList]
/*EXEC dbo.spAudioTracks_GetList @UserId = 1003, @SearchValue='Second'*/
/*EXEC dbo.spAudioTracks_GetList @AudioTrackId = 2*/
    @UserId INT = NULL
    , @SearchValue NVARCHAR(MAX) = NULL
    , @AudioTrackId INT = NULL
AS
BEGIN
    SELECT 
        AudioTrackId,
        UserId,
        SongName,
        SongTip,
        SongKey,
        SongChords,
        SongInstrument,
        SongDifficulty,
        CreatedAt,
        UpdatedAt
    FROM dbo.AudioTracks AS AudioTracks
        WHERE AudioTracks.UserId = ISNULL(@UserId, AudioTracks.UserId)
            AND AudioTracks.AudioTrackId = ISNULL(@AudioTrackId, AudioTracks.AudioTrackId)
            AND (@SearchValue IS NULL
                OR AudioTracks.SongName LIKE '%' + @SearchValue + '%')
END
GO

-- 5. Updated Upsert procedure (handles blob URL instead of binary data)
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[spAudioTracks_Upsert]
    @UserId INT
    , @AudioTrackId INT = NULL
    , @SongName NVARCHAR(255)
    , @SongTip NVARCHAR(MAX)
    , @SongKey NVARCHAR(50)
    , @SongChords NVARCHAR(MAX)
    , @SongInstrument NVARCHAR(100)
    , @SongDifficulty NVARCHAR(50)
    , @SongBlobUrl NVARCHAR(500)
AS
BEGIN
    IF @AudioTrackId IS NULL OR NOT EXISTS (SELECT * FROM dbo.AudioTracks WHERE AudioTrackId = @AudioTrackId)
        BEGIN
            INSERT INTO dbo.AudioTracks(
                [UserId],
                [SongName],
                [SongTip],
                [SongKey],
                [SongChords],
                [SongInstrument],
                [SongDifficulty],
                [SongBlobUrl],
                [CreatedAt],
                [UpdatedAt]
            ) VALUES (
                @UserId,
                @SongName,
                @SongTip,
                @SongKey,
                @SongChords,
                @SongInstrument,
                @SongDifficulty,
                @SongBlobUrl,
                GETDATE(),
                GETDATE()
            )
        END
    ELSE
        BEGIN
            UPDATE dbo.AudioTracks 
                SET SongName = @SongName,
                    SongTip = @SongTip,
                    SongKey = @SongKey,
                    SongChords = @SongChords,
                    SongInstrument = @SongInstrument,
                    SongDifficulty = @SongDifficulty,
                    SongBlobUrl = @SongBlobUrl,
                    UpdatedAt = GETDATE()
                WHERE AudioTrackId = @AudioTrackId
                    AND UserId = @UserId
        END
END
GO

-- 6. Migration procedure to update existing data (if needed)
CREATE OR ALTER PROCEDURE [dbo].[spAudioTracks_MigrateToBlob]
    @AudioTrackId INT
    , @SongBlobUrl NVARCHAR(500)
AS
BEGIN
    UPDATE dbo.AudioTracks 
        SET SongBlobUrl = @SongBlobUrl,
            UpdatedAt = GETDATE()
        WHERE AudioTrackId = @AudioTrackId
END
GO 