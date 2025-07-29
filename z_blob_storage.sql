-- Updated stored procedures for Azure Blob Storage integration
-- Replace SongData (VARBINARY) with SongBlobUrl (NVARCHAR)
-- Added support for SongArtist, SongAlbum, and SongLength fields

-- 1. Update the AudioTracks table structure (run this first)
-- ALTER TABLE dbo.AudioTracks DROP COLUMN SongData;
-- ALTER TABLE dbo.AudioTracks ADD SongBlobUrl NVARCHAR(500);
-- ALTER TABLE dbo.AudioTracks ADD SongArtist NVARCHAR(255);
-- ALTER TABLE dbo.AudioTracks ADD SongAlbum NVARCHAR(255);
-- ALTER TABLE dbo.AudioTracks ADD SongLength NVARCHAR(50);

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

-- 3. Updated Get procedure (returns all fields including new ones)
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
        SongArtist,
        SongAlbum,
        SongLength,
        SongBlobUrl,
        CreatedAt,
        UpdatedAt
    FROM dbo.AudioTracks AS AudioTracks
        WHERE AudioTracks.UserId = ISNULL(@UserId, AudioTracks.UserId)
            AND AudioTracks.AudioTrackId = ISNULL(@AudioTrackId, AudioTracks.AudioTrackId)
            AND (@SearchValue IS NULL
                OR AudioTracks.SongName LIKE '%' + @SearchValue + '%'
                OR AudioTracks.SongArtist LIKE '%' + @SearchValue + '%'
                OR AudioTracks.SongAlbum LIKE '%' + @SearchValue + '%')
END
GO

-- 4. GetList procedure removed - all endpoints now return full data including blob URL

-- 5. Updated Upsert procedure (handles all fields including new ones)
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
    , @SongArtist NVARCHAR(255)
    , @SongAlbum NVARCHAR(255)
    , @SongLength NVARCHAR(50)
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
                [SongArtist],
                [SongAlbum],
                [SongLength],
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
                @SongArtist,
                @SongAlbum,
                @SongLength,
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
                    SongArtist = @SongArtist,
                    SongAlbum = @SongAlbum,
                    SongLength = @SongLength,
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