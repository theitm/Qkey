using QKey.Core;

static void Equal(string actual, string expected, string name)
{
    if (actual != expected)
        throw new Exception($"{name}: expected '{expected}', got '{actual}'");
}

static void True(bool value, string name)
{
    if (!value) throw new Exception($"{name}: expected true");
}

var telex = new VietnameseEngine(new EngineOptions { InputMethod = InputMethod.Telex });
Equal(telex.ConvertWord("tieengs"), "tiếng", "telex tone");
Equal(telex.ConvertWord("Vieetj"), "Việt", "telex uppercase");
Equal(telex.ConvertWord("ddawng"), "đăng", "telex dd aw");
Equal(telex.ConvertWord("tuowng"), "tương", "telex uow");
Equal(telex.ConvertWord("Tooi"), "Tôi", "telex case");

var vni = new VietnameseEngine(new EngineOptions { InputMethod = InputMethod.Vni });
Equal(vni.ConvertWord("tie61ng"), "tiếng", "vni tone");
Equal(vni.ConvertWord("Vie65t"), "Việt", "vni uppercase");
Equal(vni.ConvertWord("d9a8ng"), "đăng", "vni d9 a8");
Equal(vni.ConvertWord("tu7ng"), "tưng", "vni u7");

var macroManager = new MacroManager();
macroManager.Set("dc", "được");
True(macroManager.TryExpand("dc", out var macro), "macro expands");
Equal(macro!, "được", "macro value");

var converter = new TextConverter();
Equal(converter.RemoveDiacritics("Tiếng Việt"), "Tieng Viet", "remove diacritics");
Equal(converter.ToSentenceCase("xin chào"), "Xin chào", "sentence case");

Console.WriteLine("OK: QKey .NET core tests passed");
