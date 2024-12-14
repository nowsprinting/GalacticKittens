# Copyright (c) 2023-2024 Koji Hasegawa.

PROJECT_HOME?=$(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
BUILD_DIR?=$(PROJECT_HOME)/Builds
LOG_DIR?=$(PROJECT_HOME)/Logs
PRODUCT_NAME?=$(shell grep 'productName:' $(PROJECT_HOME)/ProjectSettings/ProjectSettings.asset | sed -e 's/.*productName: //')
UNITY_VERSION?=$(shell grep 'm_EditorVersion:' $(PROJECT_HOME)/ProjectSettings/ProjectVersion.txt | grep -o -E '\d{4}\.\d\.\d+[abfp]\d+')

UNAME := $(shell uname)
ifeq ($(UNAME), Darwin)
UNITY_HOME=/Applications/Unity/HUB/Editor/$(UNITY_VERSION)/Unity.app/Contents
UNITY?=$(UNITY_HOME)/MacOS/Unity
UNITY_YAML_MERGE?=$(UNITY_HOME)/Tools/UnityYAMLMerge
STANDALONE_PLAYER=StandaloneOSX
endif
ifeq ($(UNAME), Linux)  # not test yet
UNITY_HOME=$HOME/Unity/Hub/Editor/<version>
UNITY?=$(UNITY_HOME)/Unity
UNITY_YAML_MERGE?=$(UNITY_HOME)/ # unknown
STANDALONE_PLAYER=StandaloneLinux64
endif

.PHONY: usage
usage:
	@echo "Tasks:"
	@echo "  autopilot_tests: Run multiplayer autoplay tests in Play Mode tests."
	@echo "  autopilot_player: Build and run game with autopilot."

.PHONY: clean
clean:
	rm -rf $(BUILD_DIR)
	rm -rf $(LOG_DIR)

.PHONY: build
build:
	mkdir -p $(BUILD_DIR)
	mkdir -p $(LOG_DIR)
	$(UNITY) \
	  -projectPath $(PROJECT_HOME) \
	  -logFile $(LOG_DIR)/Editor_autopilot.log \
	  -batchmode \
	  -quit \
	  -buildOSXUniversalPlayer "$(BUILD_DIR)/$(PRODUCT_NAME).app"

.PHONY: autopilot_tests
autopilot_tests: build
	$(UNITY) \
	  -projectPath $(PROJECT_HOME) \
	  -logFile $(LOG_DIR)/playmode.log \
	  -batchmode \
	  -silent-crashes \
	  -stackTraceLogType Full \
	  -runTests \
	  -testPlatform playmode \
	  -testResults $(LOG_DIR)/playmode-results.xml

.PHONY: autopilot_player
autopilot_player: build
	mkdir -p $(LOG_DIR)/Host
	mkdir -p $(LOG_DIR)/Join
	"$(BUILD_DIR)/$(PRODUCT_NAME).app/Contents/MacOS/$(PRODUCT_NAME)" \
	  -screen-width 320 \
	  -screen-height 180 \
	  -LAUNCH_AUTOPILOT_SETTINGS Host/AutopilotSettings \
	  -OUTPUT_ROOT_DIRECTORY_PATH $(LOG_DIR)/Host &
	"$(BUILD_DIR)/$(PRODUCT_NAME).app/Contents/MacOS/$(PRODUCT_NAME)" \
	  -screen-width 320 \
	  -screen-height 180 \
	  -LAUNCH_AUTOPILOT_SETTINGS Join/AutopilotSettings \
	  -OUTPUT_ROOT_DIRECTORY_PATH $(LOG_DIR)/Join
