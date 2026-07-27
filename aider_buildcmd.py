#!/usr/bin/env python3
"""
aider_buildcmd.py

A minimal helper script for building and testing the RoslynGuardAnalyzer project.

Usage:
    python3 aider_buildcmd.py [command]

Commands:
    test        Run `dotnet test` for the solution.
    build       Run `dotnet build` for the solution.
    clean       Clean the solution with `dotnet clean`.

If no command is provided, the script defaults to `test`.
"""

import argparse
import subprocess
import sys
from pathlib import Path

def run_dotnet(command: str) -> int:
    """
    Executes a dotnet command in the repository root where the .sln file resides.

    Returns the exit code of the subprocess.
    """
    # The script lives in the task-factory root, but the actual solution is under
    # `workdir/roslyn-guard-analyzer`.  Compute the correct repository root.
    script_dir = Path(__file__).resolve().parent
    repo_root = script_dir / "workdir" / "roslyn-guard-analyzer"

    # Verify that the .sln file exists in the calculated root; if not, fall back
    # to the script directory (useful for future extensions).
    if not any(repo_root.glob("*.sln")):
        repo_root = script_dir

    # Ensure the .NET SDK is available.
    try:
        subprocess.run(
            ["dotnet", "--version"],
            check=True,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
    except (FileNotFoundError, subprocess.CalledProcessError):
        print(
            "Error: .NET SDK is not installed or not available in PATH.",
            file=sys.stderr,
        )
        return 1

    result = subprocess.run(
        ["dotnet", command],
        cwd=repo_root,
        stdout=sys.stdout,
        stderr=sys.stderr,
    )
    return result.returncode

def main() -> None:
    parser = argparse.ArgumentParser(
        description="Helper script to build/test the RoslynGuardAnalyzer project."
    )
    parser.add_argument(
        "command",
        nargs="?",
        default="test",
        choices=["test", "build", "clean"],
        help="Command to execute (default: test).",
    )
    args = parser.parse_args()

    exit_code = run_dotnet(args.command)
    sys.exit(exit_code)

if __name__ == "__main__":
    main()
