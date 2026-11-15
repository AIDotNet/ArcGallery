using System.Globalization;

namespace TKCode.Arc.A2UI.Chat;

/// <summary>The app's interface text in en-US, zh-CN and zh-TW (examples and prompts stay English).</summary>
public static class ChatStrings
{
    private static readonly Dictionary<string, (string Hans, string Hant)> s_text = new(StringComparer.Ordinal)
    {
        ["New chat"] = ("新对话", "新對話"),
        ["Examples"] = ("示例", "範例"),
        ["Recents"] = ("最近", "最近"),
        ["Settings"] = ("设置", "設定"),
        ["Back to Arc Gallery"] = ("返回 Arc Gallery", "返回 Arc Gallery"),
        ["Reply…"] = ("回复…", "回覆…"),
        ["Ask anything…"] = ("想问什么都可以…", "想問什麼都可以…"),
        ["Send"] = ("发送", "傳送"),
        ["Stop"] = ("停止", "停止"),
        ["Inspector"] = ("检查器", "檢查器"),
        ["Messages"] = ("消息", "訊息"),
        ["Data"] = ("数据", "資料"),
        ["Errors"] = ("错误", "錯誤"),
        ["No provider"] = ("未配置提供商", "未設定供應商"),
        ["Add a provider"] = ("添加提供商", "新增供應商"),
        ["Run an example"] = ("运行示例", "執行範例"),
        ["Example"] = ("示例", "範例"),
        ["Providers"] = ("提供商", "供應商"),
        ["General"] = ("通用", "一般"),
        ["Name"] = ("名称", "名稱"),
        ["Protocol"] = ("协议", "協定"),
        ["Base URL"] = ("基地址", "基礎位址"),
        ["API key"] = ("API 密钥", "API 金鑰"),
        ["Remember the key on this device"] = ("在本机记住密钥", "在本機記住金鑰"),
        ["Models (one per line)"] = ("模型（每行一个）", "模型（每行一個）"),
        ["Fetch models"] = ("拉取模型列表", "取得模型清單"),
        ["Default model"] = ("默认模型", "預設模型"),
        ["Max output tokens"] = ("最大输出 Token", "最大輸出 Token"),
        ["Connection"] = ("连接", "連線"),
        ["Models"] = ("模型", "模型"),
        ["Reasoning"] = ("推理", "推理"),
        ["Network"] = ("网络", "網路"),
        ["Agent"] = ("智能体", "智慧體"),
        ["Interface"] = ("界面", "介面"),
        ["Export"] = ("导出", "匯出"),
        ["Copied"] = ("已复制", "已複製"),
        ["Saved"] = ("已保存", "已儲存"),
        ["In use"] = ("使用中", "使用中"),
        ["Developer mode"] = ("开发者模式", "開發者模式"),
        ["Shows the inspector (A2UI messages, data models, errors) and error placeholders in answers."] = ("显示检查器（A2UI 消息、数据模型、错误），并在回答里显示错误占位。", "顯示檢查器（A2UI 訊息、資料模型、錯誤），並在回答中顯示錯誤佔位。"),
        ["When an answer's interface has errors, the model gets them and fixes its answer."] = ("回答里的界面出错时，把错误交给模型，让它修正回答。", "回答中的介面出錯時，把錯誤交給模型，讓它修正回答。"),
        ["More reliable interfaces, longer prompts."] = ("界面更可靠，但提示词更长。", "介面更可靠，但提示詞更長。"),
        ["Shows a summary of the model's reasoning above the answer."] = ("在回答上方显示模型推理过程的摘要。", "在回答上方顯示模型推理過程的摘要。"),
        ["Lets the provider retry on another model when a request is refused."] = ("请求被拒绝时，由服务端换一个模型重试。", "請求被拒絕時，由伺服器端換一個模型重試。"),
        ["Providers and conversations as JSON. API keys are never included."] = ("以 JSON 导出提供商和对话，不会包含 API 密钥。", "以 JSON 匯出供應商和對話，不會包含 API 金鑰。"),
        ["Deletes every provider, remembered key and conversation on this device."] = ("删除本机上所有提供商、已记住的密钥和对话。", "刪除本機上所有供應商、已記住的金鑰和對話。"),
        ["Effort"] = ("推理力度", "推理強度"),
        ["Thinking (summaries)"] = ("思考（显示摘要）", "思考（顯示摘要）"),
        ["Server-side fallback"] = ("服务端降级", "伺服器端降級"),
        ["Proxy URL (optional)"] = ("代理地址（可选）", "代理位址（選填）"),
        ["Extra headers (Name: value)"] = ("额外请求头（名称: 值）", "額外標頭（名稱: 值）"),
        ["Test connection"] = ("测试连接", "測試連線"),
        ["Delete"] = ("删除", "刪除"),
        ["Save"] = ("保存", "儲存"),
        ["Developer mode (inspector, error placeholders)"] = ("开发者模式（检查器、错误占位）", "開發者模式（檢查器、錯誤佔位）"),
        ["Send interface errors back to the model"] = ("把界面错误发回给模型修正", "把介面錯誤傳回給模型修正"),
        ["Full catalog schema in the system prompt"] = ("系统提示词里放完整的目录 Schema", "系統提示詞裡放完整的目錄 Schema"),
        ["Copy all data (without keys)"] = ("复制全部数据（不含密钥）", "複製全部資料（不含金鑰）"),
        ["Clear all data"] = ("清除所有数据", "清除所有資料"),
        ["Keys stay in memory unless you choose to remember them. A remembered key is stored unencrypted on this device."] =
            ("密钥默认只保存在内存中；选择记住后会以明文保存在本机。", "金鑰預設只保存在記憶體中；選擇記住後會以明文保存在本機。"),
        ["Requests go from this device straight to the provider. Endpoints that do not allow browser requests need a proxy."] =
            ("请求从本机直接发往提供商；不允许浏览器跨域请求的端点需要代理。", "請求從本機直接送往供應商；不允許瀏覽器跨域請求的端點需要代理。"),
        ["Stopped."] = ("已停止。", "已停止。"),
        ["Thinking…"] = ("思考中…", "思考中…"),
        ["Working…"] = ("处理中…", "處理中…"),
        ["Connected"] = ("连接成功", "連線成功"),
        ["Close"] = ("关闭", "關閉"),
        ["What can I show you?"] = ("想看点什么？", "想看點什麼？"),
        ["Answers can include live interfaces: forms, choices, lists and charts drawn with Arc."] =
            ("回答里可以带可交互的界面：表单、选项、列表和图表，用 Arc 控件绘制。", "回答裡可以帶可互動的介面：表單、選項、清單和圖表，用 Arc 控制項繪製。")
    };

    /// <summary>The text in the current UI language.</summary>
    public static string T(string english)
    {
        var name = CultureInfo.CurrentUICulture.Name;
        if (!s_text.TryGetValue(english, out var text))
        {
            return english;
        }

        if (name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
        {
            return name.Contains("TW", StringComparison.OrdinalIgnoreCase) || name.Contains("HK", StringComparison.OrdinalIgnoreCase)
                   || name.Contains("Hant", StringComparison.OrdinalIgnoreCase)
                ? text.Hant
                : text.Hans;
        }

        return english;
    }
}
