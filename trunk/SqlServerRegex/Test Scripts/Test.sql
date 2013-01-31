-- Examples for queries that exercise different SQL objects implemented by this assembly

-----------------------------------------------------------------------------------------
-- Stored procedure
-----------------------------------------------------------------------------------------
-- exec StoredProcedureName


-----------------------------------------------------------------------------------------
-- User defined function
-----------------------------------------------------------------------------------------
-- select dbo.FunctionName()


-----------------------------------------------------------------------------------------
-- User defined type
-----------------------------------------------------------------------------------------
-- CREATE TABLE test_table (col1 UserType)
--
-- INSERT INTO test_table VALUES (convert(uri, 'Instantiation String 1'))
-- INSERT INTO test_table VALUES (convert(uri, 'Instantiation String 2'))
-- INSERT INTO test_table VALUES (convert(uri, 'Instantiation String 3'))
--
-- select col1::method1() from test_table



-----------------------------------------------------------------------------------------
-- User defined type
-----------------------------------------------------------------------------------------
-- select dbo.AggregateName(Column1) from Table1


select dbo.RegexMatch(N'.*',N'To run your project, please edit the Test.sql file in your project. This file is located in the Test Scripts folder in the Solution Explorer.')
go
select dbo.RegexGroup( N'https?://(?<server>([\w-]+\.)*[\w-]+)', N'https://bla.bla.bla',N'server' )
go
declare @text nvarchar(max), @pattern nvarchar(max)
select
    @text = N'To run your project, please edit the Test.sql file in your project. This file is located in the Test Scripts folder in the Solution Explorer.',
    @pattern = '\w+'
select count(distinct [Text])
    from dbo.RegexMatches( @pattern, @text )
go
declare @pattern nvarchar(max), @list nvarchar(max)
select @pattern = N'[^,]+', @list = N'2,4,6'
go
select d.* from [Data] d
inner join dbo.RegexMatches( @pattern, @list ) re
    on d.[ID] = re.[Text]
go
