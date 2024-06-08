using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Xml.Schema;
using static TerminalFarm.TerminalFarmData;

namespace TerminalFarm
{
	internal static class TerminalFarmGame
	{
		internal const int VersionBVH = 1002;
		internal enum PrintMessageLevel //输出信息警告规则
		{
			Info = 0, //一般信息。有时可能会返回无意中输出的异常
			Warning = 1, //执行输入的命令时有问题，但被已有应对手段无害处理
			Error = 2, //运行过程出现故障，但被已有应对手段无害处理
			Fatal = 3 //输出程序本身通过异常处理语句捕获到的异常
		}
		internal static Dictionary<int, string> DisplayLanguage = new()
		{
			{0, "en_" },
			{1, "zh_" }
		};
		internal static int CurrentScene = 0;
		internal static int CurrentPage = 0;
		internal static Dictionary<string, object>? MemoryGameData = GameDataInit();
		internal static Random mainRandom = new(); //主随机数
		internal static string MemoryCustomPath = "";
		internal static bool IsGameStillRunning = true; //用于在主循环中结束循环

        [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
        internal static void Main(string[] args) //入口点
		{
			//获取游戏翻译文本的资源管理器
			ResourceManager resMgrTranslatedText = new("TerminalFarm.TranslatedText", typeof(TerminalFarmGame).Assembly);
			//获取AppData\Roaming路径
			string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			bool wasReadOriginalSaveSuccess = true;
			Console.ForegroundColor = ConsoleColor.Red;
			if (File.Exists(appdataPath + "\\BCASoft\\TerminalFarm\\TerminalFarmSave.json")) //如果原初存档存在
			{
				//尝试读取原初存档到内存存档
				try
				{
					JsonDocument jsonDocu = JsonDocument.Parse(File.ReadAllText(appdataPath + "\\BCASoft\\TerminalFarm\\TerminalFarmSave.json"));
					JsonElement rootElement = jsonDocu.RootElement;
					//将原初存档合并到内存存档
					MemoryGameData = APIJson.MergeJsonDictionaries(MemoryGameData ?? [], APIJson.ParseJsonElement(rootElement));
					jsonDocu.Dispose();
				}
				catch (Exception ex)
				{
					Console.WriteLine("[TerminalFarm] Error : \n" + ex.ToString() + "\n");
					wasReadOriginalSaveSuccess = false;
				}
				if (wasReadOriginalSaveSuccess && MemoryGameData != null) //如果成功读取原初存档并且原初存档不为null
				{
					if (MemoryGameData.TryGetValue("CustomPath", out object? value)) //获取自定义路径值
					{
						string customPath = value.ToString() ?? "";
						if (customPath != "" && Path.Exists(customPath) && Path.GetFileName(customPath) == "TerminalFarmSave.json") //如果存档路径不为空且目标文件存在而且名字正确
						{
							bool wasReadCustomSaveSuccess = true;
							Dictionary<string, object>? tempGameData = null;
							//尝试读取自定义存档到内存存档
							try
							{
								JsonDocument jsonDocu = JsonDocument.Parse(File.ReadAllText(customPath));
								JsonElement rootElement = jsonDocu.RootElement;
								tempGameData = APIJson.ParseJsonElement(rootElement);
								jsonDocu.Dispose();
							}
							catch (Exception ex)
							{
								Console.WriteLine("[TerminalFarm] Error : \n" + ex.ToString() + "\n");
								wasReadCustomSaveSuccess = false;
							}
							if (wasReadCustomSaveSuccess && tempGameData != null) //如果自定义存档读取成功
							{
								MemoryGameData = APIJson.MergeJsonDictionaries(MemoryGameData, tempGameData);
								MemoryCustomPath = customPath; //缓存自定义路径
								MemoryGameData["CustomPath"] = customPath;
							}
						}
					}
				}
			}
			else
			{
				wasReadOriginalSaveSuccess = false;
			}
			if (!wasReadOriginalSaveSuccess)
			{
				wasReadOriginalSaveSuccess = true;
				try
				{
					MemoryGameData = GameDataInit();
					//如果原初存档不存在，就生成原初存档
					Directory.CreateDirectory(appdataPath + "\\BCASoft\\TerminalFarm\\");
					File.WriteAllText(appdataPath + "\\BCASoft\\TerminalFarm\\TerminalFarmSave.json", JsonSerializer.Serialize(MemoryGameData));
				}
				catch (Exception ex)
				{
					Console.WriteLine("[TerminalFarm] Error : \n" + ex.ToString() + "\n");
					wasReadOriginalSaveSuccess= false;
				}
				if (!wasReadOriginalSaveSuccess)
				{
					//存档读取且生成失败
					Console.WriteLine("[TerminalFarm] Fatal Error : \n" + @"Failed to launch game. Because failed to create the save file at path """ + appdataPath + "\\BCASoft\\TerminalFarm" + @""". Please check the permissions about that directory or ask for help." + "\n");
					Console.ResetColor();
					return;
				}
			}
			Console.ResetColor();
			string translatePrefix;
			if (true)
			{
				int key = 0; //语言前缀索引，也代表存档中UseLanguage
				bool forceLangByArgs = false;
				if (args.Contains<string>("-Chinese")) //启动项带-Chinese可以强制以中文启动
				{
					key = 1;
					forceLangByArgs = true; //记录当前由启动项强制指定语言
				}
				if (MemoryGameData != null) //防止空引用警告
				{
					if (MemoryGameData.TryGetValue("UseLanguage", out object? value)) //如果存档包含UseLanguage键
					{
						if (forceLangByArgs) //如果当前由启动项强制指定语言
						{
							MemoryGameData["UseLanguage"] = key; //将UseLanguage的值设为强制指定的语言
						}
						else if (value is int intValue && 0 <= intValue && intValue <= 1) //否则(当前不由启动项指定语言)如果value的类型正常且范围正常
						{
							key = (int)(value ?? key); //使用语言设为UseLanguage的值
						}
						else //否则(没有启动项强制指定语言且value的类型不正确或范围不正确)
						{
							MemoryGameData["UseLanguage"] = key; //将UseLanguage的值设为默认语言
						}
					}
					else //否则(不包含UseLanguage键)
					{
						MemoryGameData.Add("UseLanguage", key); //添加UseLanguage键值对
					}
				}
				translatePrefix = DisplayLanguage[key];
			}
			Console.WriteLine(String.Format(resMgrTranslatedText.GetString(translatePrefix + "welcome") ?? "", VersionBVH));
			TextTranslator translator = new(ref resMgrTranslatedText, translatePrefix);
			Console.Title = translator.Translate("game_title");
			bool waterSleep = false; //浇水时设为true，为true时每次等待输入之前会停止5秒
			while (IsGameStillRunning) //主循环开始
			{
				if (waterSleep)
				{
					waterSleep = false;
					Thread.Sleep(3000);
				}
				//显示输入提示箭头
				Console.Write("\n\n[");
				if (MemoryGameData != null)
				{
					int takingItemID = (int)MemoryGameData["TakingItemID"];
					if (takingItemID != 0)
					{
						Console.ForegroundColor = ItemsProperties[takingItemID].TextColor;
						Console.Write(translator.Translate("item_name_" + ItemsProperties[takingItemID].Name));
						Console.ResetColor();
					}
				}
				Console.Write("]" + translator.Translate("scene_name_" + (string)SceneData[CurrentScene]["SceneName"]) + "> ");
				//
				bool shouldSave = false; //命令执行完毕是否保存
				string[] inputSplited = (Console.ReadLine() ?? "").Trim().Split(' ', 2); //取得输入并分解成字符串数组
				bool hasArgs = inputSplited.Length > 1;
				bool needGotoPage = false; //正选需要在结尾时跳转到PAGE，跳转的代码由每个case自己编写
				switch (inputSplited[0].ToUpper()) //将输入的第一个词语大写化然后匹配检查
				{
					case "": //没输入
						break;
					case "CLS":
					case "CLEAR": //CLEAR命令
						Console.Clear();
						break;
					case "EXIT": //EXIT命令
						IsGameStillRunning = false;
						PrintMessage(translator.Translate("cmd_exit"), PrintMessageLevel.Info);
						shouldSave = true;
						break;
					case "HELP": //HELP命令
						shouldSave = false;
						if (hasArgs)
						{
							//如果有参数
							string arg = inputSplited[1].Split(' ', 2)[0].ToUpper();
							switch (arg)
							{
								case "EXIT":
									PrintMessage(translator.Translate("cmd_help_show_exit"), PrintMessageLevel.Info);
									break;
								case "CLS":
								case "CLEAR":
                                    PrintMessage(translator.Translate("cmd_help_show_clear"), PrintMessageLevel.Info);
                                    break;
								case "HELP":
									PrintMessage(translator.Translate("cmd_help_show_help"), PrintMessageLevel.Info);
									break;
								case "LANG":
									PrintMessage(translator.Translate("cmd_help_show_lang"), PrintMessageLevel.Info);
									break;
								case "LS":
								case "LIST":
									PrintMessage(translator.Translate("cmd_help_show_list"), PrintMessageLevel.Info);
									break;
								case "CD":
								case "GOTO":
									PrintMessage(translator.Translate("cmd_help_show_goto"), PrintMessageLevel.Info);
									break;
								case "DIR":
								case "PAGE":
									PrintMessage(translator.Translate("cmd_help_show_page"), PrintMessageLevel.Info);
									break;
								case "SAVE":
									PrintMessage(translator.Translate("cmd_help_show_save"), PrintMessageLevel.Info);
									break;
								case "S":
								case "SWAP":
									PrintMessage(translator.Translate("cmd_help_show_swap"), PrintMessageLevel.Info);
									break;
								case "LOAD":
									PrintMessage(translator.Translate("cmd_help_show_load"), PrintMessageLevel.Info);
									break;
								case "UPG":
								case "UPGRADE":
									PrintMessage(translator.Translate("cmd_help_show_upgrade"), PrintMessageLevel.Info);
									break;
								case "D":
								case "USE":
									PrintMessage(translator.Translate("cmd_help_show_use"), PrintMessageLevel.Info);
									break;
								default: //未知参数
									PrintMessage(String.Format(translator.Translate("cmd_help_unknown_arg"), arg), PrintMessageLevel.Info);
									break;
							}
						}
						else
						{
							//如果没参数
							PrintMessage(translator.Translate("cmd_help_text"), PrintMessageLevel.Info);
						}
						break;
					case "LANG": //LANG命令
						if (hasArgs)
						{
							//如果有参数，切换语言
							switch (inputSplited[1])
							{
								case "0": //切换至英语
									if (MemoryGameData != null)
									{
										MemoryGameData["UseLanguage"] = 0;
									}
									translator.prefix = DisplayLanguage[0];
									Console.WriteLine(translator.Translate("cmd_lang_switch_to_current_lang"));
									Console.Title = translator.Translate("game_title");
									shouldSave = true;
									break;
								case "1": //切换至中文
									if (MemoryGameData != null)
									{
										MemoryGameData["UseLanguage"] = 1;
									}
									translator.prefix = DisplayLanguage[1];
									Console.WriteLine(translator.Translate("cmd_lang_switch_to_current_lang"));
									Console.Title = translator.Translate("game_title");
									shouldSave = true;
									break;
								default: //参数无效
									PrintMessage(String.Format(translator.Translate("cmd_lang_invalid_language_index"), inputSplited[1]), PrintMessageLevel.Warning);
									break;
							}
						}
						else
						{
							//如果没参数，显示语言列表
							PrintMessage(translator.Translate("cmd_lang_list_all"), PrintMessageLevel.Info);
							Dictionary<int, string>.KeyCollection languagesKeys = DisplayLanguage.Keys;
							foreach (int key in languagesKeys)
							{
								PrintItem(key, resMgrTranslatedText.GetString(DisplayLanguage[key] + "language_name") ?? "", PrintSlotType.None);
							}
						}
						break;
					case "SAVE": //SAVE命令
						if (hasArgs)
						{
							//如果有参数
							string inputPath = inputSplited[1];
							if (inputPath.Equals("reset", StringComparison.CurrentCultureIgnoreCase)) //如果参数是reset，即重置为默认路径
							{
								MemoryCustomPath = "";
								if (MemoryGameData != null)
								{
									MemoryGameData["CustomPath"] = "";
								}
								PrintMessage(translator.Translate("cmd_save_reset"), PrintMessageLevel.Info);
								break;
							}
							inputPath = inputPath.Trim('\"'); //去除引号
							if (!Path.IsPathRooted(inputPath)) //如果路径不是合法的绝对路径
							{
								PrintMessage(translator.Translate("cmd_save_input_path_is_invalid"), PrintMessageLevel.Warning);
								break;
							}
							/*if (inputPath[^1] != '\\') //路径没有以反斜杠结尾，可能为一个文件路径
							{
								PrintMessage(translator.Translate("cmd_save_path_is_not_a_folder"), PrintMessageLevel.Warning);
								break;
							}*/
							if (!Directory.Exists(inputPath)) //目录不存在
							{
								PrintMessage(translator.Translate("cmd_save_target_folder_not_exist"), PrintMessageLevel.Warning);
								break;
							}
							if (inputPath[^1] != '\\') //路径没有以反斜杠结尾，就添加
							{
								inputPath += "\\";
							}
							inputPath += "TerminalFarmSave.json";
							MemoryCustomPath = inputPath;
							if (MemoryGameData != null)
							{
								MemoryGameData["CustomPath"] = inputPath;
							}
							PrintMessage(translator.Translate("cmd_save_success"), PrintMessageLevel.Info);
							shouldSave = true;
							break;
						}
						else
						{
							//如果没参数，显示当前存档位置
							string currentUsingPath;
							if (MemoryCustomPath == "") //没在用自定义路径
							{
								currentUsingPath = appdataPath + "\\BCASoft\\TerminalFarm\\TerminalFarmSave.json";
							}
							else //在用自定义路径
							{
								currentUsingPath = MemoryCustomPath;
							}
							PrintMessage(String.Format(translator.Translate("cmd_save_display_the_save_path_using"), currentUsingPath), PrintMessageLevel.Info);
						}
						break;
					case "LOAD": //LOAD命令
						if (hasArgs)
						{
							//如果有参数
							string inputPath = inputSplited[1];
							inputPath = inputPath.Trim('\"'); //去除引号
							if (!Path.IsPathRooted(inputPath)) //不是合法的绝对路径
							{
								PrintMessage(translator.Translate("cmd_load_input_path_is_invalid"), PrintMessageLevel.Warning);
								break;
							}
							if (Path.Exists(inputPath) && Path.GetFileName(inputPath) == "TerminalFarmSave.json") //路径存在且文件名为TerminalFarmSave.json
							{
								//尝试读取
								bool checkCustomSave = true;
								Dictionary<string, object>? tempGameData = null;
								try
								{
									JsonDocument jsonDocu = JsonDocument.Parse(File.ReadAllText(inputPath));
									JsonElement rootElement = jsonDocu.RootElement;
									tempGameData = APIJson.ParseJsonElement(rootElement);
									jsonDocu.Dispose();
								}
								catch (Exception ex)
								{
									PrintMessage(String.Format("[{0}] Error : \n", translator.Translate("game_title")) + ex.ToString() + "\n", PrintMessageLevel.Fatal);
									checkCustomSave = false;
								}
								if (checkCustomSave && tempGameData != null) //如果自定义存档读取成功
								{
									MemoryGameData = APIJson.MergeJsonDictionaries(MemoryGameData ?? [], tempGameData);
									MemoryCustomPath = inputPath; //缓存自定义路径
									MemoryGameData["CustomPath"] = inputPath;
									translator.prefix = DisplayLanguage[(int?)MemoryGameData["UseLanguage"] ?? 0];
									PrintMessage(translator.Translate("cmd_load_success"), PrintMessageLevel.Info);
									Console.Title = translator.Translate("game_title");
									shouldSave = true;
								}
								else //如果读取失败
								{
									PrintMessage(translator.Translate("cmd_load_failed"), PrintMessageLevel.Error);
								}
							}
							else //不存在或名字不为TerminalFarm.json
							{
								PrintMessage(translator.Translate("cmd_load_not_save_path"), PrintMessageLevel.Warning);
							}
						}
						else
						{
							//如果没参数，报错
							PrintMessage(translator.Translate("cmd_load_no_args"), PrintMessageLevel.Warning);
						}
						break;
					case "LS":
					case "LIST": //LIST命令
						shouldSave = false;
						if (MemoryGameData == null)
						{
							PrintMessage(translator.Translate("gamesave_null_error"), PrintMessageLevel.Error);
							break;
						}
						if (hasArgs)
						{
							//如果有参数
							string arg = inputSplited[1].ToUpper();
							switch (arg)
							{
								case "C":
								case "CMDS": //显示命令列表
									PrintMessage(translator.Translate("cmd_list_show_cmds"), PrintMessageLevel.Info);
									break;
								case "S":
								case "SCENES": //显示场景列表
									PrintMessage(translator.Translate("cmd_list_show_scenes"), PrintMessageLevel.Info);
									break;
								case "U":
								case "UPGRADES": //显示场景升级信息
									Console.Write("\n");
									for (int sceneID = 0; sceneID < 6; sceneID++){
										Console.WriteLine(translator.Translate("scene_name_" + SceneData[sceneID]["SceneName"]));
										Console.WriteLine(ListUpgradesMainLines(translator, sceneID, (List<object>)MemoryGameData["ScenesData"]));
										Console.WriteLine("  " + translator.Translate("cmd_list_upgrades_intro_" + SceneData[sceneID]["SceneName"]));
									}
									break;
								default: //其他
									PrintMessage(String.Format(translator.Translate("cmd_list_unknown_arg"), arg), PrintMessageLevel.Warning);
									break;
							}
						}
						else
						{
							//如果没参数，报提示信息
							PrintMessage(translator.Translate("cmd_list_no_args"), PrintMessageLevel.Info);
						}
						break;
					case "DIR":
					case "PAGE": //PAGE命令
						if (MemoryGameData == null)
						{
							PrintMessage(translator.Translate("gamesave_null_error"), PrintMessageLevel.Error);
							break;
						}
						if (hasArgs)
						{
							//如果有参数
							if (!int.TryParse(inputSplited[1] ?? "1", out int arg))
							{
								PrintMessage(translator.Translate("cmd_page_not_a_number"), PrintMessageLevel.Warning);
								break;
							}
							int pageMax = GetPagesCount(CurrentScene, (int)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["UpgradedTimes"]);
							if (1 <= arg && arg <= pageMax) //符合页面范围
							{
								CurrentPage = arg - 1;
							}
							else //超出页面范围
							{
								PrintMessage(String.Format(translator.Translate("cmd_page_out_of_bound"), arg), PrintMessageLevel.Warning);
								break;
							}
						}
						//根据场景显示页面
						//cS初始声明并赋值
						Dictionary<string, object> currentSceneData = (Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene];
						List<object> currentSceneSlots = (List<object>)currentSceneData["Slots"];
						PrintMessage(String.Format(translator.Translate("cmd_page_text"), translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"]), (int)currentSceneData["UpgradedTimes"] + 1, (int)SceneData[CurrentScene]["MaxUpgradeTimes"] + 1, MemoryGameData["Money"]), PrintMessageLevel.Info);
						switch (CurrentScene)
						{
							case 0: //仓库
								for (int i = 0; i < 6; i++)
								{
									ItemProperties currentItemData = ItemsProperties[(int)((Dictionary<string, object>)currentSceneSlots[i + 6 * CurrentPage])["ItemID"]];
									PrintItem(
										i,
										translator.Translate("item_name_" + currentItemData.Name),
										PrintSlotType.Interactive,
										currentItemData.TextColor,
										"",
										false
									);
								}
								break;
							case 1: //农田
								//更新农田
								for (int index = 0; index < 6; index++)
								{
									Dictionary<string, object> farmSlot = (Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[1])["Slots"])[index];
									if ((int)farmSlot["ItemID"] != 0)
									{
										farmSlot = UpdateFarmSlot(farmSlot, (int)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[1])["UpgradedTimes"]);
									}
									((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[1])["Slots"])[index] = farmSlot;
								}
								//显示
								for (int i = 0; i < 6; i++)
								{
									Dictionary<string, object> currentSlotData = (Dictionary<string, object>)currentSceneSlots[i + 6 * CurrentPage];
									int currentItemID = (int)currentSlotData["ItemID"];
									ItemProperties currentItemData = ItemsProperties[currentItemID];
									PrintItem(
										i,
										translator.Translate("item_name_" + currentItemData.Name),
										currentItemData.FarmSlotProperties.SlotType,
										currentItemData.TextColor,
										"",
										GetFarmSlotWatering((string)(currentSlotData)["LastWaterTime"], (int)currentSceneData["UpgradedTimes"])
									);
								}
								break;
							case 2: //商店
								((List<object>)MemoryGameData["ScenesData"])[CurrentScene] = UpdateStore(currentSceneData);
								currentSceneSlots = (List<object>)currentSceneData["Slots"];
								for (int i = 0; i < 6; i++)
								{
									ItemProperties currentItemData = ItemsProperties[(int)((Dictionary<string, object>)currentSceneSlots[i + 6 * CurrentPage])["ItemID"]];
									PrintItem(
										i,
										translator.Translate("item_name_" + currentItemData.Name),
										PrintSlotType.OutputOnly,
										currentItemData.TextColor,
										" --- " + ((Dictionary<string, object>)currentSceneSlots[i + 6 * CurrentPage])["Price"].ToString(),
										false
									);
								}
								break;
							case 3: //花园
								if (Console.WindowWidth < 59) //如果画面宽度过小
								{
									PrintMessage(translator.Translate("cmd_page_width_too_short"), PrintMessageLevel.Warning);
									break;
								}
								bool wasUpgrade = (int)currentSceneData["UpgradedTimes"] >= 1;
								Console.Write("\n");
								foreach (List<int> mapLine in GardenPageMap)
								{
									foreach (int pixelInt in mapLine)
									{
										if (pixelInt == 0) //空格
										{
											Console.Write(" ");
											continue;
										}
										if (pixelInt == 1) //花盆灰
										{
											Console.ForegroundColor = ConsoleColor.DarkGray;
											Console.Write("█");
											Console.ResetColor();
											continue;
										}
										if (pixelInt == 2) //花盆白
										{
											if (wasUpgrade)
											{
												Console.ForegroundColor = ConsoleColor.White;
											}
											else
											{
												Console.ForegroundColor = ConsoleColor.DarkGray;
											}
											Console.Write("█");
											Console.ResetColor();
											continue;
										}
										int slotID;
										if (pixelInt <= 19) //花瓣颜色
										{
											slotID = (int)((Dictionary<string, object>)currentSceneSlots[pixelInt - 10])["ItemID"];
											if (slotID != 0)
											{
												Console.ForegroundColor = ItemsProperties[slotID].TextColor;
												Console.Write("█");
												Console.ResetColor();
											}
											else
											{
												Console.Write(" ");
											}
											continue;
										}
										if (pixelInt <= 29) //花蕊
										{
											slotID = (int)((Dictionary<string, object>)currentSceneSlots[pixelInt - 20])["ItemID"];
											if (slotID != 0)
											{
												Console.ForegroundColor = ConsoleColor.DarkYellow;
												Console.Write("█");
												Console.ResetColor();
											}
											else
											{
												Console.Write(" ");
											}
											continue;
										}
										if (pixelInt <= 39) //花茎
										{
											slotID = (int)((Dictionary<string, object>)currentSceneSlots[pixelInt - 30])["ItemID"];
											if (slotID != 0)
											{
												Console.ForegroundColor = ConsoleColor.DarkGreen;
												Console.Write("█");
												Console.ResetColor();
											}
											else
											{
												Console.Write(" ");
											}
											continue;
										}
									}
									Console.Write("\n");
								}
								Console.WriteLine("    0         1         2         3         4         5");
								break;
							case 4: //市场
								for (int i = 0; i < 6; i++)
								{
									ItemProperties currentItemData = ItemsProperties[MarketItemList[i + 6 * CurrentPage]];
									string priceBar = " --- " + currentItemData.MarketItemProperties.NowPrice.ToString();
									if ((int)currentSceneData["UpgradedTimes"] == 1)
									{
										priceBar += translator.Translate("market_tomorrow_text") + currentItemData.MarketItemProperties.NextDayPrice.ToString();
									}
									PrintItem(
										i,
										translator.Translate("item_name_" + currentItemData.Name),
										PrintSlotType.None,
										currentItemData.TextColor,
										priceBar,
										false
									);
								}
								break;
							case 5: //苹果树
								shouldSave = true;
								((List<object>)MemoryGameData["ScenesData"])[5] = UpdateAppleTree(currentSceneData);
								currentSceneData = (Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[5];
								int appleCount = (int)currentSceneData["AppleCollected"];
								ItemProperties appleProperties = ItemsProperties[1];
								if (appleCount >= 1)
								{
									PrintItem(
										0,
										translator.Translate("item_name_" + appleProperties.Name),
										PrintSlotType.OutputOnly,
										appleProperties.TextColor,
										"",
										false
									);
								}
								else
								{
									PrintItem(
										0,
										translator.Translate("item_name_" + ItemsProperties[0].Name),
										PrintSlotType.OutputOnly,
										ItemsProperties[0].TextColor,
										"",
										false
									);
								}
								for (int i = 1; i < 6; i++)
								{
									if (appleCount >= i + 1)
									{
										PrintItem(
											i,
											translator.Translate("item_name_" + appleProperties.Name),
											PrintSlotType.None,
											appleProperties.TextColor,
											"",
											false
										);
									}
									else if (i > (int)currentSceneData["UpgradedTimes"])
									{
										PrintItem(
											i,
											"---",
											PrintSlotType.None,
											ItemsProperties[0].TextColor,
											"",
											false
										);
									}
									else
									{
										PrintItem(
											i,
											"",
											PrintSlotType.None,
											ItemsProperties[0].TextColor,
											"",
											false
										);
									}
								}
								break;
							default:
								CurrentScene = 0;
								goto case 0;
						}
						PrintMessage(String.Format(translator.Translate("cmd_page_text_pages"), CurrentPage + 1, GetPagesCount(CurrentScene, (int)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["UpgradedTimes"])), PrintMessageLevel.Info);
						break;
					case "S":
					case "SWAP": //SWAP命令
						if (MemoryGameData == null)
						{
							PrintMessage(translator.Translate("gamesave_null_error"), PrintMessageLevel.Error);
							break;
						}
						//cS初始赋值(相当于声明)
						currentSceneData = (Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene];
						currentSceneSlots = (List<object>)currentSceneData["Slots"];
						bool isSuccess = true;
						if (hasArgs)
						{
							int argIndex = Convert.ToInt32(inputSplited[1]);
							if (!(0 <= argIndex && argIndex <= 5))
							{
								PrintMessage(translator.Translate("cmd_swap_index_out_of_bound"), PrintMessageLevel.Warning);
							}
							else
							{
								int currentID = (int)MemoryGameData["TakingItemID"];
								Dictionary<string, object> targetItem;
								int targetItemID;
								switch (CurrentScene)
								{
									case 0: //仓库
										try
										{
											int targetIndex = argIndex + 6 * CurrentPage;
											targetItem = ((Dictionary<string, object>)currentSceneSlots[targetIndex]);
											targetItemID = (int)targetItem["ItemID"];
											targetItem["ItemID"] = (int)MemoryGameData["TakingItemID"];
											MemoryGameData["TakingItemID"] = targetItemID;
										}
										catch (Exception ex)
										{
											PrintMessage(String.Format("[{0}] Error : \n", translator.Translate("game_title")) + ex.ToString() + "\n", PrintMessageLevel.Fatal);
											isSuccess = false;
										}
										if (isSuccess)
										{
											shouldSave = true;
											needGotoPage = true;
											PrintMessage(String.Format(translator.Translate("cmd_swap_success_inventory"), CurrentPage + 1, argIndex), PrintMessageLevel.Info);
										}
										break;
									case 1: //农田
										if (currentID != 0)
										{
											PrintMessage(translator.Translate("cmd_swap_hand_not_empty"), PrintMessageLevel.Warning);
											break;
										}
										try
										{
											//更新农田
											for (int index = 0; index < 6; index++)
											{
												Dictionary<string, object> farmSlot = (Dictionary<string, object>)currentSceneSlots[index];
												if ((int)farmSlot["ItemID"] != 0)
												{
													farmSlot = UpdateFarmSlot(farmSlot, (int)currentSceneData["UpgradedTimes"]);
												}
												currentSceneSlots[index] = farmSlot;
											}
											//
											targetItem = (Dictionary<string, object>)currentSceneSlots[argIndex]; //获取目标格子的物品字典
											targetItemID = (int)targetItem["ItemID"]; //目标格子的物品ID
											if (!ItemsProperties[targetItemID].FarmSlotProperties.CanSwap)
											{
												//如果目标格子的物品ID是不能Swap的，报错
												PrintMessage(String.Format(translator.Translate("cmd_swap_target_cannot_swap"), argIndex), PrintMessageLevel.Warning);
											}
											else
											{
												MemoryGameData["TakingItemID"] = targetItemID;
												targetItem["ItemID"] = ItemsProperties[targetItemID].FarmSlotProperties.SwapToID;
											}
										}
										catch (Exception ex)
										{
											PrintMessage(String.Format("[{0}] Error : \n", translator.Translate("game_title")) + ex.ToString() + "\n", PrintMessageLevel.Fatal);
											isSuccess = false;
										}
										if (isSuccess)
										{
											shouldSave = true;
											needGotoPage = true;
											PrintMessage(String.Format(translator.Translate("cmd_swap_success_farm"), argIndex), PrintMessageLevel.Info);
										}
										break;
									case 2: //商店
										if (currentID != 0)
										{
											PrintMessage(translator.Translate("cmd_swap_hand_not_empty"), PrintMessageLevel.Warning);
											break;
										}
										targetItem = ((Dictionary<string, object>)currentSceneSlots[argIndex + 6 * CurrentPage]);
										int price = (int)targetItem["Price"];
										int money = (int)MemoryGameData["Money"];
										if (price > money)
										{
											PrintMessage(translator.Translate("cmd_swap_shop_no_money"), PrintMessageLevel.Warning);
											break;
										}
										targetItemID = (int)targetItem["ItemID"];
										if (targetItemID == 0) //如果商品为空
										{
											PrintMessage(String.Format(translator.Translate("cmd_swap_shop_slot_empty"), argIndex), PrintMessageLevel.Warning);
											break;
										}
										MemoryGameData["Money"] = money - price;
										if (CurrentPage == 0) //第一页
										{
											MemoryGameData["TakingItemID"] = targetItemID;
											targetItem["ItemID"] = 0;
											targetItem["Price"] = 0;
										}
										else //第二页
										{
											if (argIndex == 5) //买的重新补货
											{
												for (int i = 0; i < 6; i++) //第一页，随机上架作物种子
												{
													int itemID = StoreItemsIDPage1[mainRandom.Next(0, StoreItemsIDPage1.Count - 1)];
													((Dictionary<string, object>)currentSceneSlots[i])["ItemID"] = itemID;
													((Dictionary<string, object>)currentSceneSlots[i])["Price"] = ItemsProperties[itemID].MarketItemProperties.NowPrice;
												}
												for (int i = 6; i < 11; i++) //第二页，刷新物品价格
												{
													int itemID = (int)((Dictionary<string, object>)currentSceneSlots[i])["ItemID"];
													((Dictionary<string, object>)currentSceneSlots[i])["Price"] = ItemsProperties[itemID].MarketItemProperties.NowPrice;
												}
												((Dictionary<string, object>)currentSceneSlots[11])["Price"] = ItemsProperties[40].MarketItemProperties.NowPrice - 20 * (int)currentSceneData["UpgradedTimes"];
												break;
											}
											//第二页其他物品无限购买，买完刷新价格
											MemoryGameData["TakingItemID"] = targetItemID;
										}
										PrintMessage(String.Format(translator.Translate("cmd_swap_shop_success"), translator.Translate("item_name_" + ItemsProperties[targetItemID].Name)), PrintMessageLevel.Info);
										break;
									case 3: //花园
										try
										{
											int takingID = (int)MemoryGameData["TakingItemID"];
											if (takingID == 0 || FlowerIDs.Contains<int>(takingID)) //如果手里是空气或者花朵，允许交换
											{
												targetItem = ((Dictionary<string, object>)currentSceneSlots[argIndex]);
												targetItemID = (int)targetItem["ItemID"];
												targetItem["ItemID"] = (int)MemoryGameData["TakingItemID"];
												MemoryGameData["TakingItemID"] = targetItemID;
											}
											else //否则报错
											{
												PrintMessage(translator.Translate("cmd_swap_garden_not_flower"), PrintMessageLevel.Warning);
												isSuccess = false;
											}
											
										}
										catch (Exception ex)
										{
											PrintMessage(String.Format("[{0}] Error : \n", translator.Translate("game_title")) + ex.ToString() + "\n", PrintMessageLevel.Fatal);
											isSuccess = false;
										}
										if (isSuccess)
										{
											shouldSave = true;
											needGotoPage = true;
											PrintMessage(String.Format(translator.Translate("cmd_swap_success_garden"), argIndex), PrintMessageLevel.Info);
										}
										break;
									case 4: //市场
										PrintMessage(translator.Translate("cmd_swap_scene_not_support"), PrintMessageLevel.Warning);
										break;
									case 5: //苹果树
										PrintMessage(translator.Translate("cmd_swap_scene_cannot_arg"), PrintMessageLevel.Warning);
										break;
									default:
										break;
								}
							}
						}
						else
						{
							switch (CurrentScene)
							{
								case 0: //仓库
									PrintMessage(translator.Translate("cmd_swap_scene_need_arg"), PrintMessageLevel.Warning);
									break;
								case 1: //农田
									PrintMessage(translator.Translate("cmd_swap_scene_need_arg"), PrintMessageLevel.Warning);
									break;
								case 2: //商店
									PrintMessage(translator.Translate("cmd_swap_scene_need_arg"), PrintMessageLevel.Warning);
									break;
								case 3: //花园
									PrintMessage(translator.Translate("cmd_swap_scene_need_arg"), PrintMessageLevel.Warning);
									break;
								case 4: //市场
									int id = (int)MemoryGameData["TakingItemID"];
									int price = 0;
									if (id == 0) //如果手持为空
									{
										PrintMessage(translator.Translate("cmd_swap_hand_empty"), PrintMessageLevel.Warning);
										break;
									}
									try
									{
										int money = (int)MemoryGameData["Money"];
										price = ItemsProperties[id].MarketItemProperties.NowPrice;
										money += price;
										MemoryGameData["Money"] = money;
										MemoryGameData["TakingItemID"] = 0;
									}
									catch (Exception ex)
									{
										PrintMessage(String.Format("[{0}] Error : \n", translator.Translate("game_title")) + ex.ToString() + "\n", PrintMessageLevel.Fatal);
										isSuccess = false;
									}
									if (isSuccess)
									{
										shouldSave = true;
										PrintMessage(String.Format(translator.Translate("cmd_swap_success_market"), price), PrintMessageLevel.Info);
									}
									break;
								case 5: //苹果树
									try
									{
										int collected = (int)currentSceneData["AppleCollected"];
										if (collected == 0)
										{
											PrintMessage(translator.Translate("cmd_swap_scene_no_obj"), PrintMessageLevel.Warning);
											break;
										}
										if ((int)MemoryGameData["TakingItemID"] != 0)
										{
											PrintMessage(translator.Translate("zh_cmd_swap_hand_not_empty"), PrintMessageLevel.Warning);
											break;
										}
										int upgradedTimes = (int)currentSceneData["UpgradedTimes"];
										if (collected >= upgradedTimes + 1)
										{
											currentSceneData["LastTimeGotApple"] = DateTime.UtcNow.ToBinary().ToString();
										}
										MemoryGameData["TakingItemID"] = 1;
										currentSceneData["AppleCollected"] = collected - 1;
									}
									catch (Exception ex)
									{
										PrintMessage(String.Format("[{0}] Error : \n", translator.Translate("game_title")) + ex.ToString() + "\n", PrintMessageLevel.Fatal);
										isSuccess = false;
									}
									if (isSuccess)
									{
										shouldSave = true;
										needGotoPage = true;
										PrintMessage(translator.Translate("cmd_swap_success_tree"), PrintMessageLevel.Info);
									}
									break;
								default:
									break;
							}
						}
						if (needGotoPage)
						{
							hasArgs = false;
							Console.WriteLine("");
							goto case "PAGE";
						}
						break;
					case "CD":
					case "GOTO": //GOTO命令
						if (hasArgs)
						{
							//如果有参数
							if (MemoryGameData == null)
							{
								PrintMessage(translator.Translate("gamesave_null_error"), PrintMessageLevel.Error);
								break;
							}
							string arg = inputSplited[1].ToUpper();
							needGotoPage = true;
							switch (arg)
							{
								case "I":
									CurrentScene = 0;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "INVENTORY":
									CurrentScene = 0;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "F":
									CurrentScene = 1;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "FARM":
									CurrentScene = 1;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "S":
									CurrentScene = 2;
									CurrentPage = 0;
									((List<object>)MemoryGameData["ScenesData"])[CurrentScene] = UpdateStore((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene]);
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "STORE":
									CurrentScene = 2;
									CurrentPage = 0;
									((List<object>)MemoryGameData["ScenesData"])[CurrentScene] = UpdateStore((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene]);
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "G":
									CurrentScene = 3;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "GARDEN":
									CurrentScene = 3;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "M":
									CurrentScene = 4;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "MARKET":
									CurrentScene = 4;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "T":
									CurrentScene = 5;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								case "TREE":
									CurrentScene = 5;
									CurrentPage = 0;
									PrintMessage(
										String.Format(
											translator.Translate("cmd_goto_success"),
											translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])
										),
										PrintMessageLevel.Info
									);
									break;
								default: //其他
									PrintMessage(String.Format(translator.Translate("cmd_goto_unknown_arg"), arg), PrintMessageLevel.Warning);
									needGotoPage = false;
									break;
							}
							if (needGotoPage)
							{
								hasArgs = false;
								shouldSave = true;
								Console.WriteLine("");
								goto case "PAGE";
							}
						}
						else
						{
							//如果没参数，报提示信息
							PrintMessage(translator.Translate("cmd_goto_no_args"), PrintMessageLevel.Info);
						}
						break;
					case "D":
					case "USE":
						if (MemoryGameData == null)
						{
							PrintMessage(translator.Translate("gamesave_null_error"), PrintMessageLevel.Error);
							break;
						}
						if (hasArgs)
						{
							int currentID = (int)MemoryGameData["TakingItemID"];
							if (!ItemsProperties[currentID].FarmSlotProperties.CanUse) //如果手持物品不支持USE，阻断
							{
								PrintMessage(translator.Translate("cmd_use_item_cannot_use"), PrintMessageLevel.Warning);
								break;
							}
							int argIndex = Convert.ToInt32(inputSplited[1]);
							if (!(0 <= argIndex && argIndex <= 5)) //如果超出格子索引范围，阻断
							{
								PrintMessage(translator.Translate("cmd_use_index_out_of_bound"), PrintMessageLevel.Warning);
								break;
							}
							if (CurrentScene == 1) //当前场景是农田
							{
								if (currentID == 2) //浇水壶
								{
									((Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["Slots"])[argIndex])["LastWaterTime"] = DateTime.UtcNow.ToBinary().ToString();
									PrintMessage(translator.Translate("cmd_use_watering"), PrintMessageLevel.Info);
									waterSleep = true;
									shouldSave = true;
								}
								else if (currentID == 3) //铲子
								{
									if ((int)((Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["Slots"])[argIndex])["ItemID"] != 0)
									{
										((Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["Slots"])[argIndex])["ItemID"] = 0;
										PrintMessage(String.Format(translator.Translate("cmd_use_shovel_success"), argIndex), PrintMessageLevel.Info);
										needGotoPage = true;
										shouldSave = true;
									}
									else
									{
										PrintMessage(translator.Translate("cmd_use_shovel_air"), PrintMessageLevel.Info);
									}
								}
								else if (currentID == 4) //大号水壶
								{
									PrintMessage(translator.Translate("cmd_use_watering"), PrintMessageLevel.Info);
									waterSleep = true;
									shouldSave = true;
								}
								else if (currentID == 5) //超级水壶
								{
									for (int slotIndex = 0; slotIndex < 6; slotIndex++)
									{
										((Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["Slots"])[slotIndex])["LastWaterTime"] = DateTime.UtcNow.ToBinary().ToString();
									}
									PrintMessage(translator.Translate("cmd_use_watering"), PrintMessageLevel.Info);
									waterSleep = true;
									shouldSave = true;
								}
								else if (currentID == 39) //一次性浇水包
								{
									for (int slotIndex = 0; slotIndex < 6; slotIndex++)
									{
										((Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["Slots"])[slotIndex])["LastWaterTime"] = DateTime.UtcNow.ToBinary().ToString();
									}
									MemoryGameData["TakingItemID"] = 0;
									PrintMessage(translator.Translate("cmd_use_watering"), PrintMessageLevel.Info);
									waterSleep = true;
									shouldSave = true;
								}
								else{
									//种子
									((Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["Slots"])[argIndex])["ItemID"] = currentID;
									((Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["Slots"])[argIndex])["LastUpdateTime"] = DateTime.UtcNow.ToBinary().ToString();
									((Dictionary<string, object>)((List<object>)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene])["Slots"])[argIndex])["HadWaterTime"] = "0";
									MemoryGameData["TakingItemID"] = 0;
									needGotoPage = true;
									shouldSave = true;
								}
								if (needGotoPage)
								{
									hasArgs = false;
									Console.WriteLine("");
									goto case "PAGE";
								}
								break;
							}
							else
							{
								PrintMessage(translator.Translate("cmd_use_scene_is_not_farm"), PrintMessageLevel.Warning);
								break;
							}
						}
						else
						{
							PrintMessage(translator.Translate("cmd_use_need_arg"), PrintMessageLevel.Warning);
							break;
						}
					case "UPG":
					case "UPGRADE":
						if (MemoryGameData == null)
						{
							PrintMessage(translator.Translate("gamesave_null_error"), PrintMessageLevel.Error);
							break;
						}
						currentSceneData = (Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[CurrentScene];
						int hadUpgradeTimes = (int)currentSceneData["UpgradedTimes"];
						int maxUpgradeTimes = (int)SceneData[CurrentScene]["MaxUpgradeTimes"];
						if (CurrentScene == 3) //如果为花园
						{
							bool canUpgradeGarden = true;
							for (int i = 0; i < 3; i++)
							{
								if ((int)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[i])["UpgradedTimes"] != (int)SceneData[i]["MaxUpgradeTimes"])
								{
									canUpgradeGarden = false;
								}
							}
							for (int i = 4; i < 6; i++)
							{
								if ((int)((Dictionary<string, object>)((List<object>)MemoryGameData["ScenesData"])[i])["UpgradedTimes"] != (int)SceneData[i]["MaxUpgradeTimes"])
								{
									canUpgradeGarden = false;
								}
							}
							if (!canUpgradeGarden)
							{
								PrintMessage(translator.Translate("cmd_upgrade_garden_locked"), PrintMessageLevel.Info);
								break;
							}
						}
						if (hadUpgradeTimes >= maxUpgradeTimes)
						{
							PrintMessage(translator.Translate("cmd_upgrade_scene_max"), PrintMessageLevel.Warning);
							break;
						}
						int needMoney = ((List<int>)SceneData[CurrentScene]["UpgradeCosts"])[hadUpgradeTimes];
						int haveMoney = (int)MemoryGameData["Money"];
						if (haveMoney < needMoney)
						{
							PrintMessage(translator.Translate("cmd_upgrade_no_money"), PrintMessageLevel.Warning);
							break;
						}
						shouldSave = true;
						hadUpgradeTimes++;
						currentSceneData["UpgradedTimes"] = hadUpgradeTimes;
						haveMoney -= needMoney;
						MemoryGameData["Money"] = haveMoney;
						if (CurrentScene == 2) //如果为商店
						{
							((List<object>)MemoryGameData["ScenesData"])[CurrentScene] = UpdateStore(currentSceneData);
							((Dictionary<string, object>)((List<object>)currentSceneData["Slots"])[11])["Price"] = ItemsProperties[40].MarketItemProperties.NowPrice * (2.0f - 0.25f * hadUpgradeTimes);
						}
						if (CurrentScene == 5) //如果为苹果树
						{
							if ((int)currentSceneData["AppleCollected"] >= hadUpgradeTimes)
							{
								currentSceneData["LastTimeGotApple"] = DateTime.UtcNow.ToBinary().ToString();
							}
						}
						PrintMessage(String.Format(translator.Translate("cmd_upgrade_success"), translator.Translate("scene_name_" + SceneData[CurrentScene]["SceneName"])), PrintMessageLevel.Info);
						break;
					default:
						PrintMessage(String.Format(translator.Translate("unknown_cmd"), inputSplited[0]), PrintMessageLevel.Warning);
						break;
				}
				if (MemoryGameData == null) //防止空引用异常警告
				{
					continue;
				}
				//每个主循环结束时的保存环节
				if (!shouldSave)
				{
					continue;
				}
				if (MemoryCustomPath != "") //如果自定义路径不为空，也就是使用自定义路径
				{
					if (!Path.IsPathRooted(MemoryCustomPath)) //如果自定义路径值不是合法的绝对路径
					{
						//输出信息并回退到默认路径
						PrintMessage(String.Format(resMgrTranslatedText.GetString(translatePrefix + "custom_path_illegal") ?? ""), PrintMessageLevel.Error);
						MemoryCustomPath = "";
						MemoryGameData["CustomPath"] = "";
					}
					else
					{
						try
						{
							using (FileStream fs = new(MemoryCustomPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
							{
								// 文件没有被占用，可以进行写入操作
								fs.Close(); // 关闭文件
							}
							//保存自定义存档
							File.WriteAllText(MemoryCustomPath, JsonSerializer.Serialize(MemoryGameData));
						}
						catch (Exception ex)
						{
							//输出自定义存档报错的消息
							PrintMessage(String.Format("[{0}] Error : \n", translator.Translate("game_title")) + ex.ToString() + "\n", PrintMessageLevel.Fatal);
						}
					}
				}
				try
				{
					using (FileStream fs = new(appdataPath + "\\BCASoft\\TerminalFarm\\TerminalFarmSave.json", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
					{
						// 文件没有被占用，可以进行写入操作
						fs.Close(); // 关闭文件
					}
					//保存原初存档
					File.WriteAllText(appdataPath + "\\BCASoft\\TerminalFarm\\TerminalFarmSave.json", JsonSerializer.Serialize(MemoryGameData));
				}
				catch (Exception ex)
				{
					//输出原初存档报错的消息
					PrintMessage(String.Format("[{0}] Error : \n", translator.Translate("game_title")) + ex.ToString() + "\n", PrintMessageLevel.Fatal);
				}
			}
		}
		internal static string ListUpgradesMainLines(TextTranslator translator, int sceneNumberID, List<object> memoryScenesData) //传入相应参数，返回用于WriteLine在list upgrades中每个场景的主行的字符串，大概的内容是 仓库  [██---]  升级需要资金: 1200
		{
			string result = "  [█";
			int hadUpgradedTimes = (int)((Dictionary<string, object>)memoryScenesData[sceneNumberID])["UpgradedTimes"];
			string blockBar = "";
			for (int i = 0; i < hadUpgradedTimes; i++)
			{
				blockBar += "█";
			}
			int maxUpgradeTimes = (int)SceneData[sceneNumberID]["MaxUpgradeTimes"];
			if (hadUpgradedTimes >= maxUpgradeTimes)
			{
				result += blockBar.PadRight(maxUpgradeTimes, '-') + "]";
			}
			else
			{
				result += blockBar.PadRight(maxUpgradeTimes, '-') + "] " + translator.Translate("cmd_list_upgrades_next_money") + ((List<int>)SceneData[sceneNumberID]["UpgradeCosts"])[hadUpgradedTimes];
			}
			return result;
		}
		internal static Dictionary<string, object> UpdateAppleTree(Dictionary<string, object> treeData) //传入整个苹果树的数据，返回更新至当前时间的苹果树
		{
			Dictionary<string, object> result = new(treeData);
			int appleCollected = (int)result["AppleCollected"];
			int upgradedTimes = (int)result["UpgradedTimes"];
			DateTime lastDropTime = DateTime.FromBinary(Convert.ToInt64((string)result["LastTimeGotApple"]));
			while (appleCollected < (upgradedTimes + 1) && (DateTime.UtcNow - lastDropTime) >= AppleTreeDropDefaultTimeSpan)
			{
				appleCollected++;
				lastDropTime += AppleTreeDropDefaultTimeSpan;
			}
			result["AppleCollected"] = appleCollected;
			result["LastTimeGotApple"] = lastDropTime.ToBinary().ToString();
			return result;
		}
		internal static Dictionary<string, object> UpdateStore(Dictionary<string, object> storeData) //传入整个商店的数据，返回更新至当前时间的商店
		{
			Dictionary<string, object> result = new(storeData);
			DateTime lastUpdateTime = DateTime.FromBinary(Convert.ToInt64(storeData["LastUpdateTime"])); //商店上次更新时间，如果上次更新时间至当前时间经过了一个8:00，那么重置商店
			DateTime refillTimeShouldUse; //重置时间判断依据
			if (DateTime.Now >= new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0))
			{
				refillTimeShouldUse = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
			}
			else
			{
				refillTimeShouldUse = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, 8, 0, 0);
			}
			if ((DateTime.Now - lastUpdateTime) >= (DateTime.Now - refillTimeShouldUse))
			{
				result["LastUpdateTime"] = DateTime.Now.ToBinary().ToString();
				List<object> slots = (List<object>)result["Slots"];
				//补货
				for (int i = 0; i < 6; i++) //第一页，随机上架作物种子
				{
					if ((int)((Dictionary<string, object>)slots[i])["ItemID"] == 0) //如果格子是空的
					{
						int itemID = StoreItemsIDPage1[mainRandom.Next(0, StoreItemsIDPage1.Count - 1)];
						((Dictionary<string, object>)slots[i])["ItemID"] = itemID;
						((Dictionary<string, object>)slots[i])["Price"] = ItemsProperties[itemID].MarketItemProperties.NowPrice;
					}
				}
				//第二页
				if ((int)((Dictionary<string, object>)slots[6 + 0])["ItemID"] != 2)
				{
					((Dictionary<string, object>)slots[6 + 0])["ItemID"] = 2;
					((Dictionary<string, object>)slots[6 + 0])["Price"] = ItemsProperties[2].MarketItemProperties.NowPrice * 2;
					((Dictionary<string, object>)slots[6 + 1])["ItemID"] = 3;
					((Dictionary<string, object>)slots[6 + 1])["Price"] = ItemsProperties[3].MarketItemProperties.NowPrice * 2;
					((Dictionary<string, object>)slots[6 + 2])["ItemID"] = 4;
					((Dictionary<string, object>)slots[6 + 2])["Price"] = ItemsProperties[4].MarketItemProperties.NowPrice * 2;
					((Dictionary<string, object>)slots[6 + 3])["ItemID"] = 5;
					((Dictionary<string, object>)slots[6 + 3])["Price"] = ItemsProperties[5].MarketItemProperties.NowPrice * 2;
					((Dictionary<string, object>)slots[6 + 4])["ItemID"] = 39;
					((Dictionary<string, object>)slots[6 + 4])["Price"] = ItemsProperties[39].MarketItemProperties.NowPrice * 2;
					((Dictionary<string, object>)slots[6 + 5])["ItemID"] = 40;
					((Dictionary<string, object>)slots[6 + 5])["Price"] = ItemsProperties[40].MarketItemProperties.NowPrice * 2; //这里只起到初始化存档中的第二页的作用，重新补货的价格由升级直接修改，而不是这里
				}
			}
			return result;
		}
		internal static Dictionary<string, object> UpdateFarmSlot(Dictionary<string, object> farmSlot, int farmUpgradeTimes) //传入一个物品格子，返回该格子更新到最新时间的样子
		{
			Dictionary<string, object> result = new(farmSlot);
			int sourcePlantID = (int)result["ItemID"];
			DateTime slotDryTime = DateTime.FromBinary(Convert.ToInt64(farmSlot["LastWaterTime"])) + FarmSlotWateringDefaultTimeSpan * (1.0 + 0.2 * farmUpgradeTimes); //通过上次浇水时间计算槽位干燥的时间
			if (slotDryTime > DateTime.UtcNow)
			{
				slotDryTime = DateTime.UtcNow; //钳制湿润截止时间到当前
			}
			TimeSpan realWaterSpan = slotDryTime - DateTime.FromBinary(Convert.ToInt64(farmSlot["LastUpdateTime"])); //计算从上次更新截至现在的湿润时间长度。当前到上次更新的时间
			TimeSpan hadWaterSpan = TimeSpan.FromTicks(Convert.ToInt64(farmSlot["HadWaterTime"])) + realWaterSpan; //读取累积湿润时间并加上新增湿润时间。可用于后续不断作减法
			TimeSpan needWaterSpan = ItemsProperties[sourcePlantID].FarmSlotProperties.GrowTime; //读取对应作物总共需要的湿润时间
			Console.WriteLine("debug:" + "\nLastUpdateTime: " + DateTime.FromBinary(Convert.ToInt64(farmSlot["LastUpdateTime"])).ToString() + "\nLastWaterTime: " + DateTime.FromBinary(Convert.ToInt64(farmSlot["LastWaterTime"])).ToString() + "\nslotDryTime(日期): " + slotDryTime.ToString() + "\nrealWaterSpan(长度): " + realWaterSpan.ToString() + "\nhadWaterSpan(长度): " + hadWaterSpan + "\nneedWaterSpan: " + needWaterSpan);
			while (true)
			{
				if (hadWaterSpan >= needWaterSpan) //如果累积湿润时间+新增湿润时间大于等于需要湿润时间，进入下一个阶段
				{
					hadWaterSpan -= needWaterSpan;
					int newPlantID = FarmPlantGrow((int)result["ItemID"]);
					if (newPlantID == 38) //成花抽花
					{
						newPlantID = new Random().Next(0, FlowerIDs.Length - 1);
					}
					result["ItemID"] = newPlantID;
					needWaterSpan = ItemsProperties[newPlantID].FarmSlotProperties.GrowTime; //更新总共需要的湿润时间到新的物品ID
				}
				else //hadWaterSpan不大于needWaterSpan，不改变阶段
				{
					result["HadWaterTime"] = hadWaterSpan.Ticks.ToString(); //保存新的累积湿润时间
					break;
				}
			}
			result["LastUpdateTime"] = DateTime.UtcNow.ToBinary().ToString();
			return result;
		}
		internal static int FarmPlantGrow(int fromID)
		{
			return ItemsProperties[fromID].FarmSlotProperties.NextStateID;

		}
		internal static bool GetFarmSlotWatering(string lastWaterTimeStr, int farmUpgradeTimes) //传入格子上次浇水时间和农场升级次数，返回当前该格是否湿润
		{
			if (lastWaterTimeStr == "")
			{
				return false;
			}
			double waterTimeMultipier = 1.0 + 0.2 * farmUpgradeTimes; //根据升级次数计算浇水维持时间倍率，正常范围是1.0-2.6
			TimeSpan difference = DateTime.UtcNow - DateTime.FromBinary(Convert.ToInt64(lastWaterTimeStr)); //求当前时间与上次浇水的时间差
			return FarmSlotWateringDefaultTimeSpan * waterTimeMultipier > difference; //如果浇水应维持时间大于浇水时间差，就返回true
		}
		internal static int GetPagesCount(int sceneID, int upgradedTime = 0) //获取指定场景在指定升级次数下可以访问的最大页面数量
		{
			return sceneID switch
			{
				0 => 1 + upgradedTime,
				1 => 1,
				2 => 2,
				3 => 1,
				4 => 3,
				5 => 1,
				_ => 0,
			};
		}
		internal static readonly List<int> MarketItemList = [
			9, 13, 16, 19, 23, 27, //第一页，作物
			30, 31, 32, 33, 34, 35, //第二页，花朵
			1, 2, 3, 4, 5, 39 //第三页，苹果与工具
		];
		internal class TextTranslator
		{
			internal ResourceManager resMgr;
			internal string prefix;
			internal string Translate(string textName)
			{
				return resMgr.GetString(prefix + textName) ?? "";
			}
			internal TextTranslator(ref ResourceManager resMgrInstance, string translatePrefix)
			{
				resMgr = resMgrInstance;
				prefix = translatePrefix;
			}
		}
		internal static void PrintMessage(string text, PrintMessageLevel level)
		{	//用于输出执行返回文本等信息
			switch (level)
			{
				case PrintMessageLevel.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					break;
				case PrintMessageLevel.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					break;
				case PrintMessageLevel.Fatal:
					Console.ForegroundColor = ConsoleColor.DarkRed;
					break;
				default:
					break;
			}
			Console.WriteLine(text);
			Console.ResetColor();
		}
		internal enum PrintSlotType
		{
			None = 0,
			Interactive = 1,
			InputOnly = 2,
			OutputOnly = 3,
			FarmGrowing = 4,
			FarmCollectable = 5
		}
		internal static void PrintItem(int indexNum, string translatedName, PrintSlotType slotType, ConsoleColor? itemColor = null, string postfix = "", bool isItemWaterd = false)
		{   //用于输出单行物品
			string outputIndex = "";
			if (indexNum >= 0)
			{
				outputIndex = indexNum.ToString();
			}
			outputIndex = outputIndex.PadRight(4);
			Console.Write(outputIndex);
			if (isItemWaterd)
			{
				Console.ForegroundColor = ConsoleColor.DarkBlue;
			}
			switch (slotType)
			{
				case PrintSlotType.None:
					Console.Write(" ");
					break;
				case PrintSlotType.Interactive:
					Console.Write("[");
					break;
				case PrintSlotType.InputOnly:
					Console.Write(">");
					break;
				case PrintSlotType.OutputOnly:
					Console.Write("<");
					break;
				case PrintSlotType.FarmGrowing:
					Console.Write("(");
					break;
				case PrintSlotType.FarmCollectable:
					Console.Write("{");
					break;
				default:
					break;
			}
			Console.ResetColor();
			if (itemColor != null) //不为null时表示指定颜色，否则使用默认前景颜色
			{
				Console.ForegroundColor = (ConsoleColor)itemColor;
			}
			Console.Write(translatedName);
			if (isItemWaterd)
			{
				Console.ForegroundColor = ConsoleColor.DarkBlue;
			}
			else
			{
				Console.ResetColor();
			}
			switch (slotType)
			{
				case PrintSlotType.None:
					Console.Write(" ");
					break;
				case PrintSlotType.Interactive:
					Console.Write("]");
					break;
				case PrintSlotType.InputOnly:
					Console.Write("<");
					break;
				case PrintSlotType.OutputOnly:
					Console.Write(">");
					break;
				case PrintSlotType.FarmGrowing:
					Console.Write(")");
					break;
				case PrintSlotType.FarmCollectable:
					Console.Write("}");
					break;
				default:
					break;
			}
			Console.ResetColor();
			Console.Write(postfix + "\n");
		}
		private static Dictionary<string, object>? GameDataInit()
		{
			string defaultGameData =
				@"{
					""UseLanguage"": 0,
					""CustomPath"": """",
					""Money"": 700,
					""TakingItemID"": 0,
					""ScenesData"": [
						{
							""UpgradedTimes"": 0,
							""Slots"": [
								{},{},{},{},{},{},
								{},{},{},{},{},{},
								{},{},{},{},{},{},
								{},{},{},{},{},{},
								{},{},{},{},{},{}
							]
						},
						{
							""UpgradedTimes"": 0,
							""Slots"": [
								{},{},{},{},{},{}
							]
						},
						{
							""UpgradedTimes"": 0,
							""LastUpdateTime"": ""1"",
							""Slots"": [
								{},{},{},{},{},{},{},{},{},{},{},{}
							]
						},
						{
							""UpgradedTimes"": 0,
							""Slots"": [
								{},{},{},{},{},{}
							]
						},
						{
							""UpgradedTimes"": 0,
							""Slots"": []
						},
						{
							""UpgradedTimes"": 0,
							""AppleCollected"": 0,
							""LastTimeGotApple"": ""1"",
							""Slots"": []
						}
					]
				}";
			JsonDocument jsonDocu = JsonDocument.Parse(defaultGameData);
			JsonElement rootElement = jsonDocu.RootElement;
			Dictionary<string, object> result = APIJson.ParseJsonElement(rootElement);
			jsonDocu.Dispose();
			if (result == null)
			{
				Console.WriteLine("Failed to Init GameData");
				return result;
			}
			else
			{
				List<object> scenesData = (List<object>)result["ScenesData"];
				Dictionary<string, object> dataScene = (Dictionary<string, object>)scenesData[0];
				List<object> sceneSlots = (List<object>)dataScene["Slots"];
				for (int index = 0; index < sceneSlots.Count; index++)
				{
					sceneSlots[index] = new Dictionary<string, object>
					{
						{"ItemID", 0 }
					};
				}
				dataScene = (Dictionary<string, object>)scenesData[1];
				sceneSlots = (List<object>)dataScene["Slots"];
				for (int index = 0; index < sceneSlots.Count; index++)
				{
					sceneSlots[index] = new Dictionary<string, object>
					{
						{"LastWaterTime", "1" }, //记录上次浇水的时间，用于计算湿润状态与时间长度
						{"LastUpdateTime", "1" }, //记录上次更新此格子的时间，用于搭配浇水时间记录湿润累积
						{"HadWaterTime", "0" }, //已积累的湿润时间，达到生长时间时就会进入下一个阶段
						{"ItemID", 0 } //记录物品ID
					};
				}
				dataScene = (Dictionary<string, object>)scenesData[2];
				sceneSlots = (List<object>)dataScene["Slots"];
				for (int index = 0; index < sceneSlots.Count; index++)
				{
					sceneSlots[index] = new Dictionary<string, object>
					{
						{"Price", 0 },
						{"ItemID", 0 }
					};
				}
				dataScene = (Dictionary<string, object>)scenesData[3];
				sceneSlots = (List<object>)dataScene["Slots"];
				for (int index = 0; index < sceneSlots.Count; index++)
				{
					sceneSlots[index] = new Dictionary<string, object>
					{
						{"ItemID", 0 }
					};
				}
			}
			return result;
		}
		/*internal class StringableDateTime
		{
			internal int year = 0;
			internal int month = 0;
			internal int day = 0;
			internal int hour = 0;
			internal int minute = 0;
			internal int second = 0;
			internal int microsecond = 0;
			internal static string ToString(StringableDateTime inputDateTime)
			{
				
				string result = "";
				result += inputDateTime.year.ToString() + ".";
				result += inputDateTime.month.ToString() + ".";
				result += inputDateTime.day.ToString() + ".";
				result += inputDateTime.hour.ToString() + ".";
				result += inputDateTime.minute.ToString() + ".";
				result += inputDateTime.second.ToString() + ".";
				result += inputDateTime.microsecond.ToString();
				return result;
			}
			internal static StringableDateTime? FromString(string inputStr)
			{
				if (inputStr == "")
				{
					return null;
				}
				StringableDateTime result = new StringableDateTime();
				string[] sourceStr = inputStr.Split('.', StringSplitOptions.None);
				int carryNum = 0;
				for (int index = sourceStr.Length - 1; index >= 0; index--)
				{
					int currentNum;
					switch (index)
					{
						case 0:
							currentNum = Convert.ToInt32(sourceStr[index]) + carryNum;
							result.day = currentNum;
							continue;
						case 1:
							currentNum = Convert.ToInt32(sourceStr[index]) + carryNum;
							carryNum = int.Clamp(currentNum / 24, 0, int.MaxValue);
							currentNum %= 24;
							result.hour = currentNum;
							continue;
						case 2:
							currentNum = Convert.ToInt32(sourceStr[index]) + carryNum;
							carryNum = int.Clamp(currentNum / 60, 0, int.MaxValue);
							currentNum %= 60;
							result.minute = currentNum;
							continue;
						case 3:
							currentNum = Convert.ToInt32(sourceStr[index]) + carryNum;
							carryNum = int.Clamp(currentNum / 60, 0, int.MaxValue);
							currentNum %= 60;
							result.second = currentNum;
							continue;
						case 4:
							currentNum = Convert.ToInt32(sourceStr[index]);
							carryNum = int.Clamp(currentNum / 1000, 0, int.MaxValue);
							currentNum %= 1000;
							result.microsecond = currentNum;
							continue;
						default:
							break;
					}
				}
				return result;
			}
		}*/
		

		//Written by ChatGPT-3.5
		internal static class APIJson
		{
			internal static Dictionary<string, object> ParseJsonElement(JsonElement element)
			{
				var dictionary = new Dictionary<string, object>();

				// 遍历 JSON 对象的属性
				foreach (var property in element.EnumerateObject())
				{
					if (property.Value.ValueKind == JsonValueKind.Object)
					{
						// 递归处理 JSON 对象的属性值
						var nestedDictionary = ParseJsonElement(property.Value);
						if (nestedDictionary != null)
						{
							dictionary[property.Name] = nestedDictionary;
						}
					}
					else if (property.Value.ValueKind == JsonValueKind.Array)
					{
						// 递归处理 JSON 数组
						var nestedList = ParseJsonArray(property.Value);
						if (nestedList != null)
						{
							dictionary[property.Name] = nestedList;
						}
					}
					else
					{
						// 其他类型直接添加到字典中
						var value = GetValue(property.Value);
						if (value != null)
						{
							dictionary[property.Name] = value;
						}
					}
				}

				return dictionary;
			}

			// 递归方法：将 JsonElement 数组转换为 List<object>
			internal static List<object> ParseJsonArray(JsonElement array)
			{
				var list = new List<object>();

				// 遍历 JSON 数组中的元素
				foreach (var element in array.EnumerateArray())
				{
					if (element.ValueKind == JsonValueKind.Object)
					{
						// 递归处理 JSON 对象
						var nestedDictionary = ParseJsonElement(element);
						if (nestedDictionary != null)
						{
							list.Add(nestedDictionary);
						}
					}
					else if (element.ValueKind == JsonValueKind.Array)
					{
						// 递归处理嵌套的 JSON 数组
						var nestedList = ParseJsonArray(element);
						if (nestedList != null)
						{
							list.Add(nestedList);
						}
					}
					else
					{
						// 其他类型直接添加到列表中
						var value = GetValue(element);
						if (value != null)
						{
							list.Add(value);
						}
					}
				}

				return list;
			}

			// 辅助方法：将 JsonElement 中的值转换为对应的 .NET 类型
			internal static object? GetValue(JsonElement element)
			{
				return element.ValueKind switch
				{
					JsonValueKind.True => true,
					JsonValueKind.False => false,
					JsonValueKind.String => element.GetString(),
					JsonValueKind.Number => element.GetInt32(),// 根据需要进行适当的类型转换
					_ => null,
				};
			}

			internal static Dictionary<string, object> MergeJsonDictionaries(Dictionary<string, object> original, Dictionary<string, object> merge)
			{
				ArgumentNullException.ThrowIfNull(original);
				if (merge == null)
				{
					return original;
				}

				foreach (var kvp in merge)
				{
					string key = kvp.Key;
					object value = kvp.Value;
					if (!original.TryGetValue(key, out object? _0))
					{
						// 如果原字典中不包含当前键，则直接添加到原字典中
						original.Add(key, value);
					}
					else
					{
						// 如果原字典中包含当前键，则判断值的类型是否相同
						if (value.GetType() == value.GetType())
						{
							// 如果值的类型相同，则进行覆盖
							original[key] = value;
						}
						else
						{
							// 如果值的类型不同，则忽略
							continue;
						}
					}
				}

				return original;
			}

			internal static void MergeJsonArray(List<object> originalList, List<object> mergeList)
			{
				if (originalList == null || mergeList == null)
				{
					return;
				}

				int mergeListLength = mergeList.Count;
				int originalListLength = originalList.Count;

				// 遍历合并列表，将合并列表中的元素逐个合并到原列表中
				for (int i = 0; i < mergeListLength; i++)
				{
					// 如果合并列表的索引小于原列表的长度，说明此时还在原列表的范围内
					if (i < originalListLength)
					{
						// 获取原列表与合并列表中对应索引位置的元素
						object originalItem = originalList[i];
						object mergeItem = mergeList[i];

						// 如果原列表中的元素是字典类型，且合并列表中的元素也是字典类型
						if (originalItem is Dictionary<string, object> originalDict && mergeItem is Dictionary<string, object> mergeDict)
						{
							// 合并字典
							originalList[i] = MergeJsonDictionaries(originalDict, mergeDict);
						}
						// 如果原列表中的元素是列表类型，且合并列表中的元素也是列表类型
						else if (originalItem is List<object> originalInnerList && mergeItem is List<object> mergeInnerList)
						{
							// 递归调用，合并列表中的内部列表
							MergeJsonArray(originalInnerList, mergeInnerList);
						}
						// 如果原列表中的元素与合并列表中的元素的类型相同，则直接覆盖
						else if (originalItem.GetType() == mergeItem.GetType())
						{
							originalList[i] = mergeItem;
						}
					}
					// 如果合并列表的索引大于等于原列表的长度，说明此时已经超出了原列表的范围
					else
					{
						// 直接将合并列表中多余的元素添加到原列表的末尾
						originalList.Add(mergeList[i]);
					}
				}
			}
		}
	}
}
