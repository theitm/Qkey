#Requires AutoHotkey v2.0
#SingleInstance Force
Persistent

; QKey - Vietnamese input method for Windows
SendMode "Input"
SetWorkingDir A_ScriptDir

; State ----------------------------------------------------------------------
g_enabled := true
g_mode := "telex" ; telex | vni
g_raw := ""
g_rendered := ""
g_macros := Map(
    "qkey", "QKey",
    "vn", "Việt Nam",
    "dc", "được"
)

; Hotkeys --------------------------------------------------------------------
^+v::ToggleQKey()
^+m::ToggleMode()
^+r::Reload()

; Letter hooks
for key in StrSplit("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ") {
    Hotkey("~*" key, HandleTextKey)
}
; VNI digits and punctuation/commit keys
for key in StrSplit("0123456789") {
    Hotkey("~*" key, HandleTextKey)
}
for key in ["Space", "Enter", "Tab", "Escape", "Backspace", ".", ",", ";", ":", "?", "!", "-", ")", "(", "'", '"'] {
    try Hotkey("~*" key, HandleControlKey)
}

TraySetIcon("shell32.dll", 174)
A_TrayMenu.Delete()
A_TrayMenu.Add("Toggle QKey (Ctrl+Shift+V)", (*) => ToggleQKey())
A_TrayMenu.Add("Switch Telex/VNI (Ctrl+Shift+M)", (*) => ToggleMode())
A_TrayMenu.Add("Reload", (*) => Reload())
A_TrayMenu.Add("Exit", (*) => ExitApp())
UpdateTrayTip()

ToggleQKey() {
    global g_enabled
    g_enabled := !g_enabled
    ResetBuffer()
    UpdateTrayTip()
    ToolTip(g_enabled ? "QKey bật" : "QKey tắt")
    SetTimer(() => ToolTip(), -900)
}

ToggleMode() {
    global g_mode
    g_mode := g_mode = "telex" ? "vni" : "telex"
    ResetBuffer()
    UpdateTrayTip()
    ToolTip("QKey mode: " StrUpper(g_mode))
    SetTimer(() => ToolTip(), -900)
}

UpdateTrayTip() {
    global g_enabled, g_mode
    A_IconTip := "QKey - " (g_enabled ? "ON" : "OFF") " - " StrUpper(g_mode)
}

HandleTextKey(ThisHotkey) {
    global g_enabled, g_raw, g_rendered, g_mode
    if !g_enabled
        return

    key := RegExReplace(ThisHotkey, "^~\*", "")
    if StrLen(key) != 1
        return

    ; Let the physical key appear first, then replace current word if needed.
    SetTimer(() => ProcessTypedChar(key), -1)
}

ProcessTypedChar(ch) {
    global g_raw, g_rendered, g_mode

    if !IsWordChar(ch) {
        ResetBuffer()
        return
    }

    g_raw .= ch
    newRendered := ConvertWord(g_raw, g_mode)

    if g_rendered = "" {
        g_rendered := ch
    }

    if newRendered != g_rendered {
        ; Physical key was already inserted, so remove previous rendered word
        ; plus this just-typed key, then send the converted word.
        DeleteChars(StrLen(g_rendered) + 1)
        SendText(newRendered)
    }
    g_rendered := newRendered
}

HandleControlKey(ThisHotkey) {
    key := RegExReplace(ThisHotkey, "^~\*", "")
    if key = "Backspace" {
        global g_raw, g_rendered
        if StrLen(g_raw) > 0 {
            g_raw := SubStr(g_raw, 1, -1)
            g_rendered := ConvertWord(g_raw, g_mode)
        }
        return
    }
    if key = "Space" {
        TryExpandMacro()
    }
    ResetBuffer()
}

TryExpandMacro() {
    global g_raw, g_rendered, g_macros
    if g_raw = "" || !g_macros.Has(StrLower(g_raw))
        return
    replacement := g_macros[StrLower(g_raw)]
    DeleteChars(StrLen(g_rendered) + 1) ; rendered word + physical space
    SendText(replacement " ")
}

ResetBuffer() {
    global g_raw, g_rendered
    g_raw := ""
    g_rendered := ""
}

DeleteChars(n) {
    if n <= 0
        return
    Send("{Backspace " n "}")
}

IsWordChar(ch) {
    return RegExMatch(ch, "^[A-Za-z0-9]$")
}

ConvertWord(raw, mode) {
    return mode = "vni" ? ConvertVNI(raw) : ConvertTelex(raw)
}

; Engine ---------------------------------------------------------------------
ConvertTelex(raw) {
    out := ""
    for ch in StrSplit(raw) {
        low := StrLower(ch)
        if InStr("fsrxj", low) {
            mark := Map("f", "`", "s", "'", "r", "?", "x", "~", "j", ".")[low]
            out := ApplyTone(out, mark)
        } else if low = "w" {
            ; aw -> ă, ow -> ơ, uw -> ư, uow -> ươ
            if StrLen(out) >= 2 && BaseTone(SubStr(out, -2, 1)).base = "u" && BaseTone(SubStr(out, -1)).base = "o" {
                out := SubStr(out, 1, -2) AccentChar(SubStr(out, -2, 1), "ư") AccentChar(SubStr(out, -1), "ơ")
                continue
            }
            before := out
            out := ReplaceLastVowel(out, "a", "ă")
            if out = before
                out := ReplaceLastVowel(out, "o", "ơ")
            if out = before
                out := ReplaceLastVowel(out, "u", "ư")
            if out = before
                out .= CaseLike(ch, "ư")
        } else if low = "a" && StrLower(SubStr(out, -1)) = "a" {
            out := SubStr(out, 1, -1) CaseLike(SubStr(out, -1), "â")
        } else if low = "e" && StrLower(SubStr(out, -1)) = "e" {
            out := SubStr(out, 1, -1) CaseLike(SubStr(out, -1), "ê")
        } else if low = "o" && StrLower(SubStr(out, -1)) = "o" {
            out := SubStr(out, 1, -1) CaseLike(SubStr(out, -1), "ô")
        } else if low = "d" && StrLower(SubStr(out, -1)) = "d" {
            out := SubStr(out, 1, -1) CaseLike(SubStr(out, -1), "đ")
        } else {
            out .= ch
        }
    }
    return out
}

ConvertVNI(raw) {
    out := ""
    for ch in StrSplit(raw) {
        if InStr("12345", ch) {
            if ch = "6" {
                before := out
                out := ReplaceLastVowel(out, "a", "â")
                if out = before
                    out := ReplaceLastVowel(out, "e", "ê")
                if out = before
                    out := ReplaceLastVowel(out, "o", "ô")
                if out != before
                    continue
            }
            mark := Map("1", "'", "2", "`", "3", "?", "4", "~", "5", ".")[ch]
            out := ApplyTone(out, mark)
        } else if ch = "7" {
            before := out
            out := ReplaceLastVowel(out, "o", "ơ")
            if out = before
                out := ReplaceLastVowel(out, "u", "ư")
            if out = before
                out .= ch
        } else if ch = "8" {
            before := out
            out := ReplaceLastVowel(out, "a", "ă")
            if out = before
                out .= ch
        } else if ch = "9" {
            if StrLower(SubStr(out, -1)) = "d"
                out := SubStr(out, 1, -1) CaseLike(SubStr(out, -1), "đ")
            else
                out .= ch
        } else {
            out .= ch
        }
    }
    return out
}

ApplyTone(word, mark) {
    pos := TonePosition(word)
    if pos = 0
        return word mark
    tone := Map("`", 1, "'", 2, "?", 3, "~", 4, ".", 5)[mark]
    ch := SubStr(word, pos, 1)
    return SubStr(word, 1, pos - 1) AccentChar(ch, "", tone) SubStr(word, pos + 1)
}

TonePosition(word) {
    positions := []
    Loop Parse word {
        if IsVietnameseVowel(A_LoopField)
            positions.Push(A_Index)
    }
    if positions.Length = 0
        return 0

    for pos in positions {
        if InStr("êơôâăưÊƠÔÂĂƯ", SubStr(word, pos, 1))
            return pos
    }
    if positions.Length >= 2 {
        last := positions[positions.Length]
        lastCh := StrLower(SubStr(word, last, 1))
        if InStr("iyu", lastCh)
            return positions[positions.Length - 1]
        return positions.Length >= 3 ? positions[2] : positions[1]
    }
    return positions[1]
}

ReplaceLastVowel(word, candidates, targetBase) {
    Loop StrLen(word) {
        i := StrLen(word) - A_Index + 1
        ch := SubStr(word, i, 1)
        info := BaseTone(ch)
        if InStr(candidates, info.base) {
            return SubStr(word, 1, i - 1) AccentChar(ch, targetBase, info.tone) SubStr(word, i + 1)
        }
    }
    return word
}

AccentChar(ch, newBase := "", tone := -1) {
    info := BaseTone(ch)
    base := newBase != "" ? newBase : info.base
    t := tone >= 0 ? tone : info.tone
    chars := AccentTable(base)
    if !IsObject(chars)
        return ch
    return CaseLike(ch, chars[t + 1])
}

BaseTone(ch) {
    lower := StrLower(ch)
    for base in ["a", "ă", "â", "e", "ê", "i", "o", "ô", "ơ", "u", "ư", "y"] {
        chars := AccentTable(base)
        Loop chars.Length {
            if lower = chars[A_Index]
                return {base: base, tone: A_Index - 1}
        }
    }
    return {base: lower, tone: 0}
}

AccentTable(base) {
    switch base {
        case "a": return ["a", "à", "á", "ả", "ã", "ạ"]
        case "ă": return ["ă", "ằ", "ắ", "ẳ", "ẵ", "ặ"]
        case "â": return ["â", "ầ", "ấ", "ẩ", "ẫ", "ậ"]
        case "e": return ["e", "è", "é", "ẻ", "ẽ", "ẹ"]
        case "ê": return ["ê", "ề", "ế", "ể", "ễ", "ệ"]
        case "i": return ["i", "ì", "í", "ỉ", "ĩ", "ị"]
        case "o": return ["o", "ò", "ó", "ỏ", "õ", "ọ"]
        case "ô": return ["ô", "ồ", "ố", "ổ", "ỗ", "ộ"]
        case "ơ": return ["ơ", "ờ", "ớ", "ở", "ỡ", "ợ"]
        case "u": return ["u", "ù", "ú", "ủ", "ũ", "ụ"]
        case "ư": return ["ư", "ừ", "ứ", "ử", "ữ", "ự"]
        case "y": return ["y", "ỳ", "ý", "ỷ", "ỹ", "ỵ"]
    }
    return ""
}

IsVietnameseVowel(ch) {
    info := BaseTone(ch)
    return InStr("aăâeêioôơuưy", info.base) > 0
}

CaseLike(src, dst) {
    return RegExMatch(src, "^[A-ZĐ]$") ? StrUpper(dst) : dst
}
