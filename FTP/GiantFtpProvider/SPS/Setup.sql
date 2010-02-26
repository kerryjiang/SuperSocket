declare @GbUnit bigint
declare @MbUnit int
set @GbUnit = 1024 * 1024 * 1024
set @MbUnit = 1024 * 1024
insert into FtpUsers (UserName, PassWord, Email, MaxSpace, UsedSpace, MaxThread, MaxSpeed, LowerUserName, LowerEmail)
values ('administrator', '123456', 'kerry-jiang@hotmail.com', @GbUnit, 0, 5, @MbUnit, 'administrator', 'kerry-jiang@hotmail.com')