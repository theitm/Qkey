#!/usr/bin/env python3
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "src"))

from qkey_engine import QKeyEngine, convert_telex, convert_vni


def check(actual: str, expected: str) -> None:
    if actual != expected:
        raise AssertionError(f"expected {expected!r}, got {actual!r}")


def main() -> None:
    check(convert_telex("tieengs"), "tiếng")
    check(convert_telex("Vieetj"), "Việt")
    check(convert_telex("ddawng"), "đăng")
    check(convert_telex("hoaf"), "hòa")
    check(convert_telex("tuowng"), "tương")
    check(convert_telex("Tooi"), "Tôi")

    check(convert_vni("tie61ng"), "tiếng")
    check(convert_vni("Vie65t"), "Việt")
    check(convert_vni("d9a8ng"), "đăng")
    check(convert_vni("hoa2"), "hòa")
    check(convert_vni("tu7ng"), "tưng")

    check(QKeyEngine("telex").convert("tieengs Vieetj Nam"), "tiếng Việt Nam")
    check(QKeyEngine("vni").convert("tie61ng Vie65t Nam"), "tiếng Việt Nam")
    print("OK: QKey engine tests passed")


if __name__ == "__main__":
    main()
