USE Insight_Log
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Logs') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Logs
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Logs_Rules') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Logs_Rules
GO


/*****日志数据表*****/

/*****日志规则表*****/

CREATE TABLE SYS_Logs_Rules(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Logs_Rules PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Code]             VARCHAR(6) NOT NULL,                                                                                                    --操作代码
[ToDataBase]       BIT DEFAULT 0 NOT NULL,                                                                                                 --是否写到数据库：0、否；1、是
[Level]            INT DEFAULT 0 NOT NULL,                                                                                                 --日志等级：0、Emergency；1、Alert；2、Critical；3、Error；4、Warning；5、Notice；6、Informational；7、Debug
[Source]           NVARCHAR(16),                                                                                                           --事件来源
[Action]           NVARCHAR(16),                                                                                                           --操作名称
[Message]          NVARCHAR(128),                                                                                                          --日志默认内容
[CreatorUserId]    UNIQUEIDENTIFIER NOT NULL,                                                                                              --创建人ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

/*****日志表*****/

CREATE TABLE SYS_Logs(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Logs PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --自增序列
[Level]            INT NOT NULL,                                                                                                           --日志等级：0、Emergency；1、Alert；2、Critical；3、Error；4、Warning；5、Notice；6、Informational；7、Debug
[Code]             VARCHAR(6),                                                                                                             --操作代码
[Source]           NVARCHAR(16) NOT NULL,                                                                                                  --事件来源
[Action]           NVARCHAR(16) NOT NULL,                                                                                                  --操作名称
[Message]          NVARCHAR(MAX),                                                                                                          --日志内容
[SourceUserId]     UNIQUEIDENTIFIER,                                                                                                       --来源用户ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO


