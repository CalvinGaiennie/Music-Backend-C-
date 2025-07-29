-- Update AudioTracks table to add new fields and remove SongData
-- Run this script to update your database schema

-- Add SongBlobUrl column
ALTER TABLE dbo.AudioTracks ADD SongBlobUrl NVARCHAR(500);

-- Drop the old SongData column (if it exists)
IF COL_LENGTH('dbo.AudioTracks', 'SongData') IS NOT NULL
BEGIN
    ALTER TABLE dbo.AudioTracks DROP COLUMN SongData;
END

-- Add new metadata fields
ALTER TABLE dbo.AudioTracks ADD SongArtist NVARCHAR(255);
ALTER TABLE dbo.AudioTracks ADD SongAlbum NVARCHAR(255);
ALTER TABLE dbo.AudioTracks ADD SongLength NVARCHAR(50);
ALTER TABLE dbo.AudioTracks ADD RecordingQuality NVARCHAR(50);
