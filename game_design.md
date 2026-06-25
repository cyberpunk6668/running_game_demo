# 《浪潮》游戏设计与实现文档（基于 demo 画面与当前 Godot 原型扩展）

本文基于以下信息整理：

- demo 页面文案：**“改革开放四十年的创业传奇”**；
- 参考画面：开场介绍页、主界面三大经营板块、公司概况弹窗、投资卡片面板；
- 当前项目原型代码：`Godot 4.7 + .NET 8 + C#`；
- 当前已实现脚本：`GameManager.cs`、`DataManager.cs`、`EventManager.cs`、`MainUIController.cs`、`SimulationModels.cs`；
- 当前已接入数据：`Data/projects.json`、`Data/events.json`。

这份文档的目标不是只描述“想做什么”，而是要把它整理成一份**可直接指导后续制作**的设计蓝图：既保留 demo 的氛围和结构，又贴合现在仓库里已经存在的系统和代码组织。

---

## 一、项目定位

### 1.1 核心题材

《浪潮》是一款横跨 1978—2018 的时代创业模拟经营游戏。玩家以普通创业者身份起步，带着 **3000 元启动资金**，在改革开放四十年的历史浪潮中，通过经营、扩张、招聘、营销、事件决策与时代选择，从个体小店逐步成长为区域品牌乃至商业集团。

### 1.2 核心体验目标

玩家在游戏中应该持续感受到四件事：

1. **时代在推动我**：每个阶段都有不同机会与风险；
2. **每月都有取舍**：扩产、招人、营销、保现金流，不能全都要；
3. **数值变化可解释**：为什么赚钱、为什么亏损、为什么幸福下降，都要能从 UI 上看出来；
4. **从小摊到帝国的成长感**：项目由点到面，从单一投资变成系统协同。

### 1.3 设计支柱

#### 支柱 A：时代叙事

不是纯数学表，而是“身处时代中的商业冒险”。每一个年代都要有自己的关键词、政策风向、主流渠道和组织打法。

#### 支柱 B：空间化经营界面

demo 画面不是单纯的表格列表，而是**上方空间场景 + 下方投资面板**的组合。玩家不是在 Excel 里点升级，而是在经营一块逐渐被商业节点填满的版图。

#### 支柱 C：轻度策略，强反馈

每个月一个 Tick，操作简单，但反馈要密：

- 左侧经营概况即时变化；
- 投资后市场需求、产能、竞争力、利润预估马上刷新；
- 事件选择立刻改变经营节奏；
- 成就、概况、日志共同强化成长反馈。

#### 支柱 D：数据驱动扩展

项目内容、时代事件、新闻、成就、排行公司都尽量做成 JSON 配置化，避免每次扩内容都要大改核心逻辑。

---

## 二、从 demo 画面反推的目标玩法结构

结合截图与 demo 页面，可以明确本作的目标主界面由以下部分组成。

### 2.1 开场页

开场页应包含：

- 游戏标题“浪潮”；
- 一段时代背景文案；
- 五个时代阶段说明；
- 6 条左右的特色卖点标签；
- 一个主按钮“开始创业”。

开场页的作用不是教学，而是**建立主题情绪与时代跨度**。玩家点击“开始创业”后进入第一条引导事件，而不是直接丢进主界面。

### 2.2 主界面布局

根据截图，主界面至少应该有以下五个可识别区域：

1. **顶部控制栏**
	- 当前年月；
	- 暂停 / 单步 / 倍速控制；
	- 概况 / 新闻 / 指南 / 排行 / 保存 等入口。

2. **左侧经营状态栏**
	- 资金；
	- 声望；
	- 人脉；
	- 知识；
	- 合规；
	- 幸福；
	- 月收入 / 月支出 / 月净利；
	- 市场引擎关键指标。

3. **中部经营版图**
	- 当前板块的场景化节点展示；
	- 已建项目以“节点卡牌/建筑图标”的形式出现在场景中；
	- 可通过点击节点高亮对应投资卡。

4. **下方投资面板**
	- 按板块切换：`生产车间` / `人事团队` / `市场营销`；
	- 每个板块显示一组项目卡；
	- 卡片展示：名称、等级、成本、维护、效果、解锁条件、ROI 提示。

5. **弹窗层**
	- 事件决策弹窗；
	- 公司概况弹窗；
	- 新闻弹窗；
	- 指南弹窗；
	- 排行榜弹窗。

### 2.3 公司概况弹窗

截图中的“公司经营概况”不是装饰，而是一个关键反馈页，建议包含四块内容：

1. **经营状况**：产能效率、团队效率、市场效率、总资金；
2. **市场引擎**：市场需求、供给能力、竞争力、月收入；
3. **成就矩阵**：按类别展示已完成 / 未完成的关键 milestone；
4. **策略评级**：根据玩家打法给出分数与标签。

### 2.4 demo 对当前原型的启示

当前仓库原型已经实现了：

- 月度时间推进；
- 三大板块投资；
- 事件弹窗；
- 预测收入 / 支出 / 净利润；
- 存档 / 读档。

但要更贴近 demo，仍需补足：

- `合规` 指标；
- 新闻 / 指南 / 排行弹窗；
- 公司概况与成就系统；
- 更丰富的项目数量；
- 上方经营版图的空间化表达；
- 更强的时代事件链与行业波动。

---

## 三、版本目标：当前原型与目标成品对照

| 模块 | 当前原型状态 | 目标版本要求 |
| --- | --- | --- |
| 时间推进 | 已实现：暂停、开始、单月推进、倍速 | 保持，并加入更明确的动画与提示 |
| 三大投资板块 | 已实现基础卡片型投资 | 扩充项目数量，并与空间版图联动 |
| 市场结算 | 已实现基础供需 + 竞争力 + 利润模型 | 增加通胀、政策、合规、新闻扰动 |
| 时代事件 | 已实现少量事件 | 扩展为“历史大事件 + 随机经营事件”双层系统 |
| 左侧经营面板 | 已实现资金 / 声望 / 人脉 / 知识 / 幸福 | 增加合规、风险提示、趋势箭头 |
| 公司概况弹窗 | 未实现 | 必做，用于总览经营质量与成就 |
| 新闻系统 | 未实现 | 建议实现，承载时代氛围与市场波动 |
| 指南系统 | 未实现 | 建议实现，作为新手帮助和词条库 |
| 排行系统 | 未实现 | 可用 AI 公司或预设竞争对手生成 |
| 内容量 | 当前约 12 个项目、4 个事件 | 目标至少 40+ 项目、20+ 事件、30+ 成就 |

---

## 四、核心玩法循环（Gameplay Loop）

### 4.1 单月循环

每一个月是一个最小经营回合，流程如下：

1. 玩家查看当前经营盘面与预估收益；
2. 选择是否投资 / 升级某个项目；
3. 选择是否切换板块，优化产能、人效或营销；
4. 点击“开始”让时间流动，或点击“单步”推进一个月；
5. 系统进行月度结算；
6. 若触发事件，则暂停游戏并弹出决策框；
7. 结算完成后刷新资金、属性、解锁项和日志；
8. 进入下一月。

### 4.2 中期循环（跨时代）

每一时代有不同的推荐经营重心：

- 前期：先保生存与现金流；
- 中期：建立产能与组织体系；
- 后期：做品牌、做渠道、做数字化；
- 终局：多线协同，让公司在多个指标上保持均衡。

### 4.3 核心决策冲突

游戏乐趣来自“不能兼得”：

- 扩产快，但幸福可能下降；
- 招人多，但维护成本变高；
- 营销猛，但如果供给跟不上会带来库存或交付压力；
- 利润高，不代表长期更稳；
- 合规低时，可能在某些时代触发严重负面事件。

---

## 五、时代内容设计

为了与 demo 文案保持一致，建议最终显示时代名称调整为以下五段（当前代码中的 `EraType` 与显示名可以一一映射）：

| 时代 | 年份 | 时代关键词 | 主要经营主题 | 对应代码映射建议 |
| --- | --- | --- | --- | --- |
| 春风时代 | 1978-1984 | 个体经济、起步、试错 | 小成本创业、抢第一桶金 | `SpringBreeze` |
| 浪潮时代 | 1985-1992 | 价格改革、双轨并行、野蛮生长 | 快速扩张、建立区域优势 | `Wave` |
| 崛起时代 | 1993-2001 | 品牌化、证券化、入世前夜 | 质量、组织、外部合作 | 当前 `Leap` 建议显示为“崛起时代” |
| 腾飞时代 | 2002-2008 | 连锁、外贸、渠道爆发 | 规模复制、供应链建设 | 当前 `Global` 建议显示为“腾飞时代” |
| 新潮时代 | 2009-2018 | 电商、移动互联网、数字运营 | 数字化经营、私域与效率 | 当前 `Digital` 建议显示为“新潮时代” |

### 5.1 当前原型已实现的时代基础数值

`DataManager.cs` 中已经存在每个时代的三组基础参数：

- `BaseDemand`：时代基础需求；
- `BaseUnitProfit`：时代基础客单利润；
- `BaseCompetitiveness`：时代基础竞争力。

当前原型参数如下：

| 时代 | BaseDemand | BaseUnitProfit | BaseCompetitiveness |
| --- | ---: | ---: | ---: |
| 春风时代 | 26 | 19 | 0.82 |
| 浪潮时代 | 44 | 25 | 1.05 |
| 崛起时代 | 64 | 31 | 1.24 |
| 腾飞时代 | 88 | 37 | 1.42 |
| 新潮时代 | 118 | 45 | 1.68 |

这些参数已经足够支撑第一版节奏，但后续建议额外加入：

- `inflation_index`（通胀）；
- `policy_volatility`（政策波动）；
- `channel_efficiency_bonus`（渠道时代红利）；
- `labor_cost_factor`（劳动力成本系数）；
- `digital_bonus`（数字化加成，仅后期生效）。

### 5.2 每个时代推荐新增的必做事件

1. **1978-12：创业起步**
	- 选择先做现金流还是先做人脉知识；
2. **1984-01：价格改革窗口**
	- 决定激进扩张还是稳住财务；
3. **1992-01：市场信心重启**
	- 选择品牌化还是技术组织升级；
4. **2001-12：加入全球市场**
	- 开放外贸、认证、供应链项目；
5. **2003-04：公共事件冲击**
	- 线下渠道受损，组织韧性项目价值上升；
6. **2008-09：金融震荡**
	- 现金为王，扩张风险抬升；
7. **2014-01：移动互联网红利**
	- 小程序、会员 App、私域运营解锁；
8. **2018-12：阶段收官**
	- 生成总结页、策略评分、结局评价。

---

## 六、核心数值系统设计

### 6.1 基础资源

当前原型已有：

- `Money`：资金；
- `Reputation`：声望；
- `Network`：人脉；
- `Knowledge`：知识；
- `Happiness`：幸福。

为了更贴近截图，建议新增：

- `Compliance`：合规；
- `Risk`：经营风险（可不直接显示为资源，而是展示为预警等级）；
- `BrandHeat`：品牌热度（可作为营销和复购的中间量）；
- `CashBufferMonths`：现金缓冲月数（概况页展示）。

### 6.2 面板中应显示的衍生指标

左侧经营栏建议显示：

- 预计月收入；
- 预计月支出；
- 预计净利润；
- 市场需求；
- 供给能力；
- 市场竞争力；
- 客单利润；
- 库存压力成本；
- 固定成本；
- 项目维护成本；
- 幸福修正倍率；
- 合规风险提示（低 / 中 / 高）。

### 6.3 月度结算公式（与当前原型对齐）

当前 `DataManager.BuildSnapshot()` 已经形成了一套很好的第一版公式，建议在文档里明确下来，方便后续所有扩展遵守统一逻辑。

#### 当前公式

1. 幸福修正：

`happiness_modifier = clamp(0.55 + happiness / 100, 0.45, 1.40)`

2. 人脉带来的需求加成：

`network_demand_factor = 1 + network * 0.0025`

3. 有效需求：

`effective_demand = (era_base_demand + demand_bonus + project_demand + reputation * 0.85) * (1 + demand_multiplier) * network_demand_factor`

4. 供给能力：

`supply_capacity = (12 + project_capacity) * happiness_modifier * (1 + capacity_multiplier)`

5. 市场竞争力：

`competitiveness = clamp(era_base_competitiveness + competitiveness_bonus + project_competitiveness + reputation*0.015 + knowledge*0.010 + network*0.007 + competitiveness_multiplier, 0.45, 4.50)`

6. 客单利润：

`unit_profit = era_base_unit_profit + unit_profit_bonus + project_unit_profit + knowledge*0.12 + reputation*0.04`

7. 实际销量：

`actual_sales = min(supply_capacity, effective_demand * competitiveness)`

8. 月收入：

`monthly_income = actual_sales * unit_profit`

9. 月支出：

`monthly_expense = project_maintenance + fixed_expense + inventory_cost`

10. 净利润：

`net_profit = monthly_income - monthly_expense`

#### 后续建议扩展公式

在不推翻现有框架的前提下，增加三项最值得补的扰动：

1. **通胀**：提高维护成本和基础利润上限；
2. **合规**：低合规时触发罚款、停业、品牌打击；
3. **新闻波动**：影响 3-12 个月内的需求基数或竞争系数。

可以扩展为：

`monthly_expense = project_maintenance * inflation_factor + fixed_expense + inventory_cost + compliance_penalty`

`effective_demand = effective_demand * news_multiplier * season_multiplier`

### 6.4 月结后自动变化

当前原型已经实现了以下月结后属性波动：

- 盈利时幸福小幅上升；
- 高支出 / 低幸福时幸福额外下降；
- 盈利时声望增加，亏损时声望降低；
- 项目提供的知识 / 人脉会带来缓慢积累。

这一机制非常好，建议保留，并继续强化“组织建设不是一次性买断，而是长期缓增”的设计方向。

---

## 七、项目内容设计（重点扩充）

当前原型已经有 12 个项目雏形，但要贴近 demo，需要把它扩展为一套更丰富的项目池。建议最终目标如下：

- 生产车间：**12-15 个项目**；
- 人事团队：**17 个项目左右**；
- 市场营销：**18 个渠道左右**。

所有项目继续沿用现有 `ProjectDefinition` 数据结构：

- `id`
- `name`
- `description`
- `type`
- `maxLevel`
- `baseUpfrontCost`
- `costGrowthFactor`
- `maintenanceCost`
- `unlockCondition`
- `effects`

### 7.1 生产车间（建议扩展清单）

生产类项目负责提升产能、交付稳定性、库存效率和单位利润，是“能不能接住市场需求”的核心。

| 项目名 | 所属时代 | 主要定位 | 建议主效果 |
| --- | --- | --- | --- |
| 个体餐饮小店 | 春风时代 | 起步现金流 | `capacity`、`unitProfit`、少量 `reputation` |
| 来料加工车间 | 春风时代 | 初步工业化 | `capacity`、`knowledge`、`inventoryCostReduction` |
| 乡镇工厂 | 浪潮时代 | 区域扩产 | `capacity`、`network`、`reputation` |
| 标准化生产线 | 浪潮时代 | 提高规模复制能力 | `capacityMultiplier`、`knowledge` |
| OEM 订单中心 | 浪潮时代 | 快速吃需求 | `capacity`、`demand`，但 `unitProfit` 较低 |
| 连锁中央厨房 | 浪潮时代 | 连锁复制核心 | `capacity`、`competitiveness`、`unitProfit` |
| 质量检验中心 | 崛起时代 | 控制质量与口碑 | `reputation`、`compliance`、`inventoryCostReduction` |
| 外贸代工厂 | 崛起时代 | 承接外部订单 | `demand`、`network`、`unitProfit` |
| 区域仓配中心 | 崛起时代 | 降低积压与配送损失 | `inventoryCostReduction`、`capacityMultiplier` |
| 连锁直营门店 | 腾飞时代 | 控制终端体验 | `unitProfit`、`reputation`、`demand` |
| 自动化产线 | 腾飞时代 | 用技术替代低效人工 | `capacity`、`knowledge`、降低维护压力 |
| 全国供应链网络 | 腾飞时代 | 大范围扩张必备 | `capacityMultiplier`、`demandMultiplier` |
| ERP 生产中台 | 新潮时代 | 数字化调度 | `knowledge`、`inventoryCostReduction`、`competitivenessMultiplier` |
| 智能制造单元 | 新潮时代 | 高阶效率项目 | `capacity`、`unitProfit`、`compliance` |
| 柔性定制工厂 | 新潮时代 | 满足个性需求 | `unitProfit`、`competitivenessMultiplier` |

### 7.2 人事团队（建议扩展清单）

人事类项目负责提升幸福、知识、组织效率和长期竞争力。它们通常不会立刻带来大额收入，但会让整个系统更稳定。

| 项目名 | 所属时代 | 主要定位 | 建议主效果 |
| --- | --- | --- | --- |
| 师徒带教 | 春风时代 | 创业初期经验传承 | `knowledge`、`happiness`、少量 `competitiveness` |
| 培训体系 | 春风时代 | 让经验可复制 | `knowledge`、`network`、`demandMultiplier` |
| 优质薪酬福利 | 浪潮时代 | 稳团队 | `happiness`、轻微 `maintenanceDelta` |
| 职业技能计划 | 浪潮时代 | 提升基层执行效率 | `knowledge`、`capacityMultiplier` |
| 年终奖金制度 | 浪潮时代 | 高峰期稳士气 | `happiness`、`reputation` |
| 进阶培训 | 浪潮时代 | 从熟练到专业 | `knowledge`、`competitivenessMultiplier` |
| 员工宿舍 | 浪潮时代 | 降低流失 | `happiness`、`reputation` |
| 员工食堂 | 浪潮时代 | 稳定幸福与健康 | `happiness`、少量 `fixed_expense` 抵消 |
| 健身活动中心 | 崛起时代 | 中后期文化建设 | `happiness`、`reputation` |
| 员工工会 | 崛起时代 | 提升稳定与协商能力 | `happiness`、`compliance` |
| 心理咨询室 | 崛起时代 | 缓和高压扩张副作用 | `happiness`、降低事件负面影响 |
| 人才招聘会 | 崛起时代 | 加快组织扩张 | `network`、`knowledge` |
| 校企合作 | 崛起时代 | 培养稳定人才来源 | `knowledge`、`reputation` |
| 创新实验室 | 腾飞时代 | 打开研发曲线 | `knowledge`、`unitProfit`、`competitivenessMultiplier` |
| AI 助手系统 | 新潮时代 | 提高信息处理效率 | `knowledge`、`capacityMultiplier`、`demandMultiplier` |
| 企业大学 | 新潮时代 | 大型组织内训 | `knowledge`、`network`、`reputation` |
| 股权激励计划 | 新潮时代 | 锁住核心团队 | `happiness`、`reputation`、长期 `competitiveness` |

### 7.3 市场营销（建议扩展清单）

营销类项目负责拉升需求、品牌曝光、复购能力与渠道深度。它既包括广告，也包括门店、会员、线上入口等“渠道资产”。

| 项目名 | 所属时代 | 主要定位 | 建议主效果 |
| --- | --- | --- | --- |
| 街边招牌 | 春风时代 | 最基础曝光 | `demand`、`reputation` |
| 本地广告牌 | 春风时代 | 地域知名度提升 | `demand`、`competitiveness` |
| 广播电台广告 | 浪潮时代 | 扩大区域触达 | `demandMultiplier`、`network` |
| 传统印刷推广 | 浪潮时代 | 大众传播补充 | `demand`、`reputation` |
| 地摊摊位 | 春风时代 | 快速验证市场 | `demand`、现金流导向 |
| 本地门店 | 浪潮时代 | 建立实体触点 | `unitProfit`、`reputation` |
| 连锁门店 | 腾飞时代 | 品牌规模化 | `demandMultiplier`、`reputation` |
| 外贸出口 | 崛起时代 | 扩展市场边界 | `demand`、`network`、`unitProfit` |
| 电视广告投放 | 浪潮时代 | 高成本高声量 | `demand`、`reputation`、`unitProfit` |
| TM 品牌注册 | 崛起时代 | 品牌护城河 | `reputation`、`compliance` |
| VIP 会员体系 | 崛起时代 | 拉复购 | `unitProfit`、`happiness`、`competitivenessMultiplier` |
| 会员积分 App | 新潮时代 | 精细化运营 | `demandMultiplier`、`unitProfit` |
| 微信小程序 | 新潮时代 | 轻入口获客 | `demand`、`competitiveness` |
| 电商旗舰店 | 腾飞 / 新潮 | 线上销售主阵地 | `demand`、`unitProfit` |
| 跨境电商平台 | 新潮时代 | 国际扩张轻渠道 | `demandMultiplier`、`network` |
| 社区团购合作 | 新潮时代 | 高频低成本分发 | `demand`、`capacity` 协同 |
| 私域社群运营 | 新潮时代 | 提升留存 | `happiness`、`unitProfit`、`demandMultiplier` |
| KOL 联名推广 | 新潮时代 | 品牌爆发型增长 | `reputation`、`demand`，伴随高维护 |

### 7.4 现有项目如何映射到扩展内容

当前 `projects.json` 中的 12 个项目可以作为第一批可玩的 MVP 内容保留：

- 生产：`个体餐饮小店`、`来料加工车间`、`乡镇工厂`、`连锁中央厨房`；
- 人事：`师徒带教`、`培训体系`、`员工宿舍`、`技术实验室`；
- 营销：`街边招牌`、`本地电台广告`、`电视广告投放`、`会员体系`。

建议下一阶段直接在 `projects.json` 中继续扩表，而不是推翻现有结构。

---

## 八、事件、新闻与叙事系统

### 8.1 事件系统分层

建议把事件分成三层：

1. **主线时代事件**：按年份强触发，塑造历史感；
2. **经营状态事件**：由资金、幸福、声望、合规等触发；
3. **随机新闻事件**：短期 buff / debuff，增加局势波动。

### 8.2 当前原型已实现的事件结构

`events.json` 当前已支持：

- `Start`
- `DateReached`
- `MoneyBelow`
- `HappinessBelow`
- `ReputationAbove`

并且一个事件可以配置：

- 标题；
- 描述；
- 多个选项；
- 每个选项的数值效果。

这是非常适合继续扩展的结构。建议新增以下字段：

- `icon`：事件图标；
- `category`：政策 / 市场 / 团队 / 危机 / 机遇；
- `cooldownMonths`：避免类似事件连续刷脸；
- `followUpEventId`：形成事件链；
- `newsHeadline`：在新闻系统里同步生成标题。

### 8.3 建议新增事件题材

#### 历史 / 政策类

- 个体经营放宽；
- 承包责任制红利；
- 价格改革窗口；
- 南方市场信心提振；
- 入世机遇；
- 金融危机冲击；
- 移动互联网爆发。

#### 经营危机类

- 现金流断裂预警；
- 核心员工离职；
- 食品 / 质量 / 服务事故；
- 监管抽查；
- 供应链延误；
- 舆论负面。

#### 正向机会类

- 大客户订单；
- 媒体报道；
- 地方扶持政策；
- 高校合作；
- 爆款营销内容出圈。

### 8.4 新闻系统设计

新闻系统是 demo 氛围感的重要补强项，建议新增 `Data/news.json`，结构如下：

- `id`
- `title`
- `body`
- `triggerType`
- `triggerValue`
- `durationMonths`
- `effects`

新闻和事件的区别在于：

- 事件需要玩家做决定；
- 新闻通常只通知，不需要中断游戏；
- 新闻可以带来短期市场修正，比如 6 个月内需求 +8%，或线下渠道 -10%。

---

## 九、成就与策略评级系统

### 9.1 为什么必须做

截图中的“公司经营概况”弹窗里有明显的成就矩阵，这意味着游戏不仅要让玩家赚钱，还要告诉玩家：

- 你是怎么赢的；
- 你的公司像什么类型；
- 你解锁了哪些时代节点。

### 9.2 成就分类建议

建议至少 30 个成就，分为 6 类：

1. **创业起步**：第一笔盈利、第一家门店、第一家工厂；
2. **产能扩张**：总产能达到 100 / 300 / 600；
3. **人才建设**：幸福 > 80、知识 > 60、连续 12 个月未出现低士气；
4. **品牌增长**：声望 > 50、会员体系成型、电视广告完成；
5. **风险管理**：连续 24 个月不触发现金流危机、合规 > 70；
6. **时代里程碑**：跨入新年代、完成关键历史事件链。

### 9.3 数据结构建议

新增 `Data/achievements.json`：

- `id`
- `name`
- `description`
- `category`
- `unlockCondition`
- `score`

### 9.4 策略评级算法建议

概况页给出一个总分和流派标签，例如：

- “现金流大师”
- “渠道扩张派”
- “组织经营派”
- “技术效率派”
- “均衡型企业家”

建议评分构成为：

- 利润表现：35%
- 增长速度：20%
- 品牌与声望：15%
- 团队幸福与组织稳定：15%
- 资产多样化：15%

如果想更贴近 demo，可以额外给出 1-2 条文字总结，例如：

- “你擅长在市场窗口期迅速放大规模，但团队承压较高。”
- “你不是跑得最快的人，但你建立了更稳固的长期结构。”

---

## 十、UI 与交互实现细节

### 10.1 推荐界面风格

从截图看，UI 视觉关键词应为：

- 深色底；
- 金色描边；
- 霓虹感弱高亮；
- 复古 + 科技混搭；
- 高信息密度但排版规整。

### 10.2 顶部栏交互

顶部栏建议包含以下按钮：

- 时间控制：`暂停`、`单步`、`1x/2x/4x`；
- 弹窗入口：`概况`、`新闻`、`指南`、`排行`、`保存`；
- 可选：`统计`、`设置`、`返回主菜单`。

### 10.3 中部经营版图交互

建议不是只保留 `TabContainer + GridContainer`，而是增加一个场景化 Board：

- 每个板块都有一个独立的背景区域；
- 项目被激活后，在场景中生成对应的“节点建筑”；
- 点击场景节点，底部卡片自动聚焦；
- 未解锁节点可显示轮廓或锁图标；
- 节点之间可用线条表示“前置关系”或“协同关系”。

### 10.4 投资卡片交互

每张卡片建议显示：

- 名称；
- 当前等级 / 最大等级；
- 当前升级成本；
- 升级后的维护费；
- 核心增益摘要；
- 解锁条件；
- ROI 或“推荐指数”；
- 点击后即时刷新预测收益。

### 10.5 弹窗交互

#### 事件弹窗

- 大标题；
- 描述；
- 选项按钮；
- 每个选项下附 1 行效果说明；
- 事件期间禁用继续推进时间。

#### 概况弹窗

- 左上：经营概要；
- 右上：市场引擎；
- 中段：成就矩阵；
- 底部：策略评级与一句总结。

#### 新闻弹窗

- 时间线列表；
- 新闻分类过滤；
- 当前生效中的市场修正标记。

---

## 十一、Godot 实现方案（与当前仓库对齐）

### 11.1 当前代码结构说明

当前项目已经采用非常合适的架构方向：**Autoload 单例 + UI 场景控制器**。

#### 已有核心脚本

1. `GameManager.cs`
	- 控制时间流逝；
	- 管理暂停、继续、步进、倍速；
	- 保存 / 读取存档的入口。

2. `DataManager.cs`
	- 持有 `SimulationState`；
	- 加载 `projects.json`；
	- 执行预测与月度结算；
	- 管理项目投资与解锁逻辑。

3. `EventManager.cs`
	- 加载 `events.json`；
	- 监听月结；
	- 判断是否触发事件；
	- 处理事件选项结果。

4. `SimulationModels.cs`
	- 定义所有核心 DTO / Enum；
	- 包括 `EraDefinition`、`ProjectDefinition`、`GameEventDefinition`、`SimulationState` 等。

5. `MainUIController.cs`
	- 缓存 UI 节点；
	- 绑定按钮、刷新左侧数据；
	- 动态构建项目卡片；
	- 显示事件弹窗和日志。

### 11.2 推荐新增脚本

随着内容变多，建议把当前 `MainUIController.cs` 做职责拆分，避免后期变成“万能大总管”。推荐新增：

- `OverviewDialogController.cs`
  - 负责公司概况弹窗；
- `NewsManager.cs`
  - 处理新闻队列和新闻修正效果；
- `NewsDialogController.cs`
  - 展示新闻时间线；
- `AchievementManager.cs`
  - 管理解锁判断与成就分数；
- `RankingManager.cs`
  - 维护竞争对手排行；
- `BoardLayoutController.cs`
  - 处理中部空间版图节点；
- `GuideDialogController.cs`
  - 展示词条、玩法提示、时代说明。

### 11.3 推荐新增数据文件

在 `business-game/Data/` 下建议新增：

- `achievements.json`
- `news.json`
- `rank_companies.json`
- `guides.json`
- `board_layouts.json`
- `eras.json`（可选，用于把当前写死在 `DataManager` 的时代参数也配置化）

### 11.4 推荐扩展的数据结构

#### 对 `ProjectEffectSet` 的扩展建议

当前已有：

- `Capacity`
- `Demand`
- `Competitiveness`
- `UnitProfit`
- `MaintenanceDelta`
- `Reputation`
- `Network`
- `Knowledge`
- `Happiness`
- `DemandMultiplier`
- `CapacityMultiplier`
- `CompetitivenessMultiplier`
- `InventoryCostReduction`

建议新增：

- `Compliance`
- `BrandHeat`
- `InflationResistance`
- `RiskReduction`
- `EventResilience`
- `OfflineDemandBonus`
- `OnlineDemandBonus`

#### 对 `UnlockCondition` 的扩展建议

建议新增：

- `MinCompliance`
- `RequiredAchievementId`
- `RequiredAnyProjectIds`
- `ForbiddenEventId`

### 11.5 推荐场景树结构

```text
Main (Node2D)
├── MonthTimer (Timer)
├── CanvasLayer
│   └── RootUI (Control)
│       ├── TopBar
│       │   ├── DateLabel
│       │   ├── EraLabel
│       │   ├── PlayPauseButton
│       │   ├── StepButton
│       │   ├── SpeedButton
│       │   ├── OverviewButton
│       │   ├── NewsButton
│       │   ├── GuideButton
│       │   ├── RankingButton
│       │   └── SaveButton
│       ├── Body
│       │   ├── LeftSidebar
│       │   │   ├── StatsText
│       │   │   ├── FinanceText
│       │   │   ├── MarketText
│       │   │   └── RiskSummary
│       │   └── MainArea
│       │       ├── BoardViewport
│       │       ├── ProjectTabs
│       │       │   ├── ProductionTab
│       │       │   │   └── ProductionGrid
│       │       │   ├── HumanResourcesTab
│       │       │   │   └── HumanResourcesGrid
│       │       │   └── MarketingTab
│       │       │       └── MarketingGrid
│       │       └── FooterLog
│       │           └── LogText
│       └── PopupLayer
│           ├── EventDialog
│           ├── OverviewDialog
│           ├── NewsDialog
│           ├── GuideDialog
│           └── RankingDialog
└── AudioRoot
```

### 11.6 信号流建议

当前信号已经很好，建议保持并继续扩展：

- `GameManager.PlayStateChanged`
- `DataManager.StateChanged`
- `DataManager.ProjectPortfolioChanged`
- `DataManager.MonthResolved`
- `EventManager.ActiveEventChanged`

新增建议：

- `NewsManager.NewsTriggered`
- `AchievementManager.AchievementUnlocked`
- `RankingManager.RankingChanged`
- `BoardLayoutController.NodeSelected`

---

## 十二、存档、失败条件与结局设计

### 12.1 存档

当前原型已经把 `SimulationState` 序列化到 `user://business_game_save.json`，这个方向完全正确。

后续建议额外保存：

- 已解锁成就；
- 新闻状态与持续回合；
- 排行榜状态；
- 当前概况页评分缓存；
- 是否已播放某些一次性引导。

### 12.2 失败条件

建议不要“一破产就秒 Game Over”，而是采用更符合经营题材的弹性失败：

- 资金为负并持续 3 个月：破产结算；
- 合规过低并触发重大事故：强制停业结算；
- 幸福持续过低：团队崩盘结算；
- 也可提供“困难模式”下更严厉的失败规则。

### 12.3 结局

2018 年结束时，根据多个维度给玩家结局：

- 财富型结局；
- 品牌型结局；
- 稳健型结局；
- 科技型结局；
- 组织型结局；
- 均衡传奇结局。

结局建议由以下维度共同决定：

- 最终资金；
- 声望；
- 知识；
- 合规；
- 幸福；
- 成就分；
- 行业影响力。

---

## 十三、推荐的开发顺序

为了让项目快速从“原型”走向“可展示版本”，建议按以下顺序推进。

### 阶段 1：把当前原型打磨成完整可玩闭环

目标：保留现有逻辑，补足最关键缺口。

优先事项：

1. 统一时代显示名，与 demo 保持一致；
2. 增加 `合规` 指标；
3. 增加公司概况弹窗；
4. 扩充 `projects.json` 到至少 20+ 项目；
5. 扩充 `events.json` 到 10+ 事件。

### 阶段 2：补足 demo 的“可视化经营感”

优先事项：

1. 加入中部经营版图；
2. 卡片与场景节点联动；
3. 增加概况 / 新闻 / 指南 / 排行按钮；
4. 新增成就矩阵和策略评分。

### 阶段 3：增强时代氛围与深度

优先事项：

1. 新闻系统；
2. 通胀 / 政策 / 风险波动；
3. 竞争对手排行；
4. 多结局总结页。

---

## 十四、结论

当前仓库中的 Godot 原型已经具备一个非常好的基础骨架：

- `GameManager` 负责时间；
- `DataManager` 负责数值与项目；
- `EventManager` 负责剧情；
- `MainUIController` 负责界面刷新；
- `projects.json` 与 `events.json` 已经验证了“数据驱动”的方向。

接下来最重要的，不是推翻重做，而是**顺着这个骨架把 demo 中最吸引人的部分补齐**：

1. 补内容量；
2. 补概况 / 新闻 / 成就；
3. 补空间化经营版图；
4. 补时代波动与结局反馈。

只要沿着这条路线推进，这个项目完全可以从“能跑的经营原型”升级成“有主题、有节奏、有长线成长感的时代创业模拟游戏”。