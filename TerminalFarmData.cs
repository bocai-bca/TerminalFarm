using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TerminalFarm.TerminalFarmGame;

namespace TerminalFarm
{
    internal static class TerminalFarmData
    {
        internal static readonly TimeSpan FarmSlotWateringDefaultTimeSpan = new(0, 18, 0, 0);
        internal static readonly TimeSpan AppleTreeDropDefaultTimeSpan = new(0, 6, 0, 0);
        internal struct ItemProperties
        {
            internal string Name { get; set; } //物品的snake_case名称，用于翻译标识符
            internal ConsoleColor TextColor { get; set; } //物品在控制台中显示名称所使用的颜色
            internal FarmSlotItemProperties FarmSlotProperties { get; set; } //物品的农田格子数据
            internal MarketItemProperties MarketItemProperties { get; set; } //物品的市场数据
        }
        internal struct FarmSlotItemProperties //每一个物品的农场格子数据
        {
            internal bool CanSwap { get; set; } //能否通过swap命令从农田取出
            internal bool CanUse { get; set; } //能否在农田使用use命令
            internal PrintSlotType SlotType { get; set; } //在农田格子中的默认显示框
            internal int NextStateID { get; set; } //在农田种植达到时间后变更为的下一个物品ID
            internal TimeSpan GrowTime { get; set; } //本阶段的生长时长，从种下后经过该时长将变为下一个物品ID
            internal int SwapToID { get; set; } //进行swap后的该格物品ID
        }
        internal struct MarketItemProperties //每一个物品的市场数据，商店上架物品时使用物品市场价的4倍
        {
            internal int MaxPrice { get; set; } //最大价值
            internal int MinPrice { get; set; } //最小价值
            internal float SeedMulti { get; set; } //种子乘法器
            internal int SeedOffset { get; set; } //种子偏移器
            internal readonly int NowPrice
            {
                get
                {
                    Random tempRandom = new(Convert.ToInt32((DateTime.Now.Date - DateTime.MinValue).Days * SeedMulti) + SeedOffset);
                    return tempRandom.Next(MinPrice, MaxPrice);
                }
            }
            internal readonly int NextDayPrice
            {
                get
                {
                    Random tempRandom = new(Convert.ToInt32((DateTime.Now.Date.AddDays(1.0D) - DateTime.MinValue).Days * SeedMulti) + SeedOffset);
                    return tempRandom.Next(MinPrice, MaxPrice);
                }
            }
        }
        internal static readonly List<int> StoreItemsIDPage1 = [6, 10, 14, 17, 20, 24, 28];
        internal static readonly List<int> StoreItemsIDPage2 = [2, 3, 4, 5, 39];
        internal static readonly Dictionary<int, ItemProperties> ItemsProperties = new()
        {
            { 0, new ItemProperties
                {
                    Name = "air",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.InputOnly, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use
						NextStateID = 0, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //空槽位
			{ 1, new ItemProperties
                {
                    Name = "apple",
                    TextColor = ConsoleColor.Red,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.OutputOnly, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 1, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //苹果
			{ 2, new ItemProperties
                {
                    Name = "watering_can",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.None, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 2, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //浇水壶
			{ 3, new ItemProperties
                {
                    Name = "shovel",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.None, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 3, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 400,
                        MinPrice = 375,
                        SeedMulti = 1.0f,
                        SeedOffset = 12
                    }
                }
            }, //铲子
			{ 4, new ItemProperties
                {
                    Name = "big_watering_can",
                    TextColor = ConsoleColor.Cyan,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.None, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 4, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //三格浇水壶
			{ 5, new ItemProperties
                {
                    Name = "super_watering_can",
                    TextColor = ConsoleColor.Yellow,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.None, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 5, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //全版面浇水壶
			{ 6, new ItemProperties
                {
                    Name = "tomato_seed",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 7, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 3, 50, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //西红柿种子(反复结果)
			{ 7, new ItemProperties
                {
                    Name = "tomato_seedling",
                    TextColor = ConsoleColor.DarkGreen,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 8, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 13, 10, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //西红柿幼苗
			{ 8, new ItemProperties
                {
                    Name = "tomato_plant",
                    TextColor = ConsoleColor.DarkGreen,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 9, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 21, 35, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //西红柿植株
			{ 9, new ItemProperties
                {
                    Name = "tomato",
                    TextColor = ConsoleColor.DarkRed,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 9, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //西红柿
			{ 10, new ItemProperties
                {
                    Name = "potato_seed",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 11, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 5, 20, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //土豆种子(一次性)
			{ 11, new ItemProperties
                {
                    Name = "potato_sprout",
                    TextColor = ConsoleColor.Yellow,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 12, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 10, 45, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //土豆幼芽
			{ 12, new ItemProperties
                {
                    Name = "potato_plant",
                    TextColor = ConsoleColor.DarkGreen,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 13, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 12, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //土豆植株
			{ 13, new ItemProperties
                {
                    Name = "potato",
                    TextColor = ConsoleColor.Yellow,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 13, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //土豆
			{ 14, new ItemProperties
                {
                    Name = "cabbage_seed",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 15, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 5, 25, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //卷心菜种子(一次性)
			{ 15, new ItemProperties
                {
                    Name = "cabbage_seedling",
                    TextColor = ConsoleColor.DarkGreen,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 36, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 16, 30, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //卷心菜幼苗
			{ 36, new ItemProperties
                {
                    Name = "cabbage_plant",
                    TextColor = ConsoleColor.Green,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 16, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(2, 0, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //卷心菜植株
			{ 16, new ItemProperties
                {
                    Name = "cabbage",
                    TextColor = ConsoleColor.Green,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 16, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //卷心菜
			{ 17, new ItemProperties
                {
                    Name = "carrot_seed",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 18, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 3, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //胡萝卜种子(一次性)
			{ 18, new ItemProperties
                {
                    Name = "carrot_sprout",
                    TextColor = ConsoleColor.DarkGreen,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 37, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 6, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //胡萝卜幼芽
			{ 37, new ItemProperties
                {
                    Name = "carrot_plant",
                    TextColor = ConsoleColor.DarkGreen,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 19, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 19, 5, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //胡萝卜植株
			{ 19, new ItemProperties
                {
                    Name = "carrot",
                    TextColor = ConsoleColor.DarkYellow,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 19, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //胡萝卜
			{ 20, new ItemProperties
                {
                    Name = "strawberry_seed",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 21, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 18, 35, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //草莓种子(反复结果)
			{ 21, new ItemProperties
                {
                    Name = "strawberry_seedling",
                    TextColor = ConsoleColor.DarkGreen,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 22, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(1, 12, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //草莓幼苗
			{ 22, new ItemProperties
                {
                    Name = "strawberry_plant",
                    TextColor = ConsoleColor.Green,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 23, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(1, 3, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //草莓植株
			{ 23, new ItemProperties
                {
                    Name = "strawberry",
                    TextColor = ConsoleColor.Red,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 23, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //草莓
			{ 24, new ItemProperties
                {
                    Name = "blueberry_seed",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 25, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 18, 35, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //蓝莓种子(反复结果)
			{ 25, new ItemProperties
                {
                    Name = "blueberry_seedling",
                    TextColor = ConsoleColor.DarkGreen,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 26, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(1, 12, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //蓝莓幼苗
			{ 26, new ItemProperties
                {
                    Name = "blueberry_plant",
                    TextColor = ConsoleColor.Green,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 27, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(1, 3, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //蓝莓植株
			{ 27, new ItemProperties
                {
                    Name = "blueberry",
                    TextColor = ConsoleColor.Blue,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 27, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //蓝莓
			{ 28, new ItemProperties
                {
                    Name = "flower_seed",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 29, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(0, 20, 30, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //花种
			{ 29, new ItemProperties
                {
                    Name = "flower_seedling",
                    TextColor = ConsoleColor.Green,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmGrowing, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 38, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = new(4, 6, 0, 0) //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //花苗
			{ 30, new ItemProperties
                {
                    Name = "red_flower",
                    TextColor = ConsoleColor.Red,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 30, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //红花
			{ 31, new ItemProperties
                {
                    Name = "yellow_flower",
                    TextColor = ConsoleColor.Yellow,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 31, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //黄花
			{ 32, new ItemProperties
                {
                    Name = "green_flower",
                    TextColor = ConsoleColor.Green,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 32, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //绿花
			{ 33, new ItemProperties
                {
                    Name = "cyan_flower",
                    TextColor = ConsoleColor.Cyan,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 33, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //青花
			{ 34, new ItemProperties
                {
                    Name = "blue_flower",
                    TextColor = ConsoleColor.Blue,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 34, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //蓝花
			{ 35, new ItemProperties
                {
                    Name = "magenta_flower",
                    TextColor = ConsoleColor.Magenta,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.FarmCollectable, //物品在农田的默认边框
						CanSwap = true, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 35, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //紫花
			{ 38, new ItemProperties
                {
                    Name = "complete_flower",
                    TextColor = ConsoleColor.White,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.None, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 38, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.Zero //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 0,
                        MinPrice = 0,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //成花抽花ID
			{ 39, new ItemProperties
                {
                    Name = "consumable_water_pack",
                    TextColor = ConsoleColor.DarkCyan,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.None, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = true, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 39, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 28,
                        MinPrice = 22,
                        SeedMulti = 1.0f,
                        SeedOffset = 12
                    }
                }
            }, //一次性浇水包
			{ 40, new ItemProperties
                {
                    Name = "store_restock",
                    TextColor = ConsoleColor.Gray,
                    FarmSlotProperties = new FarmSlotItemProperties
                    {
                        SlotType = PrintSlotType.OutputOnly, //物品在农田的默认边框
						CanSwap = false, //物品作为农场格子时是否允许被swap至缓存
						CanUse = false, //物品作为缓存格子时是否允许被use进农田
						NextStateID = 40, //物品下一个生长阶段的ID，如果没有下一个生长阶段就填自身
						SwapToID = 0, //进行swap后本格变成什么
						GrowTime = TimeSpan.MaxValue //物品从本阶段开始生长到下一个阶段所需的时间
					},
                    MarketItemProperties = new MarketItemProperties
                    {
                        MaxPrice = 100,
                        MinPrice = 100,
                        SeedMulti = 1.0f,
                        SeedOffset = 0
                    }
                }
            }, //商店重新补货
		};
    }
}
