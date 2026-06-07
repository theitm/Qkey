#!/usr/bin/env python3
"""Core Vietnamese input conversion used to test QKey rules off Windows.

This is intentionally small and dependency-free. The AutoHotkey script mirrors
these tables and heuristics for the Windows runtime.
"""
from __future__ import annotations

from dataclasses import dataclass

TONE_ORDER = "`'?.~"
TONE_MARKS = {
    "`": 1,  # huyền
    "'": 2,  # sắc
    "?": 3,  # hỏi
    "~": 4,  # ngã
    ".": 5,  # nặng
}

BASE_TO_ACCENTED = {
    "a": ["a", "à", "á", "ả", "ã", "ạ"],
    "ă": ["ă", "ằ", "ắ", "ẳ", "ẵ", "ặ"],
    "â": ["â", "ầ", "ấ", "ẩ", "ẫ", "ậ"],
    "e": ["e", "è", "é", "ẻ", "ẽ", "ẹ"],
    "ê": ["ê", "ề", "ế", "ể", "ễ", "ệ"],
    "i": ["i", "ì", "í", "ỉ", "ĩ", "ị"],
    "o": ["o", "ò", "ó", "ỏ", "õ", "ọ"],
    "ô": ["ô", "ồ", "ố", "ổ", "ỗ", "ộ"],
    "ơ": ["ơ", "ờ", "ớ", "ở", "ỡ", "ợ"],
    "u": ["u", "ù", "ú", "ủ", "ũ", "ụ"],
    "ư": ["ư", "ừ", "ứ", "ử", "ữ", "ự"],
    "y": ["y", "ỳ", "ý", "ỷ", "ỹ", "ỵ"],
}
ACCENTED_TO_BASE: dict[str, tuple[str, int]] = {}
for base, chars in BASE_TO_ACCENTED.items():
    for tone, char in enumerate(chars):
        ACCENTED_TO_BASE[char] = (base, tone)

VOWELS = set(ACCENTED_TO_BASE)

TELEX_TONES = {"f": "`", "s": "'", "r": "?", "x": "~", "j": "."}
VNI_TONES = {"1": "'", "2": "`", "3": "?", "4": "~", "5": "."}


def _case_like(src: str, dst: str) -> str:
    if src.isupper():
        return dst.upper()
    if src[:1].isupper():
        return dst.upper()
    return dst


def _normalize_char(ch: str) -> tuple[str, int]:
    lower = ch.lower()
    if lower in ACCENTED_TO_BASE:
        return ACCENTED_TO_BASE[lower]
    return lower, 0


def _accent_char(ch: str, new_base: str | None = None, tone: int | None = None) -> str:
    base, old_tone = _normalize_char(ch)
    base = new_base or base
    tone = old_tone if tone is None else tone
    out = BASE_TO_ACCENTED.get(base, [base])[tone]
    return _case_like(ch, out)


def _vowel_positions(word: str) -> list[int]:
    return [i for i, ch in enumerate(word) if ch.lower() in VOWELS]


def _tone_position(word: str) -> int | None:
    positions = _vowel_positions(word)
    if not positions:
        return None
    lowered = word.lower()
    # Prefer ê/ơ/ô/â/ă/ư when present.
    for i in positions:
        if lowered[i] in "êơôâăư":
            return i
    # For vowel clusters, Vietnamese orthography usually places tone on the
    # main vowel; this heuristic covers common MVP cases.
    if len(positions) >= 2:
        last = positions[-1]
        if lowered[last] in "iyu" and last == positions[0] + len(positions) - 1:
            return positions[-2]
        return positions[1 if len(positions) >= 3 else 0]
    return positions[0]


def apply_tone(word: str, tone_mark: str) -> str:
    pos = _tone_position(word)
    if pos is None:
        return word + tone_mark
    tone = TONE_MARKS[tone_mark]
    return word[:pos] + _accent_char(word[pos], tone=tone) + word[pos + 1 :]


def _replace_last_vowel(word: str, candidates: str, target: str) -> str:
    for i in range(len(word) - 1, -1, -1):
        base, tone = _normalize_char(word[i])
        if base in candidates:
            return word[:i] + _accent_char(word[i], new_base=target, tone=tone) + word[i + 1 :]
    return word


def convert_telex(raw: str) -> str:
    out = ""
    for ch in raw:
        low = ch.lower()
        if low in TELEX_TONES:
            out = apply_tone(out, TELEX_TONES[low])
        elif low == "w":
            # aw -> ă, ow -> ơ, uw -> ư, uow -> ươ, standalone w -> ư
            if len(out) >= 2 and _normalize_char(out[-2])[0] == "u" and _normalize_char(out[-1])[0] == "o":
                out = out[:-2] + _accent_char(out[-2], new_base="ư") + _accent_char(out[-1], new_base="ơ")
                continue
            before = out
            out = _replace_last_vowel(out, "a", "ă")
            if out == before:
                out = _replace_last_vowel(out, "o", "ơ")
            if out == before:
                out = _replace_last_vowel(out, "u", "ư")
            if out == before:
                out += _case_like(ch, "ư")
        elif low == "a" and out[-1:].lower() == "a":
            out = out[:-1] + _case_like(out[-1], "â")
        elif low == "e" and out[-1:].lower() == "e":
            out = out[:-1] + _case_like(out[-1], "ê")
        elif low == "o" and out[-1:].lower() == "o":
            out = out[:-1] + _case_like(out[-1], "ô")
        elif low == "d" and out[-1:].lower() == "d":
            out = out[:-1] + _case_like(out[-1], "đ")
        else:
            out += ch
    return out


def convert_vni(raw: str) -> str:
    out = ""
    for ch in raw:
        if ch in VNI_TONES:
            out = apply_tone(out, VNI_TONES[ch])
        elif ch == "6":
            before = out
            out = _replace_last_vowel(out, "a", "â")
            if out == before:
                out = _replace_last_vowel(out, "e", "ê")
            if out == before:
                out = _replace_last_vowel(out, "o", "ô")
            if out == before:
                out = apply_tone(out, ".")
        elif ch == "7":
            before = out
            out = _replace_last_vowel(out, "o", "ơ")
            if out == before:
                out = _replace_last_vowel(out, "u", "ư")
            if out == before:
                out += ch
        elif ch == "8":
            before = out
            out = _replace_last_vowel(out, "a", "ă")
            if out == before:
                out += ch
        elif ch == "9":
            if out[-1:].lower() == "d":
                out = out[:-1] + _case_like(out[-1], "đ")
            else:
                out += ch
        else:
            out += ch
    return out


@dataclass
class QKeyEngine:
    mode: str = "telex"

    def convert(self, raw: str) -> str:
        if self.mode == "vni":
            return convert_vni(raw)
        return convert_telex(raw)


if __name__ == "__main__":
    import sys

    mode = sys.argv[1] if len(sys.argv) > 1 else "telex"
    text = sys.argv[2] if len(sys.argv) > 2 else "tieengs Vieetj Nam"
    print(QKeyEngine(mode).convert(text))
