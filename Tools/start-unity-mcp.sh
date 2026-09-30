#!/bin/bash
# Launches Unity headless (no GUI window) with the MCP Unity bridge server enabled,
# so Claude Code can drive the Editor without Orlando needing to open Unity's own
# (not fully VoiceOver-accessible) window at all.
#
# Usage: ./Tools/start-unity-mcp.sh
# Stop it with: pkill -f "Unity.app/Contents/MacOS/Unity -batchmode.*EchoesOfNeon"

PROJECT_PATH="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
UNITY_VERSION="6000.6.0f1"
UNITY_BIN="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
LOG_FILE="${PROJECT_PATH}/Logs/unity-mcp-server.log"

mkdir -p "${PROJECT_PATH}/Logs"

if lsof -i :8090 >/dev/null 2>&1; then
  echo "Something is already listening on port 8090 - MCP Unity server may already be running."
  exit 0
fi

MCP_UNITY_ALLOW_BATCH_MODE=true "$UNITY_BIN" -batchmode -nographics \
  -projectPath "$PROJECT_PATH" -logFile "$LOG_FILE" &
disown

echo "Launched Unity (headless, MCP bridge enabled) - PID $!"
echo "Log: $LOG_FILE"
echo "It can take 1-2 minutes to finish importing/compiling before the bridge is ready."
echo "Check readiness with: lsof -i :8090"
