#!/usr/bin/env python3
import xml.etree.ElementTree as ET
import glob, sys, os

MIN_LINE   = float(os.environ.get("COVERAGE_MIN_LINE",   "80"))
MIN_BRANCH = float(os.environ.get("COVERAGE_MIN_BRANCH", "70"))

RESET = "\033[0m"; RED = "\033[31m"; GREEN = "\033[32m"; YELLOW = "\033[33m"; BOLD = "\033[1m"

def color(val, threshold):
    if val >= threshold:           return f"{GREEN}{val:.1f}%{RESET}"
    elif val >= threshold * 0.9:   return f"{YELLOW}{val:.1f}%{RESET}"
    else:                          return f"{RED}{val:.1f}%{RESET}"

files = glob.glob("./coverage/**/coverage.cobertura.xml", recursive=True)
if not files:
    print(f"{RED}No coverage XML found.{RESET}"); sys.exit(1)

root   = ET.parse(files[0]).getroot()
line   = float(root.attrib["line-rate"])   * 100
branch = float(root.attrib["branch-rate"]) * 100

print(f"\n{BOLD}─── Coverage Report ───────────────────────────{RESET}")
print(f"  Line   : {color(line,   MIN_LINE)}  (min {MIN_LINE:.0f}%)")
print(f"  Branch : {color(branch, MIN_BRANCH)}  (min {MIN_BRANCH:.0f}%)")
print(f"{BOLD}───────────────────────────────────────────────{RESET}\n")

failed = False
if line   < MIN_LINE:   print(f"{RED}✖ Line coverage too low{RESET}");   failed = True
if branch < MIN_BRANCH: print(f"{RED}✖ Branch coverage too low{RESET}"); failed = True
if not failed:          print(f"{GREEN}✔ Coverage passed{RESET}")

sys.exit(1 if failed else 0)