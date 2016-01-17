USE Insight_Log
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Logs') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Logs
GO
IF EXISTS (SELECT * FROM sysobjects WHERE id = OBJECT_ID(N'SYS_Logs_Rules') AND OBJECTPROPERTY(id, N'ISUSERTABLE') = 1)
DROP TABLE SYS_Logs_Rules
GO


/*****��־���ݱ�*****/

/*****��־�����*****/

CREATE TABLE SYS_Logs_Rules(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Logs_Rules PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --��������
[Code]             VARCHAR(6) NOT NULL,                                                                                                    --��������
[ToDataBase]       BIT DEFAULT 0 NOT NULL,                                                                                                 --�Ƿ�д�����ݿ⣺0����1����
[Level]            INT DEFAULT 0 NOT NULL,                                                                                                 --��־�ȼ���0��Emergency��1��Alert��2��Critical��3��Error��4��Warning��5��Notice��6��Informational��7��Debug
[Source]           NVARCHAR(16),                                                                                                           --�¼���Դ
[Action]           NVARCHAR(16),                                                                                                           --��������
[Message]          NVARCHAR(128),                                                                                                          --��־Ĭ������
[CreatorUserId]    UNIQUEIDENTIFIER NOT NULL,                                                                                              --������ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --����ʱ��
)
GO

/*****��־��*****/

CREATE TABLE SYS_Logs(
[ID]               UNIQUEIDENTIFIER CONSTRAINT IX_SYS_Logs PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
[SN]               BIGINT IDENTITY(1,1),                                                                                                   --��������
[Level]            INT NOT NULL,                                                                                                           --��־�ȼ���0��Emergency��1��Alert��2��Critical��3��Error��4��Warning��5��Notice��6��Informational��7��Debug
[Code]             VARCHAR(6),                                                                                                             --��������
[Source]           NVARCHAR(16) NOT NULL,                                                                                                  --�¼���Դ
[Action]           NVARCHAR(16) NOT NULL,                                                                                                  --��������
[Message]          NVARCHAR(MAX),                                                                                                          --��־����
[SourceUserId]     UNIQUEIDENTIFIER,                                                                                                       --��Դ�û�ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --����ʱ��
)
GO

insert SYS_Logs_Rules (Code, Level, Source, Action, Message, CreatorUserId)
select '200101', 2, 'ϵͳƽ̨', 'SqlQuery', null, '00000000-0000-0000-0000-000000000000' union all
select '200102', 2, 'ϵͳƽ̨', 'SqlNonQuery', null, '00000000-0000-0000-0000-000000000000' union all
select '200103', 2, 'ϵͳƽ̨', 'SqlScalar', null, '00000000-0000-0000-0000-000000000000' union all
select '200104', 2, 'ϵͳƽ̨', 'SqlExecute', null, '00000000-0000-0000-0000-000000000000' union all
select '200105', 2, 'ϵͳƽ̨', 'SqlExecute', null, '00000000-0000-0000-0000-000000000000' union all

select '300601', 3, '��־����', '��������', '��������ʧ��', '00000000-0000-0000-0000-000000000000' union all
select '300602', 3, '��־����', 'ɾ������', 'ɾ������ʧ��', '00000000-0000-0000-0000-000000000000' union all
select '300603', 3, '��־����', '�༭����', '��������ʧ��', '00000000-0000-0000-0000-000000000000' union all

select '500101', 5, 'ϵͳƽ̨', '�ӿ���֤', null, '00000000-0000-0000-0000-000000000000' union all
select '500601', 5, '��־����', '�ӿ���֤', null, '00000000-0000-0000-0000-000000000000' union all

select '600601', 6, '��־����', '��������', null, '00000000-0000-0000-0000-000000000000' union all
select '600602', 6, '��־����', 'ɾ������', null, '00000000-0000-0000-0000-000000000000' union all
select '600603', 6, '��־����', '�༭����', null, '00000000-0000-0000-0000-000000000000'
