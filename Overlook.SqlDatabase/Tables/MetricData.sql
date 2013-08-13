CREATE TABLE [dbo].[MetricData]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Date] INT NOT NULL, 
    [MetricDevice] NVARCHAR(30) NOT NULL, 
    [MetricCategory] NVARCHAR(30) NOT NULL, 
    [MetricName] NVARCHAR(50) NOT NULL, 
    [SuffixLabel] NVARCHAR(50) NOT NULL
)
