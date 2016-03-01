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
[Key]              VARCHAR(64),                                                                                                            --操作名称
[SourceUserId]     UNIQUEIDENTIFIER,                                                                                                       --来源用户ID
[CreateTime]       DATETIME DEFAULT GETDATE() NOT NULL                                                                                     --创建时间
)
GO

insert SYS_Logs_Rules (Code, Level, Source, Action, Message, CreatorUserId)
select '200101', 2, '系统平台', 'SqlQuery', null, '00000000-0000-0000-0000-000000000000' union all
select '200102', 2, '系统平台', 'SqlNonQuery', null, '00000000-0000-0000-0000-000000000000' union all
select '200103', 2, '系统平台', 'SqlScalar', null, '00000000-0000-0000-0000-000000000000' union all
select '200104', 2, '系统平台', 'SqlExecute', null, '00000000-0000-0000-0000-000000000000' union all
select '200105', 2, '系统平台', 'SqlExecute', null, '00000000-0000-0000-0000-000000000000' union all

select '300601', 3, '日志服务', '新增规则', '插入数据失败', '00000000-0000-0000-0000-000000000000' union all
select '300602', 3, '日志服务', '删除规则', '删除数据失败', '00000000-0000-0000-0000-000000000000' union all
select '300603', 3, '日志服务', '编辑规则', '更新数据失败', '00000000-0000-0000-0000-000000000000' union all

select '500101', 5, '系统平台', '接口验证', null, '00000000-0000-0000-0000-000000000000' union all
select '500601', 5, '日志服务', '接口验证', null, '00000000-0000-0000-0000-000000000000' union all
select '508001', 5, '商城服务', '接口验证', null, '00000000-0000-0000-0000-000000000000' union all

select '600601', 6, '日志服务', '新增规则', null, '00000000-0000-0000-0000-000000000000' union all
select '600602', 6, '日志服务', '删除规则', null, '00000000-0000-0000-0000-000000000000' union all
select '600603', 6, '日志服务', '编辑规则', null, '00000000-0000-0000-0000-000000000000'


insert SYS_Logs_Rules (Code, Level, Source, Action, Message, CreatorUserId, ToDataBase)
select '608001', 6, '商城服务', '提交订单', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608002', 6, '商城服务', '微信支付', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608003', 6, '商城服务', '生成分期', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608004', 6, '商城服务', '取消订单', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608005', 6, '商城服务', '删除订单', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608006', 6, '商城服务', '申请取消订单', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608007', 6, '商城服务', '申请退货', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608008', 6, '商城服务', '用户签收', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608008', 6, '商城服务', '自动签收', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608101', 6, '商城管理后台', '取消订单', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608102', 6, '商城管理后台', '确认订单', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608103', 6, '商城管理后台', '发货', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608104', 6, '商城管理后台', '制单', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608105', 6, '商城管理后台', '取消退货申请', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608106', 6, '商城管理后台', '确认退货申请', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608107', 6, '商城管理后台', '退货申请中已收货', null, '00000000-0000-0000-0000-000000000000', 1 union all
select '608108', 6, '商城管理后台', '退款', null, '00000000-0000-0000-0000-000000000000', 1