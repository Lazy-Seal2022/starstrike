#!/usr/bin/env bash
set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_REPO="$REPO_ROOT/unity"
WIN_UNITY="/home/quan/window/c/Data/Unity/StarStrike"

if [ ! -d "$WIN_UNITY" ]; then
    echo "Error: Windows Unity directory not found at $WIN_UNITY"
    exit 1
fi

ACTION="${1:-pull}"

if [ "$ACTION" = "push" ]; then
    echo "==> Pushing changes from WSL repo ($UNITY_REPO) to Windows ($WIN_UNITY)..."
    rsync -av --delete \
        --exclude="Library/" \
        --exclude="Temp/" \
        --exclude="Logs/" \
        --exclude="UserSettings/" \
        --exclude="Build/" \
        --exclude="Builds/" \
        --exclude=".vs/" \
        --exclude=".idea/" \
        --exclude="*.csproj" \
        --exclude="*.sln" \
        "$UNITY_REPO/Assets/" "$WIN_UNITY/Assets/"

    rsync -av "$UNITY_REPO/Packages/" "$WIN_UNITY/Packages/"
    rsync -av "$UNITY_REPO/ProjectSettings/" "$WIN_UNITY/ProjectSettings/"
    echo "==> Push complete."

elif [ "$ACTION" = "pull" ]; then
    echo "==> Pulling changes from Windows ($WIN_UNITY) to WSL repo ($UNITY_REPO)..."
    rsync -av --delete \
        --exclude="Library/" \
        --exclude="Temp/" \
        --exclude="Logs/" \
        --exclude="UserSettings/" \
        --exclude="Build/" \
        --exclude="Builds/" \
        --exclude=".vs/" \
        --exclude=".idea/" \
        --exclude="*.csproj" \
        --exclude="*.sln" \
        "$WIN_UNITY/Assets/" "$UNITY_REPO/Assets/"

    rsync -av "$WIN_UNITY/Packages/" "$UNITY_REPO/Packages/"
    rsync -av "$WIN_UNITY/ProjectSettings/" "$UNITY_REPO/ProjectSettings/"
    echo "==> Pull complete."

else
    echo "Usage: $0 [push|pull]"
    exit 1
fi
