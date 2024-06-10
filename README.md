# 🎉欢迎  
终端农场，一个命令行种田游戏。  
游戏灵感启发自 **半屏挂机农场游戏《[Rusty's Retirement](https://store.steampowered.com/app/2666510/Rustys_Retirement/)》** 的前期低操作挂机农场设计和 **高难度多人合作恐怖射击游戏《[GTFO](https://store.steampowered.com/app/493520/GTFO/)》** 的用于查询信息和查找与定位游戏关键物品、资源道具的以命令行的形式交互的终端电脑。  
本身通过输入命令进行的游戏就十分不可思议了，更何况是让人出乎意料的种田要素？大胆地将这两个似乎无论如何都无法搭上边的标签结合在了一起，这就是这款富有**实验性**意味的游戏—— **《终端农场》** 。  
### ⚠玩前须知，本游戏含有以下可能劝退人的特殊要素  
- 完全没有鼠标操作(点叉叉关掉窗口和拖动框选文本等不算)，故在本游戏中执行操作只对少部分人来说轻松惬意  
- 游戏中实际使用到的命令种类很少，而且几乎不存在语法一说，重复地使用相同命令甚至相同参数可能令人乏味(这也算是一点实验尝试，因为GTFO能使用到的命令种类和语法复杂度也很少很简单)  
- 虽然是离线游戏，但作物生长、市场价格涨跌、商店进货等游戏机制，仍与现实时间关联，游戏中大部分时间都处于看不到作物何时才有变化的等待时间(除非翻开存档文件或者游戏源数值)  
- 游戏的操作量很少而发展速度又很慢，有时可能一天只需要打开一次然后输两条命令就关掉了  
- 没有自动化，也没有会提升玩家操作效率的功能。几乎体验不到升级感

# ⌨️操作方式  
*本章节仅适用于对控制台操作不了解的玩家，命令及其作用将在后续介绍，如果您是开发者、管理员等熟悉命令行控制台操作概念的玩家，可跳过此章节*  
游戏的实际操作只需要用到键盘，鼠标可以用来拖曳选中文本或右键粘贴内容。  
打字输入特定内容并发送给游戏，就可以执行对应操作，而具体要输入什么将在后续介绍。  
不要输入中文、日语、韩语等语言！请用英语输入法玩这个游戏。  
可以使用上下方向键来选择填充之前发送的命令，使用左右方向键来移动光标。可以使用Home和End来跳到首和尾。  
翻页可以使用PageUp和PageDown，不过这里倒是比较推荐用鼠标滚轮了。  

# 📑命令与命令列表  
(如果看着头大，可以跳过本章节先去看玩法介绍，在玩法介绍中也有一部分对命令作用的介绍，之后再回到本章节就容易理解了)  

《终端农场》对大小写不敏感。  
命令列表的介绍中，部分命令后面会跟上一个括号，括号中的内容是该命令的简写，在游戏中可以直接使用简写替代全写，其余部分语法不变。  
命令列表的介绍中，用```<```和```>```包裹起来的部分是参数，该部分填写什么是不固定的，由玩家根据自己的意图来填写。每个参数都会有属于它的介绍，在实际输入时请不要带```<```和```>```符号。  
以```📜```开头的命令在执行过后会进行保存。  
## 📂命令列表  
### 📄CLEAR  
清空控制台屏幕上的所有内容  
**```CLEAR```**  
　清空屏幕  
### 📜EXIT  
退出游戏  
**```EXIT```**  
　退出游戏  
### 📜GOTO(CD)  
进行游戏场景相关的操作  
**```GOTO```**  
　显示所有场景  
**```GOTO <场景>```**  
　前往特定场景  
　```<场景>```: 一个场景的名称或其首字母  
> 示例:  
> 　```GOTO FARM```: 前往农场  
> 　```GOTO F```: 前往农场  
### 📄HELP  
显示帮助信息和命令用法  
**```HELP```**  
　显示帮助信息  
**```HELP <命令>```**  
　显示一条命令的用法  
　```<命令>```: 一个命令的全写或简写  
> 示例:  
> 　```HELP SWAP```: 显示命令SWAP的用法  
> 　```HELP DIR```: 显示命令PAGE的用法  
> 　```HELP D```: 显示命令USE的用法  
### 📜LANG  
进行显示语言相关的操作  
**```LANG```**  
　显示所有可用语言  
**```LANG <语言序号>```**  
　切换到指定语言  
　```<语言序号>```: 一个语言的数字序号  
> 示例:  
> 　```LANG 0```: 切换到英语  
> 　```LANG 1```: 切换到中文  
### 📄LIST(LS)  
列出特定列表  
**```LIST```**  
　显示所有可列出的列表  
**```LIST <列表名称>```**  
　列出特定列表  
　```<列表名称>```: 一个列表名称全称或者首字母  
> 示例:  
> 　```LIST SCENES```: 显示所有可以用GOTO去的场景  
> 　```LIST C```: 显示所有命令  
> 　```LIST U```: 显示所有场景的升级信息  
### 📜LOAD  
读取存档  
**```LOAD <路径>```**  
　从指定路径处读取存档  
　```<路径>```: 一个指向文件TerminalFarmSave.json的绝对路径  
> 示例:  
> 　```LOAD D:\Example\TerminalFarmSave.json```: 从该示例路径读取存档  
> 错误示例:  
> 　```LOAD D:\Example\123.json```: 失败，必须指向名字是TerminalFarmSave.json的文件(这一机制要求可能会在未来更新中取消)  
### 📜PAGE(DIR)  
显示场景内容和翻页  
**```PAGE```**  
　显示场景内容  
**```PAGE <页码>```**  
　翻页并显示该页内容  
　```<页码>```: 页码数字  
> 示例:  
> 　```PAGE 2```: 显示第二页的内容  
### 📜SAVE  
手动保存存档  
**```SAVE```**  
　显示当前存档保存路径  
**```SAVE RESET```**  
　重置存档保存路径为默认路径  
**```SAVE <路径>```**  
　设置自定义存档保存路径  
　```<路径>```: 一个指向已存在目录的绝对路径  
> 示例:
> 　```SAVE D:\Custom\```: 设置保存位置为该示例路径。但如果失败，有时(例如被其他程序占用)可能不会自动回退到默认路径。当保存完成时，以后每次打开游戏都会自动读取该路径的存档  
### 📜SWAP(S)  
使手持槽与当前所在场景的指定格子交换物品  
**```SWAP [<索引>]```**  
　与当前场景的当前页面上由索引指定的格子交换物品  
　```<索引>```: 格子索引序号  
> 示例:  
> 　```SWAP 0```: 与当前场景的第一格交换物品  
> 　```SWAP 5```: 与当前场景的最后一格交换物品  
> 　```SWAP```: 在市场和苹果树中，不可以带索引数字，直接单独SWAP来卖掉物品或者捡苹果  
### 📜UPGRADE(UPG)  
升级场景  
**```UPGRADE```**  
　升级当前场景(没有确认环节，当心使用)  
### 📜USE(D)  
把手持槽的物品使用在当前所在场景的指定格子上  
**```USE <索引>```**  
　使用手持槽物品。可以使用此命令的物品有: 各种种子、铲子、各种是水壶、一次性浇水包  
　```<索引>```: 格子索引序号  
> 示例:  
> 　```USE 2```: 在当前场景的第三格交换物品  

# 🎮玩法  
《终端农场》的核心玩法可以用三点总结: 种植作物、集齐所有收集品、蹲市场卖高价。  
游戏中以不同场景来划分玩家可使用的功能板块，每个场景都有自己独有的功能，有的与经济相关、有的与存储相关。  
命令具有切换场景和与场景交互的能力，只要理解了命令的功能，就可以把脑袋里所想的一切意图转变成在游戏中进行对应行动的操作。  
在游戏中，玩家可以进行的行动，小到从苹果树收集苹果，中到购买水壶浇灌农田，大到花钱升级仓库容量。  
游戏的游玩历程即是从一开始只有寥寥几百块钱的钱包，从每一步实打实地种下种子到收获，到观察市场价格等到作物高价时卖出，到每天打开商店查看有没有上架花种，再到攒够了钱升级了许多场景，最后心满意足地看着花园里白色花盆上摆满了颜色不重复的花朵。最后一步的完成意味着《终端农场》的全部内容均体验到位了。  
当然了，这一过程可能甚至需要一年之久。因此，《终端农场》不适合当作一个热度上手游戏来玩，它需要玩家拥有无比持久的耐心，或者说需要玩家几乎不在乎游戏的发展。不妨把它放在开始菜单置顶固定项，对于常用电脑的玩家来说，每天在大脑放松之余打开一下农场，看看一切有没有变化。  
当然了×2，以上内容和美好的成就感幻想仅限于玩家没有作弊。通过查看源代码或者编辑存档，各种规模的作弊都是可以轻松实现的，而《终端农场》对一些玩家来说是一种等待的折磨，萌生作弊之心非常能理解。不过作者推荐为了保留体验原味《终端农场》的一个后路，建议想要作弊的玩家活用多存档备份的方法，给自己保留好没有作弊过的存档，与其他存档并行游玩。  
## ✏输入提示
在玩家的输入光标之前有一个前缀，用来指示手持的物品和所在的场景和所在页码。以便每次输入命令之前检查自己所处的环境是不是符合预期的。  
```[手持槽]所在场景.页码>```  
## 🎞️场景一览  
### 📦仓库(Inventory)  
用于存放物品的场景，你可以在干活时在仓库中暂时放下物品，或者在仓库中长期存放物品。  
在仓库中使用```SWAP```命令来在手持槽与指定仓库格子交换物品。  
仓库每页有6个格子。仓库初始状态下只有一页，通过升级可以获得更多页面。满级时总共拥有30个格子。  
### 🌾农田(Farm)  
能够种下种子并使其成长的场景，但是需要给格子浇水才能让作物生长。  
在农田中手持种子使用```USE```命令来在指定格子种下种子，手持浇水壶、浇水包使用```USE```来给指定格子浇水(不同浇水道具的效果和用法将在后续具体介绍)，手持铲子使用```USE```来铲除指定格子的作物。  
通过升级，可以提升农田格子的保湿能力。初始状态下格子能保湿18小时，满级时能保湿将近2天。  
### 🏪商店(Store)  
用来购买物品的场景，分第一页和第二页。第一页会在每天早上8点给空格子随机上架作物种子，已经有商品的格子不会被更新，如果没有满意的商品可以购买第二页的"重新补货"；第二页的物品是固定的工具商品，可以无限购买，其价格会在每天早上八点变化。  
在商店中空手使用```SWAP```命令来购买当前页面指定格子的物品。  
升级可以降低购买"重新补货"所需的费用。默认是200，满级为100。  
### 🌷花园(Garden)  
游戏中有六种颜色的花朵作为收集品，花园即是陈列收集品的地方。  
使用```SWAP```命令可以将花朵放置在花园中。  
花园的升级是免费的，但是需要将其他所有场景都升满级才能解锁。花园升级后花盆会变成白色。一个白色花盆、放满六种不同颜色的花朵的花园，即是玩家通关《终端农场》的证明。  
### 💹市场(Market)  
用来查看市场价格和出售物品的场景。  
市场每天会更新物品价格，在市场列表上的物品是可以出售的(出售种子没有收入)。  
在市场任意页面手持物品使用```SWAP```命令即可出售，即使不是在对应物品所在的页面也没关系。  
升级市场后可以查看明天的价格。  
### 🍎苹果树(Tree)  
会自动产出苹果的场景，苹果用来卖钱。这是用来在钱包里一点资金都没有的情况下缓慢产钱防止彻底没得玩的功能，平时苹果也可以作为一份很小的收入。  
在苹果树空手使用```SWAP```命令来拾取苹果。  
升级苹果树可以增加苹果收集行数。  
## 🌱种田  
种子只能从商店购买获得，并不能将作物产出的成品用作种子。游戏中有六种主要作物，分别是: 西红柿、马铃薯、胡萝卜、卷心菜、草莓、蓝莓。  
将种子种在农田之后需要对其浇水，湿润的格子的括号会呈现深蓝色，反之为灰白色。生长具有不展示给玩家的精确计时进度，只有在湿润状态下该进度才会增加，最终成长到成品作物。  
不同作物的生长时间各不相同。有的作物是能够重复产出成品的，这些作物被收集成品以后会回退到它之前的生长阶段，然后再次长出成品。这样的作物有: 西红柿、草莓、蓝莓。  
但总之，玩家只需要种下作物然后不断浇水，最后收集成品即可。  
## 🌻种花  
种花与种其他作物差不多，也是种下花种不断浇水即可，但是花种会种出不同颜色的花，这个颜色是随机的，在花朵成长到成品阶段前是不可知的。  
## 💰购物与售卖  
在商店购买物品，在市场出售物品。物品的价格是会变化的，而且高度随机、无法预测，涨跌没有规律、变化或小或大，甚至可能今天是最低价明天是最高价。  
可以通过购买市场升级来查看第二天的价格，以便安排合适的购入或出售策略。  
## ⏫升级场景  
移动到想要升级的场景后输入```UPGRADE```或```UPG```即可升级场景，升级场景需要花费资金。关于场景升级的具体信息可以输入```LIST UPGRADES```或```LS U```查看。  